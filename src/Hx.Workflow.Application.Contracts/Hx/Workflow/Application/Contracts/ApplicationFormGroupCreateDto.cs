using System;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormGroupCreateDto : ApplicationFormGroupCreateOrUpdateDtoBase
    {
        /// <summary>
        /// 父Id
        /// </summary>
        public Guid? ParentId { get; set; }
    }
}
