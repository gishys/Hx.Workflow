using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCurrentInstanceDetailsDto
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid DefinitionId { get; set; }
        public required string Reference { get; set; }
        public required string RegistrationCategory { get; set; }
        public string? Receiver { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public WkExecutionPointerDto? CurrentExecutionPointer { get; set; }
        public required ICollection<WkAuditorDto> WkAuditors {  get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public required string BusinessType { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public required string ProcessType { get; set; }
        /// <summary>
        /// 可办理
        /// </summary>
        public bool CanHandle {  get; set; }
        /// <summary>
        /// 业务数据
        /// </summary>
        public IDictionary<string,object>? Data { get; set; }
    }
}
