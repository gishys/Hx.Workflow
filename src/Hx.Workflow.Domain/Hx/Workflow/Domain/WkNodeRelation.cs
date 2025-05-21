using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain
{
    public class WkNodeRelation
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNodeRelation() { }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public virtual string? Label { get; protected set; }
        public virtual string NextNodeName { get; protected set; }
        public virtual WkRoleNodeType NodeType { get; protected set; }
        public virtual ICollection<WkNodeRelationRule> Rules { get; protected set; }
        public WkNodeRelation(
            string nextNodeName,
            WkRoleNodeType nodeType,
            string label = "")
        {
            Label = label;
            NodeType = nodeType;
            NextNodeName = nextNodeName;
            Rules = [];
        }
        public Task AddConNodeCondition(WkNodeRelationRule condition)
        {
            Rules.Add(condition);
            return Task.CompletedTask;
        }
        public void Update(string nextNodeName, WkRoleNodeType nodeType)
        {
            NextNodeName = nextNodeName;
            NodeType = nodeType;
        }
    }
}
