using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application
{
    public class WkDefinitionAppService : WorkflowAppServiceBase, IWkDefinitionAppService
    {
        private readonly IWkDefinitionRespository _definitionRespository;
        private readonly IWkStepBodyRespository _wkStepBody;
        public WkDefinitionAppService(
            IWkDefinitionRespository definitionRespository,
            IWkStepBodyRespository wkStepBody)
        {
            _definitionRespository = definitionRespository;
            _wkStepBody = wkStepBody;
        }
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var entity = new WkDefinition(
                    input.Title,
                    input.Icon,
                    input.Color,
                    1,
                    input.Discription,
                    version: input.Version <= 0 ? 1 : input.Version);
            var nodeEntitys = input.Nodes.ToWkNodes();
            foreach (var node in nodeEntitys)
            {
                var wkStepBodyId = input.Nodes.FirstOrDefault(d => d.Name == node.Name).WkStepBodyId;
                if (wkStepBodyId?.Length > 0)
                {
                    Guid.TryParse(wkStepBodyId, out Guid guidStepBodyId);
                    var stepBodyEntity = await _wkStepBody.FindAsync(guidStepBodyId);
                    if (stepBodyEntity == null)
                        throw new BusinessException(message: "StepBody没有查询到");
                    await node.SetWkStepBody(stepBodyEntity);
                }
                await entity.AddWkNode(node);
            }

            await _definitionRespository.InsertAsync(entity);
        }
    }
}