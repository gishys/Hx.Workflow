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
        public DateTime BusinessCommitmentDeadline { get; set; }
        public WkExecutionPointerDto CurrentExecutionPointer { get; set; }
        public string ProcessName { get; set; }
        public string Located { get; set; }
        /// <summary>
        /// 正在运行
        /// </summary>
        public bool IsProcessed { get; set; }
        /// <summary>
        /// 可办理
        /// </summary>
        public bool CanHandle {  get; set; }
    }
}