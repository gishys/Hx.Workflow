using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// Serializes and deduplicates WorkflowCore lifecycle operations in one DI container.
    /// </summary>
    public sealed class WorkflowRuntimeGuard : ISingletonDependency
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _definitionsInitialized;
        private bool _hostStarted;

        public async Task InitializeOnceAsync(Func<Task> initializeAsync)
        {
            ArgumentNullException.ThrowIfNull(initializeAsync);
            await _gate.WaitAsync();
            try
            {
                if (_definitionsInitialized)
                {
                    return;
                }

                await initializeAsync();
                _definitionsInitialized = true;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StartOnceAsync(Func<Task> startAsync)
        {
            ArgumentNullException.ThrowIfNull(startAsync);
            await _gate.WaitAsync();
            try
            {
                if (_hostStarted)
                {
                    return;
                }

                await startAsync();
                _hostStarted = true;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StopOnceAsync(Func<Task> stopAsync)
        {
            ArgumentNullException.ThrowIfNull(stopAsync);
            await _gate.WaitAsync();
            try
            {
                if (!_hostStarted)
                {
                    return;
                }

                await stopAsync();
                _hostStarted = false;
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
