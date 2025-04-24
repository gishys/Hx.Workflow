using Hx.Workflow.Domain.Repositories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicCodeExecutor
    {
        // Cache for resolved assemblies to avoid repeated loading
        private static readonly Dictionary<string, Assembly> _assemblyCache = [];

        // Core assemblies that should always be included
        private static readonly string[] _coreAssemblies =
        [
            "System.Private.CoreLib",
            "System.Runtime",
            "netstandard"
        ];

        public Assembly CompileCode(string code)
        {
            // Parse the code to get using directives
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            // Get all using directives from the code
            var usingDirectives = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(u => u.Name.ToString())
                .ToList();

            // Add default references that are always needed
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ITransientDependency).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ILocalEventBus).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IWkInstanceRepository).Assembly.Location),
            };
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) references.Add(MetadataReference.CreateFromFile(entryAssembly.Location));
            // Add dynamically resolved references from using directives
            foreach (var usingDirective in usingDirectives)
            {
                try
                {
                    var assembly = ResolveAssemblyFromUsingDirective(usingDirective);
                    if (assembly != null && !references.Any(r => r.Display?.EndsWith(assembly.GetName().Name + ".dll") == true))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                }
                catch
                {
                    // Skip if we can't resolve the assembly
                    continue;
                }
            }

            // Add any additional assemblies that might be indirectly referenced
            AddIndirectReferences(references);

            // Configure compilation options
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            // Compile the code
            var compilation = CSharpCompilation.Create(
                "StartStepBody",
                [syntaxTree],
                references,
                compilationOptions);

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                var errors = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.GetMessage());
                throw new InvalidOperationException($"Compilation errors:\n{string.Join("\n", errors)}");
            }

            ms.Seek(0, SeekOrigin.Begin);
            return AssemblyLoadContext.Default.LoadFromStream(ms);
        }

        private static Assembly? ResolveAssemblyFromUsingDirective(string usingDirective)
        {
            // Check cache first
            if (_assemblyCache.TryGetValue(usingDirective, out var cachedAssembly))
            {
                return cachedAssembly;
            }

            // Try to get the assembly from the type name
            var typeName = ConvertUsingToTypeName(usingDirective);
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var assembly = type.Assembly;
                _assemblyCache[usingDirective] = assembly;
                return assembly;
            }

            // Try to load assembly by name
            var assemblyName = GetAssemblyNameFromUsing(usingDirective);
            if (!string.IsNullOrEmpty(assemblyName))
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    _assemblyCache[usingDirective] = assembly;
                    return assembly;
                }
                catch
                {
                    // Try alternative loading methods
                }
            }

            // Try to find the assembly in the current domain
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetTypes().Any(t => t.Namespace == usingDirective))
                {
                    _assemblyCache[usingDirective] = assembly;
                    return assembly;
                }
            }

            return null;
        }

        private static string ConvertUsingToTypeName(string usingDirective)
        {
            // Simple conversion - assumes the namespace has at least one public type
            // with the same name as the last part of the namespace
            var parts = usingDirective.Split('.');
            return usingDirective + "." + parts[^1];
        }

        private static string GetAssemblyNameFromUsing(string usingDirective)
        {
            // Heuristic to convert namespace to likely assembly name
            var parts = usingDirective.Split('.');

            // Handle common patterns
            if (usingDirective.StartsWith("System."))
            {
                if (usingDirective.StartsWith("System.Collections."))
                    return "System.Collections";
                if (usingDirective.StartsWith("System.Linq."))
                    return "System.Linq";
                return "System.Runtime";
            }

            // Try the first part of the namespace
            return parts[0];
        }

        private static void AddIndirectReferences(List<MetadataReference> references)
        {
            // Add core assemblies that might not be explicitly referenced
            foreach (var assemblyName in _coreAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    if (!references.Any(r => r.Display?.EndsWith(assembly.GetName().Name + ".dll") == true))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                }
                catch
                {
                    continue;
                }
            }

            // Add assemblies from types used in the default references
            var defaultReferences = references.ToList();
            foreach (var reference in defaultReferences)
            {
                try
                {
                    if (reference?.Display == null) continue;
                    var assembly = Assembly.LoadFrom(reference.Display);
                    foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                    {
                        try
                        {
                            var loadedAssembly = Assembly.Load(referencedAssembly);
                            if (!references.Any(r => r.Display?.EndsWith(loadedAssembly.GetName().Name + ".dll") == true))
                            {
                                references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}