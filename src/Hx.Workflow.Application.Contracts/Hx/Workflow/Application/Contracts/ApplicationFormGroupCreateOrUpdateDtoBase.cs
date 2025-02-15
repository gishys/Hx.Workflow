using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormGroupCreateOrUpdateDtoBase
    {
        /// <summary>
        /// 分组标题
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }
    }
}
