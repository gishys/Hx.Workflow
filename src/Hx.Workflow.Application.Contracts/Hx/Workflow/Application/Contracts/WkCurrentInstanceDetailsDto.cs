using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCurrentInstanceDetailsDto
    {
        public Guid Id { get; set; }
        public Guid DefinitionId { get; set; }
        public string Reference { get; set; }
        public string RegistrationCategory { get; set; }
        public string Receiver { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public WkExecutionPointerDto CurrentExecutionPointer { get; set; }
        public ICollection<WkAuditorDto> WkAuditors {  get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public string ProcessType { get; set; }
        /// <summary>
        /// 可办理
        /// </summary>
        public bool CanHandle {  get; set; }
        /// <summary>
        /// 业务数据
        /// </summary>
        public IDictionary<string,object> Data { get; set; }
    }
}