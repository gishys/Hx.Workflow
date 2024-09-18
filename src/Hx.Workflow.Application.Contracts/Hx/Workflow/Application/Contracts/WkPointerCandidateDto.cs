using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkPointerCandidateDto : WkCandidateDto
    {
        /// <summary>
        /// 执行者类型
        /// </summary>
        public ExeCandidateType CandidateType { get; set; }
        /// <summary>
        /// 执行者状态
        /// </summary>
        public ExeCandidateState ParentState { get; set; }
        /// <summary>
        /// 关注
        /// </summary>
        public bool? Follow { get; set; }
    }
}
