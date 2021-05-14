using System.Collections.Generic;

namespace Hx.Workflow.Domain.JsonDefinition
{
    public class JStepSource1
    {
        public JStepSource1()
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
