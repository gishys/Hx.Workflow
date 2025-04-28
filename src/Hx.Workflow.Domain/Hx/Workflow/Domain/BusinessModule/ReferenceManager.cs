using Hx.Workflow.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class ReferenceManager(
        IWkInstanceRepository wkInstanceRepository,
        IDistributedCache<ReferenceCache> appointmentStockCache) : ISingletonDependency
    {
        private IWkInstanceRepository WkInstanceRepository { get; } = wkInstanceRepository;
        private readonly IDistributedCache<ReferenceCache> AppointmentStockCache = appointmentStockCache;
        static readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<string> GetMaxNumber(string businessType = "")
        {
            _semaphore.Wait();
            try
            {
                var key = $"{businessType}{DateTime.Now:d}";
                var cache = await AppointmentStockCache.GetOrAddAsync(key,
                    async () =>
                    {
                        var maxNumber = await WkInstanceRepository.GetMaxNumberAsync();
                        return new ReferenceCache(key, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                    });
                if (cache != null)
                {
                    string prefix = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}";
                    cache.Next();
                    string nextNumber = $"{prefix}{cache.Count.ToString().PadLeft(5, '0')}";
                    await AppointmentStockCache.SetAsync(key, cache);
                    return nextNumber;
                }
                throw new UserFriendlyException(message: $"获取Reference Cache：[{key}]失败！");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}