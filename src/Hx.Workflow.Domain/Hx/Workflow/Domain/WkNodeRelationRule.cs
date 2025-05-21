using System;

namespace Hx.Workflow.Domain
{
    public class WkNodeRelationRule
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNodeRelationRule() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public string Field { get; protected set; }
        public string Operator { get; protected set; }
        public string Value { get; protected set; }
        public WkNodeRelationRule(
            string field,
            string tOperator,
            string value
            )
        {
            Field = field;
            Operator = tOperator;
            Value = value;
        }
    }
}
