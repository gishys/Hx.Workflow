using Hx.Workflow.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class BusinessNumberManager : ISingletonDependency
    {
        private IWkInstanceRepository WkInstanceRepository { get; }
        private readonly IDistributedCache<BusinessNumberCache> AppointmentStockCache;
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public BusinessNumberManager(
            IWkInstanceRepository wkInstanceRepository,
            IDistributedCache<BusinessNumberCache> appointmentStockCache)
        {
            WkInstanceRepository = wkInstanceRepository;
            AppointmentStockCache = appointmentStockCache;
        }
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
                        return new BusinessNumberCache(businessType, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                    });
                string prefix = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}";
                cache.Next();
                string nextNumber = (cache.Count + 1).ToString();
                await AppointmentStockCache.SetAsync(key, cache);
                return nextNumber;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}