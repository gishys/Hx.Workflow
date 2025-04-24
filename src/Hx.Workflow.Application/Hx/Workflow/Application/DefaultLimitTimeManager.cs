using Hx.Workflow.Application.Contracts;
using System;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public class DefaultLimitTimeManager : ILimitTimeManager
    {
        public async Task<DateTime?> GetAsync(DateTime time, int? minutes)
        {
            if (minutes == null) return null;
            if (double.TryParse(minutes.ToString(), out double addTime))
            {
                return await Task.FromResult(time.AddMinutes(addTime));
            }
            return null;
        }
    }
}
