using Hx.Workflow.Domain.Shared;
using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNodePara : Entity<Guid>, IHxKeyValueConvert
    {
        public Guid WkNodeId { get; protected set; }
        public string Key { get; protected set; }
        public string Value { get; protected set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNodePara()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public WkNodePara(
            string key,
            string value)
        {
            Key = key;
            Value = value;
        }
    }
}