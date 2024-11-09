using Hx.Workflow.Application.Contracts;
using System;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public class DefaultLimitTimeManager : ILimitTimeManager
    {
        public Task<DateTime> GetAsync(DateTime time, int? minutes)
        {
            if (minutes == null) return null;
            double.TryParse(minutes.ToString(), out double addTime);
            return Task.FromResult(time.AddMinutes(addTime));
        }
    }
}
