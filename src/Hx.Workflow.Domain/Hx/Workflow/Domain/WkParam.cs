using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkParam : Entity<Guid>
    {
        public Guid WkNodeId { get; protected set; }
        public string WkParamKey { get; protected set; }
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Value { get; protected set; }
        public WkParam(
            Guid id,
            string wkParamKey,
            string name,
            string displayName,
            string value)
        {
            Id = id;
            WkParamKey = wkParamKey;
            Name = name;
            DisplayName = displayName;
            Value = value;
        }
    }
}
