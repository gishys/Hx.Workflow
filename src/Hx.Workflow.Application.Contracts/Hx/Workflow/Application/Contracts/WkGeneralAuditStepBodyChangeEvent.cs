﻿using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkGeneralAuditStepBodyChangeEvent
    {
        public WkGeneralAuditStepBodyChangeEvent(Guid workflowInstanceId, string reference, IDictionary<string, object> data)
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
