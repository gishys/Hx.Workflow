using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Repositories;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Hx.Workflow.Application.StepBodys
{
    internal class WkGeneralAuditStepBodyChangeEventHandler : ILocalEventHandler<WkGeneralAuditStepBodyChangeEvent>,
          ITransientDependency
    {
        private readonly IWkInstanceRepository _wkInstance;
        public WkGeneralAuditStepBodyChangeEventHandler(IWkInstanceRepository wkInstance)
        {
            _wkInstance = wkInstance;
        }
        public async Task HandleEventAsync(WkGeneralAuditStepBodyChangeEvent eventData)
        {
            var instance = await _wkInstance.FindAsync(eventData.WorkflowInstanceId);
            //TODO: your code that does something on the event
        }
    }
}
