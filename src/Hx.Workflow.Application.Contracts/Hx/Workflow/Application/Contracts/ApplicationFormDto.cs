using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using Volo.Abp.Data;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormDto
    {
        /// <summary>
        /// 应用数据
        /// </summary>
        public string? Data { get; set; }
        /// <summary>
        /// 应用组件类型
        /// </summary>
        public ApplicationComponentType ApplicationComponentType { get; set; }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public ExtraPropertyDictionary ExtraProperties { get; set; }
        /// <summary>
        /// application name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// application node type
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// application type
        /// </summary>
        public ApplicationType ApplicationType { get; set; }
        public virtual ICollection<WkParamDto> Params { get; set; }
    }
}
