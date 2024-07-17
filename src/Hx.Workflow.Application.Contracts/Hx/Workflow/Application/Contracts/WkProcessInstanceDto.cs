using System;
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
        /// 预警
        /// </summary>
        public string EarlyWarning { get; set; }
        /// <summary>
        /// 受理编号
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 业务名称
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// 坐落
        /// </summary>
        public string Located { get; set; }
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
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 业务限定日期
        /// </summary>
        public DateTime BusinessCommitmentDeadline { get; set; }
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
        /// 可办理
        /// </summary>
        public bool CanHandle { get; set; }
    }
}