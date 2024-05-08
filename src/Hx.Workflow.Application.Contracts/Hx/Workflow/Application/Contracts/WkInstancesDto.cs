using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkInstancesDto
    {
        /// <summary>
        /// 流程名称
        /// </summary>
        public string WkDefinitionName { get; set; }
        /// <summary>
        /// 当前步骤名称
        /// </summary>
        public string CurrentStepName { get; set; }
        /// <summary>
        /// 流程状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 流程创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 流程环节开始时间
        /// </summary>
        public DateTime? StepStartTime { get; set; }
        /// <summary>
        /// 流程结束时间
        /// </summary>
        public DateTime CompleteTime { get; set; }
        /// <summary>
        /// 流程描述
        /// </summary>
        public string Description { get; set; }
    }
}
