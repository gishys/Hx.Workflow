﻿using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public interface IHxWorkflowManager
    {
        /// <summary>
        /// terminate workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        Task<bool> TerminateWorkflow(string workflowId);
        /// <summary>
        /// by workflow id get definition
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        WorkflowDefinition GetDefinition(string workflowId, int? version = null);
        /// <summary>
        ///  initialize process registration
        /// </summary>
        Task Initialize();
        Task<IEnumerable<WkStepBody>> GetAllStepBodys();
        Task PublishEventAsync(string eventName, string eventKey, object eventData);
        Task CreateAsync(WkDefinition entity);
        Task UpdateAsync(WkDefinition entity);
        Task StartWorlflow(string id, int version, Dictionary<string, object> inputs);
    }
}
