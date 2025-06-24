using Castle.Core.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicCompilationSecurityPolicy(
        ILogger<DynamicCompilationSecurityPolicy> logger,
        IConfiguration configuration,
        DynamicExecutionOptions options) : ITransientDependency
    {
        private readonly ILogger<DynamicCompilationSecurityPolicy> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DynamicExecutionOptions _options = options;

        public async Task ValidateCodeAsync(string code)
        {
            try
            {
                // 1. 检查禁止的关键字
                await CheckForbiddenKeywordsAsync(code);

                // 2. 检查代码长度
                CheckCodeLength(code);

                // 3. 检查类型声明限制
                CheckClassDeclarations(code);

                // 4. 检查必需的安全属性
                CheckRequiredAttributes(code);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Dynamic code validation failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task CheckForbiddenKeywordsAsync(string code)
        {
            var forbiddenKeywords = _options.ForbiddenKeywords;

            // 使用并行检查提高性能
            var tasks = forbiddenKeywords.Select(keyword =>
                Task.Run(() =>
                {
                    if (code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new AbpException($"Forbidden keyword detected: {keyword}");
                    }
                }));

            await Task.WhenAll(tasks);
        }

        private void CheckCodeLength(string code)
        {
            if (code.Length > _options.MaxCodeLength)
            {
                throw new AbpException($"Code length exceeds limit: {code.Length} > {_options.MaxCodeLength}");
            }
        }

        private void CheckClassDeclarations(string code)
        {
            var classCount = Regex.Matches(code, @"\bclass\b").Count;
            if (classCount > _options.MaxClasses)
            {
                throw new AbpException($"Class declarations exceed limit: {classCount} > {_options.MaxClasses}");
            }
        }

        private void CheckRequiredAttributes(string code)
        {
            if (!code.Contains("[AbpDynamicDependency]"))
            {
                throw new AbpException("Dynamic classes must be marked with [AbpDynamicDependency]");
            }
        }
    }

    public class DynamicExecutionOptions
    {
        public int MaxCodeLength { get; set; } = 10000;
        public int MaxClasses { get; set; } = 3;
        public List<string> ForbiddenKeywords { get; set; } =
        [
            "System.IO", "System.Net", "System.Reflection",
            "System.Diagnostics", "System.Runtime", "DllImport",
            "unsafe", "Marshal", "Environment.Exit"
        ];
        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
