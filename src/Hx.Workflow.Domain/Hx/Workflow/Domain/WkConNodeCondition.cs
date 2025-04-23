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
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkConNodeCondition()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
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
