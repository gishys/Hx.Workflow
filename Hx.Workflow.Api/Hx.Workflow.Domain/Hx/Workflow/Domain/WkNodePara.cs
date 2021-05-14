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
        public WkNodePara()
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