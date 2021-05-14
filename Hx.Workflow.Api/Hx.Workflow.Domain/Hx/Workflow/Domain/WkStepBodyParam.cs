using Hx.Workflow.Domain.Shared;
using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkStepBodyParam : Entity<Guid>, IHxKeyValueConvert
    {
        public Guid WkNodeId { get; protected set; }
        public string Key { get; protected set; }
        public StepBodyParaType StepBodyParaType { get; protected set; }
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Value { get; protected set; }
        public WkStepBodyParam(
            Guid id,
            string key,
            string name,
            string displayName,
            string value,
            StepBodyParaType stepBodyParaType)
        {
            Id = id;
            Key = key;
            Name = name;
            DisplayName = displayName;
            Value = value;
            StepBodyParaType = stepBodyParaType;
        }
    }
}