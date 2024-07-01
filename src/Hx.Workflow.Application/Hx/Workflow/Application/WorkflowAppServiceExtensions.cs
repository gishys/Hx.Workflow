using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Hx.Workflow.Application
{
    public static class WorkflowAppServiceExtensions
    {
        public static List<WkNode> ToWkNodes(this ICollection<WkNodeCreateDto> nodes)
        {
            var nodeEntitys = new List<WkNode>();
            foreach (var node in nodes.OrderBy(d => d.SortNumber))
            {
                var nodeEntity = new WkNode(
                        node.Name,
                        node.DisplayName,
                        node.StepNodeType,
                        node.Version,
                        node.NodeFormType,
                        node.LimitTime);
                if (node.NextNodes?.Count > 0)
                {
                    foreach (var condition in node.NextNodes)
                    {
                        if (condition.NextNodeName.Length > 0)
                        {
                            var conditionEntity = new WkConditionNode(condition.NextNodeName, condition.NodeType);
                            if (condition.WkConNodeConditions?.Count > 0)
                            {
                                foreach (var conCondition in condition.WkConNodeConditions)
                                {
                                    conditionEntity.AddConNodeCondition(
                                        new WkConNodeCondition(
                                            conCondition.Field,
                                            conCondition.Operator,
                                            conCondition.Value
                                            ));
                                }
                            }
                            nodeEntity.AddNextNode(conditionEntity);
                        }
                    }
                }
                if (node.Position?.Count > 0)
                {
                    foreach (var point in node.Position)
                    {
                        nodeEntity.AddPoint(
                            new WkPoint(
                                point.Left,
                                point.Right,
                                point.Top,
                                point.Bottom));
                    }
                }
                if (node.OutcomeSteps?.Count > 0)
                {
                    foreach (var outcomp in node.OutcomeSteps)
                    {
                        nodeEntity.AddOutcomeSteps(
                            new WkNodePara(
                                outcomp.Key,
                                outcomp.Value));
                    }
                }
                if (node.WkCandidates?.Count > 0)
                {
                    foreach (var candidate in node.WkCandidates)
                    {
                        nodeEntity.AddCandidates(new WkNodeCandidate(
                            candidate.CandidateId,
                            candidate.UserName,
                            candidate.DisplayUserName,
                            candidate.DefaultSelection));
                    }
                }
                if (node.ApplicationForms?.Count > 0)
                {
                    foreach (var appForm in node.ApplicationForms)
                    {
                        nodeEntity.AddApplicationForms(new ApplicationForm(
                            appForm.ParentId,
                            appForm.Code,
                            appForm.Name,
                            appForm.DisplayName,
                            appForm.ApplicationType));
                    }
                }
                nodeEntitys.Add(nodeEntity);
            }
            return nodeEntitys;
        }
        public static ICollection<Candidate> ToWkCandidate(this ICollection<WkCandidateUpdateDto> inputs)
        {
            var resultEntity = new List<Candidate>();
            foreach (var entity in inputs)
            {
                resultEntity.Add(new Candidate(entity.CandidateId, entity.UserName, entity.DisplayUserName, entity.DefaultSelection));
            }
            return resultEntity;
        }
        public static IDictionary<string, object> Cancat(this IDictionary<string, object> inputs, IDictionary<string, object> objs)
        {
            var workflowData = new Dictionary<string, object>(inputs);
            foreach (var item in objs)
            {
                if (!workflowData.ContainsKey(item.Key))
                {
                    workflowData.Add(item.Key, item.Value);
                }
            }
            return workflowData;
        }
    }
}