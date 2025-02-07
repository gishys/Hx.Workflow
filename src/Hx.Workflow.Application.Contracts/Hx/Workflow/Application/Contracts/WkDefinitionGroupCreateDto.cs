using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionGroupCreateDto : WkDefinitionGroupCreateOrUpdateDtoBase
    {
        /// <summary>
        /// 父Id
        /// </summary>
        public Guid? ParentId { get; set; }
    }
}
