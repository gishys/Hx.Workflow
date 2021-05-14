using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hx.Workflow.Domain
{
    public class WkStepBody : FullAuditedEntity<Guid>
    {
        public string Name { get;protected set; }
        public string DisplayName { get; protected set; }
        public ICollection<WkStepBodyParam> Inputs { get; protected set; }
        public string TypeFullName { get; protected set; }
        public string AssemblyFullName { get; protected set; }
        public WkStepBody()
        { }
        public WkStepBody(
            string name,
            string displayName,
            ICollection<WkStepBodyParam> inputs,
            string typeFullName,
            string assemblyFullName)
        {
            Name = name;
            DisplayName = displayName;
            Inputs = inputs;
            TypeFullName = typeFullName;
            AssemblyFullName = assemblyFullName;
        }
        public Task SetInputs(WkStepBodyParam wkParam)
        {
            Inputs.Add(wkParam);
            return Task.CompletedTask;
        }
    }
}
