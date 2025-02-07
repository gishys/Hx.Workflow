using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class ProcessingStatusStatDto
    {
        /// <summary>
        /// 办理人
        /// </summary>
        public Guid? TransactorId { get; set; }
        /// <summary>
        /// 办理状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }
}
