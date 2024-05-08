using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain.JsonDefinition
{
    public class JStepSource
    {
        public JStepSource()
        {
            Inputs = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            SelectNextStep = new Dictionary<string, object>();
            //OutcomeSteps = new Dictionary<string, object>();
        }
        public string Id { get; set; }
        public string StepType { get; set; }
        public string Name { get; set; }
        public string NextStepId { get; set; }
        public IDictionary<string, object> Inputs { get; set; }
        public IDictionary<string, object> Outputs { get; set; }
        public IDictionary<string, object> SelectNextStep { get; set; }
        //public IDictionary<string, object> OutcomeSteps { get; set; }
    }
}
