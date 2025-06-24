using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicTypeLoader(
        ILogger<DynamicTypeLoader> logger,
        DynamicCompilationSecurityPolicy securityPolicy) : IDisposable
    {
        private readonly ILogger<DynamicTypeLoader> _logger = logger;
        private readonly DynamicCompilationSecurityPolicy _securityPolicy = securityPolicy;
        private readonly ConcurrentDictionary<string, (Assembly Assembly, List<Type> Types)> _cache = new();
        private readonly List<WeakReference> _loadedAssemblies = new();

        public async Task<(Assembly Assembly, List<Type> Types)> LoadTypesFromCodeAsync(string classCode)
        {
            var cacheKey = GenerateCacheKey(classCode);

            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                _logger.LogDebug("Using cached dynamic assembly");
                return cached;
            }

            await _securityPolicy.ValidateCodeAsync(classCode);

            // 创建语法树
            var syntaxTree = CSharpSyntaxTree.ParseText(classCode);

            // 获取所有必需的引用
            var references = GetRequiredReferences();

            // 创建编译
            var compilation = CSharpCompilation.Create(
                $"DynamicAssembly_{Guid.NewGuid():N}",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            // 编译到内存
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                var errors = string.Join("\n",
                    emitResult.Diagnostics.Select(d => d.ToString()));
                throw new AbpException($"Dynamic compilation failed:\n{errors}");
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // 获取标记类型
            var dynamicTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<AbpDynamicDependencyAttribute>() != null)
                .ToList();

            if (dynamicTypes.Count == 0)
            {
                throw new AbpException("No types marked with [AbpDynamicDependency] found");
            }

            // 缓存结果
            _cache.TryAdd(cacheKey, (assembly, dynamicTypes));
            _loadedAssemblies.Add(new WeakReference(assembly));

            return (assembly, dynamicTypes);
        }

        public void UnloadUnusedAssemblies()
        {
            var toRemove = new List<WeakReference>();

            foreach (var reference in _loadedAssemblies)
            {
                if (!reference.IsAlive)
                {
                    toRemove.Add(reference);
                }
            }

            foreach (var reference in toRemove)
            {
                _loadedAssemblies.Remove(reference);

                // 从缓存中移除 - 修复CS0253警告
                var targetAssembly = reference.Target as Assembly;
                var cacheKey = _cache.FirstOrDefault(x => x.Value.Assembly == targetAssembly).Key;

                if (cacheKey != null)
                {
                    _cache.TryRemove(cacheKey, out _);
                }
            }

            // 建议GC收集
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Dispose()
        {
            UnloadUnusedAssemblies();
        }

        private string GenerateCacheKey(string code)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private List<MetadataReference> GetRequiredReferences()
        {
            var referencePaths = new List<string>();
            var referencedAssemblies = new HashSet<Assembly>
            {
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(ValueTask).Assembly,
                typeof(AbpDynamicDependencyAttribute).Assembly,
                Assembly.GetEntryAssembly()!
            };

            // 添加ABP核心程序集
            var abpCoreAssembly = typeof(AbpApplicationBase).Assembly;
            referencedAssemblies.Add(abpCoreAssembly);

            // 添加所有直接引用的程序集
            foreach (var assembly in referencedAssemblies)
            {
                referencePaths.Add(assembly.Location);

                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        var loaded = Assembly.Load(reference);
                        referencePaths.Add(loaded.Location);
                    }
                    catch
                    {
                        // 忽略无法加载的程序集
                    }
                }
            }

            // 去重并创建元数据引用 - 修复CS0029错误
            return referencePaths
                .Distinct()
                .Where(File.Exists)
                .Select(r => (MetadataReference)MetadataReference.CreateFromFile(r))
                .ToList();
        }
    }
}