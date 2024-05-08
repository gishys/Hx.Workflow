using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace Hx.Workflow.Domain
{
    public class WkNodeInstance_Obsolete
    {
        /// <summary>
        /// 业务号
        /// </summary>
        public string BusinessNumber { get; protected set; }
        /// <summary>
        /// 受理时间
        /// </summary>
        public DateTime AcceptTime { get; protected set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? SubmitTime { get; protected set; }
        /// <summary>
        /// 步骤实例状态
        /// </summary>
        public StepInstanceState StepInstanceState { get; protected set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int LimitTime { get; protected set; }
        /// <summary>
        /// 步骤
        /// </summary>
        public WkNode Node { get; protected set; }
        /// <summary>
        /// 用户
        /// </summary>
        public IdentityUser User { get; protected set; }
        /// <summary>
        /// 表单集合
        /// </summary>
        protected virtual ICollection<ApplicationForm> ApplicationForms { get; set; }
        public WkNodeInstance_Obsolete()
        { }
        public WkNodeInstance_Obsolete(
            string businessNumber,
            DateTime acceptTime,
            StepInstanceState stepInstanceState,
            WkNode node,
            IdentityUser user,
            int limitTime)
        {
            BusinessNumber = businessNumber;
            AcceptTime = acceptTime;
            StepInstanceState = stepInstanceState;
            Node = node;
            User = user;
            LimitTime = limitTime;
        }
        public Task SetStepInstanceState(StepInstanceState stepInstanceState)
        {
            StepInstanceState = stepInstanceState;
            return Task.CompletedTask;
        }
        public Task SetSubmitTime(DateTime submitTime)
        {
            SubmitTime = submitTime;
            return Task.CompletedTask;
        }
    }
}
