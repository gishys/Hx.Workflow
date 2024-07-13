using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public static class WorkflowShareExtensions
    {
        public static IDictionary<string, object> Cancat(this IDictionary<string, object> inputs, IDictionary<string, object> objs)
        {
            var workflowData = inputs == null ? new Dictionary<string, object>() : new Dictionary<string, object>(inputs);
            foreach (var item in objs)
            {
                if (!workflowData.ContainsKey(item.Key))
                {
                    workflowData.Add(item.Key, item.Value);
                }
            }
            return workflowData;
        }
    }
}
