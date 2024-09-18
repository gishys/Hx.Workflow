using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeTreeDto
    {
        public string Title { get; set; }
        public Guid Key { get; set; }
        public bool Selected { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 接收人名称
        /// </summary>
        public string Receiver { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public DateTime? SignInTime { get; set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? CommitmentDeadline { get; set; }
        public int Status { get; set; }
        /// <summary>
        /// 办理人信息
        /// </summary>
        public ICollection<WkPointerCandidateDto> WkCandidates { get; set; }
    }
}