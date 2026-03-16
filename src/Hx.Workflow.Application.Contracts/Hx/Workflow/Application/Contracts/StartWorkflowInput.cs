using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class StartWorkflowInput
    {
        public required string Id { get; set; }
        public int Version { get; set; }
        /// <summary>
        /// 外部传入的受理编号（业务编号），作为一等公民使用。
        /// 如果不提供，则由系统自动生成。
        /// </summary>
        public string? Reference { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = [];
    }
}
