using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkParam
    {
        public string WkParamKey { get; protected set; }
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Value { get; protected set; }
        public WkParam(
            string wkParamKey,
            string name,
            string displayName,
            string value)
        {
            WkParamKey = wkParamKey;
            Name = name;
            DisplayName = displayName;
            Value = value;
        }
    }
}
