﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkProcessInstanceDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 受理编号
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 办理步骤
        /// </summary>
        public string ProcessingStepName { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        public string Recipient { get; set; }
        /// <summary>
        /// 提交人
        /// </summary>
        public string Submitter { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceivingTime { get; set; }
        /// <summary>
        /// 步骤期限
        /// </summary>
        public string StepCommitmentDeadline { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public string ProcessType { get; set; }
        /// <summary>
        /// 是否已签收
        /// </summary>
        public bool IsSign {  get; set; }
        /// <summary>
        /// 正在运行
        /// </summary>
        public bool IsProcessed {  get; set; }
        /// <summary>
        /// 业务数据
        /// </summary>
        public IDictionary<string, object> Data { get; set; }
    }
}