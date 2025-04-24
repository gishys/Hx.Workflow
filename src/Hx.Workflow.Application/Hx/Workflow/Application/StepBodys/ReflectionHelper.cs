using System;
using System.Collections.Generic;
using System.Reflection;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class ReflectionHelper
    {
        public class TypeMethodInfo
        {
            public required string TypeFullName { get; set; }
            public required string AssemblyFullName { get; set; }
            public required string Name { get; set; }
            public required string DisplayName { get; set; }
        }

        public static List<TypeMethodInfo> GetStepBodyAsyncDerivatives()
        {
            var result = new List<TypeMethodInfo>();

            // 获取当前应用程序域中所有已加载的程序集
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            // 过滤条件：是类、非抽象、继承自 StepBodyAsync
                            if (type.IsClass &&
                                !type.IsAbstract &&
                                type.IsSubclassOf(typeof(StepBodyAsync)))
                            {
                                // 预先获取所有必需的值
                                var typeFullName = type.FullName;
                                var assemblyFullName = assembly.FullName;
                                var nameField = GetConstStringField(type, "Name");
                                var displayNameField = GetConstStringField(type, "DisplayName");

                                // 获取字段值（安全访问）
                                var nameValue = nameField?.GetValue(null)?.ToString();
                                var displayNameValue = displayNameField?.GetValue(null)?.ToString();

                                // 统一验证所有必需字段
                                if (string.IsNullOrEmpty(typeFullName) ||
                                    string.IsNullOrEmpty(assemblyFullName) ||
                                    string.IsNullOrEmpty(nameValue) ||
                                    string.IsNullOrEmpty(displayNameValue))
                                {
                                    continue;
                                }

                                // 构建对象（已确保非空）
                                var typeInfo = new TypeMethodInfo
                                {
                                    TypeFullName = typeFullName!,
                                    AssemblyFullName = assemblyFullName!,
                                    Name = nameValue!,
                                    DisplayName = displayNameValue!
                                };
                                result.Add(typeInfo);
                            }
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            // 处理类型加载异常
                            continue;
                        }
                    }
                }
                catch (Exception ex) when (ex is ReflectionTypeLoadException || ex is NotSupportedException)
                {
                    // 处理程序集加载异常
                    continue;
                }
            }
            return result;
        }
        public static FieldInfo? GetConstStringField(Type type, string fieldName)
        {
            return type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);
        }

    }
}
