using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormUpdateDto : ExtensibleObject
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 应用数据
        /// </summary>
        public string? Data { get; set; }
        /// <summary>
        /// 应用组件类型
        /// </summary>
        public ApplicationComponentType ApplicationComponentType { get; set; }
        /// <summary>
        /// application name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// application node type
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// application type
        /// </summary>
        public ApplicationType ApplicationType { get; set; } = ApplicationType.Form;
        public ICollection<WkParamCreateDto> Params { get; set; }
    }
}
