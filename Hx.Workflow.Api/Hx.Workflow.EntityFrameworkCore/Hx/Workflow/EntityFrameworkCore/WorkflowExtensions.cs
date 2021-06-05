using Hx.Workflow.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hx.Workflow.EntityFrameworkCore
{
    public static class WorkflowExtensions
    {
        public static IQueryable<WkDefinition> IncludeDetials(
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
                .ThenInclude(d => d.WkConNodeConditions)
                .Include(d=>d.Nodes)
                .ThenInclude(d=>d.WkCandidates)
                .Include(d=>d.Nodes)
                .ThenInclude(d=>d.ApplicationForms);
        }
        public static IQueryable<WkInstance> IncludeDetials(
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
                .Include(x => x.WkDefinition)
                .Include(x=>x.WkAuditors);
        }
    }
}