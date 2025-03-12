using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hx.Workflow.Application.Contracts
{
    public class StartStepBodyChangeEvent
    {
        public StartStepBodyChangeEvent(
            Guid workflowInstanceId,
            string reference,
            IDictionary<string, object> data
            )
        {
            WorkflowInstanceId = workflowInstanceId;
            Reference = reference;
            Data = data;
        }
        /// <summary>
        /// 流程实例Id
        /// </summary>
        public Guid WorkflowInstanceId { get; set; }
        /// <summary>
        /// 业务关联编号
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 流程实例数据
        /// </summary>
        public IDictionary<string, object> Data { get; set; }
    }
}
