using System.Reflection;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.DynamicCode
{
    public static class AsyncMethodExecutor
    {
        public static async Task<object?> ExecuteAsync(MethodInfo method, object instance, object[] parameters)
        {
            try
            {
                var result = method.Invoke(instance, parameters);
                return result switch
                {
                    Task task => await HandleTaskAsync(task),
                    ValueTask valueTask => await HandleValueTaskAsync(valueTask),
                    _ => result
                };
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private static async Task<object?> HandleTaskAsync(Task task)
        {
            await task.ConfigureAwait(false);
            if (task.GetType().IsGenericType)
            {
                return task.GetType().GetProperty("Result")?.GetValue(task);
            }
            return null;
        }

        private static async Task<object?> HandleValueTaskAsync(ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
            return null;
        }
    }
}
