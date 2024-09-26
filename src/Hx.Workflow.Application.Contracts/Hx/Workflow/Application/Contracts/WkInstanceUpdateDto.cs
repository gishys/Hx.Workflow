using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkInstanceUpdateDto
    {
        public Guid WkInstanceId { get; set; }
        public Guid WkExecutionPointerId { get; set; }
        /// <summary>
        /// 执行操作类型
        /// </summary>
        public ExePersonnelOperateType ExeOperateType { get; set; }
        public List<Guid> WkCandidates { get; set; }
    }
}
