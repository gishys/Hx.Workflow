using System.Collections.Generic;

namespace Hx.Workflow.Domain.JsonDefinition
{
    public class JStepSource1
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public JStepSource1()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            Inputs = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            OutcomeSteps = new Dictionary<string, object>();
        }
        public string Id { get; set; }
        public string StepType { get; set; }
        public string Name { get; set; }
        public IDictionary<string, object> Inputs { get; set; }
        public IDictionary<string, object> Outputs { get; set; }
        public IDictionary<string, object> OutcomeSteps { get; set; }
    }
}
