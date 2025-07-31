using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode_ApplicationForms : Entity
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNode_ApplicationForms() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNode_ApplicationForms(Guid applicationId, int sequenceNumber, ICollection<WkParam> ps, int version = 1)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            ApplicationId = applicationId;
            SequenceNumber = sequenceNumber;
            Params = ps;
            Version = version;
        }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNode_ApplicationForms(Guid nodeId, Guid applicationId, int version = 1)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            NodeId = nodeId;
            ApplicationId = applicationId;
            Version = version;
        }
        public virtual Guid NodeId { get; protected set; }
        public virtual Guid ApplicationId { get; protected set; }
        public virtual ApplicationForm ApplicationForm { get; protected set; }
        public virtual int SequenceNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = [];
        /// <summary>
        /// 版本号
        /// </summary>
        public virtual int Version { get; protected set; }
        public override object?[] GetKeys()
        {
            return [NodeId, ApplicationId, Version];
        }
    }
}
