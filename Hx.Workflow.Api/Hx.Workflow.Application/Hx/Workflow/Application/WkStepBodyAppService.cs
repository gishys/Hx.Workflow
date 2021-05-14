using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public class WkStepBodyAppService : WorkflowAppServiceBase, IWkStepBodyAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody;
        public WkStepBodyAppService(
            IWkStepBodyRespository wkStepBody)
        {
            _wkStepBody = wkStepBody;
        }
        /// <summary>
        /// 需要获取有哪些StepBody（通过判断是否继承自StepBody），
        /// 通过StepBody的public属性
        /// 来查询可用的WkParamKey（必须是StepBody public 属性）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task CreateAsync(WkSepBodyCreateDto input)
        {
            var bodyParams = input.Inputs?.Select(d =>
            new WkStepBodyParam(
                GuidGenerator.Create(),
                d.Key,
                d.Name,
                d.DisplayName,
                d.Value,
                d.StepBodyParaType)).ToList();
            var entity = await _wkStepBody.GetStepBodyAsync(input.Name);
            if (entity != null)
                return;
            await _wkStepBody.InsertAsync(new WkStepBody(
                    input.Name,
                    input.DisplayName,
                    bodyParams,
                    input.TypeFullName,
                    input.AssemblyFullName));
        }
        public virtual async Task DeleteAsync(string name)
        {
            var entity = await _wkStepBody.GetStepBodyAsync(name);
            if (entity != null)
                await _wkStepBody.DeleteAsync(entity, autoSave: true);
        }
        public virtual async Task<WkStepBodyDto> GetStepBodyAsync(string name)
        {
            var entity = await _wkStepBody.GetStepBodyAsync(name);
            return ObjectMapper.Map<WkStepBody, WkStepBodyDto>(entity);
        }
    }
}