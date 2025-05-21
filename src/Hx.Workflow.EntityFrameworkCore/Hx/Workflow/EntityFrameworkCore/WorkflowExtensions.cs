using Hx.Workflow.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hx.Workflow.EntityFrameworkCore
{
    public static class WorkflowExtensions
    {
        public static IQueryable<WkDefinition> IncludeDetails(
            this IQueryable<WkDefinition> query,
            bool include = true)
        {
            if (!include)
                return query;
            return query
                .Include(d => d.Nodes)
                .ThenInclude(d => d.OutcomeSteps)
                .Include(d => d.Nodes)
                .ThenInclude(d => d.StepBody)
                .ThenInclude(d => d.Inputs)
                .Include(d => d.Nodes)
                .ThenInclude(d => d.NextNodes)
                .ThenInclude(d => d.Rules)
                .Include(d => d.Nodes)
                .ThenInclude(d => d.WkCandidates)
                .Include(d => d.Nodes)
                .ThenInclude(d => d.ApplicationForms)
                .ThenInclude(d => d.ApplicationForm)
                .Include(d => d.Nodes)
                .ThenInclude(d => d.Materials)
                .ThenInclude(d => d.Children)
                .Include(d => d.WkCandidates);
        }
        public static IQueryable<WkDefinitionGroup> IncludeDetails(
            this IQueryable<WkDefinitionGroup> query,
            bool include = true)
        {
            if (!include)
                return query;
            return query
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.OutcomeSteps)
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.StepBody)
                .ThenInclude(d => d.Inputs)
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.NextNodes)
                .ThenInclude(d => d.Rules)
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.WkCandidates)
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.ApplicationForms)
                .ThenInclude(d => d.ApplicationForm)
                .Include(d => d.Items)
                .ThenInclude(d => d.Nodes)
                .ThenInclude(d => d.Materials)
                .ThenInclude(d => d.Children)
                .Include(d => d.Items)
                .ThenInclude(d => d.WkCandidates);
        }
        public static IQueryable<WkInstance> IncludeDetails(
            this IQueryable<WkInstance> query,
            bool include = true)
        {
            if (!include)
                return query;
            return query
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.ExtensionAttributes)
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.WkSubscriptions)
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.WkCandidates)
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.Materials)
                .ThenInclude(d => d.Children)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.Nodes)
                .ThenInclude(x => x.NextNodes)
                .ThenInclude(x => x.Rules)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.WkCandidates)
                .Include(x => x.WkAuditors)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.Nodes)
                .ThenInclude(x => x.ApplicationForms)
                .ThenInclude(x => x.ApplicationForm)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.Nodes)
                .ThenInclude(x => x.WkCandidates);
        }
    }
}