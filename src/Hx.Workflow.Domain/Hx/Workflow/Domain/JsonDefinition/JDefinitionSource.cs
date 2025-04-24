using System.Collections.Generic;

namespace Hx.Workflow.Domain.JsonDefinition
{
    public class JDefinitionSource
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public JDefinitionSource()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            Steps = [];
        }
        public string Id { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public List<JStepSource> Steps { get; set; }
    }
}