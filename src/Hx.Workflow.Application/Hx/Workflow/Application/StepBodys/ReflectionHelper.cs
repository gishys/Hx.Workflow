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
            public string TypeFullName { get; set; }
            public string AssemblyFullName { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
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
                                var typeInfo = new TypeMethodInfo
                                {
                                    TypeFullName = type.FullName,
                                    AssemblyFullName = assembly.FullName,
                                    Name = GetConstStringField(type, "Name")?.GetValue(null)?.ToString(),
                                    DisplayName = GetConstStringField(type, "DisplayName")?.GetValue(null)?.ToString(),
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
        public static FieldInfo GetConstStringField(Type type, string fieldName)
        {
            return type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);
        }

    }
}
