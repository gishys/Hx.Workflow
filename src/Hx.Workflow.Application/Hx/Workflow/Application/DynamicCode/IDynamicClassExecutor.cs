using System;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.DynamicCode
{
    public interface IDynamicClassExecutor
    {
        Task ExecuteClassAsync(string classCode, string methodName = "Execute");
        Task<object?> ExecuteMethodAsync(
            string classCode, 
            string methodName = "Execute", 
            object[]? parameters = null, 
            Type[]? genericArguments = null);
    }
}