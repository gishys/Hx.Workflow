using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkConNodeCondition : Entity<Guid>
    {
        public Guid WkConditionNodeId { get; protected set; }
        public string Field { get; protected set; }
        public string Operator { get; protected set; }
        public string Value { get; protected set; }
        public WkConNodeCondition()
        { }
        public WkConNodeCondition(
            string field,
            string tOperator,
            string value
            )
        {
            Field = field;
            Operator = tOperator;
            Value = value;
        }
        public WkConNodeCondition(
            Guid id,
            string field,
            string tOperator,
            string value
            )
        {
            Id = id;
            Field = field;
            Operator = tOperator;
            Value = value;
        }
    }
}
