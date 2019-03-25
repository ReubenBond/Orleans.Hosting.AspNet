using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using IAspNetLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using AspNetLifetime = Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime;
using IHostingLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using HostingLifetime = Microsoft.Extensions.Hosting.Internal.ApplicationLifetime;

namespace Microsoft.AspNetCore.Hosting
{
    internal class SiloHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly object applicationLifetime;
        private readonly ISiloHost siloHost;
        private ExceptionDispatchInfo startupException;

        public SiloHostedService(
            ISiloHost siloHost,
            IEnumerable<IConfigurationValidator> configurationValidators,
            ILogger<SiloHostedService> logger,
            IServiceProvider serviceProvider)
        {
            this.ValidateSystemConfiguration(configurationValidators);
            this.siloHost = siloHost;
            this.logger = logger;
            this.applicationLifetime = serviceProvider.GetService(typeof(IAspNetLifetime))
                                    ?? serviceProvider.GetService(typeof(IHostingLifetime));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Starting Orleans Silo.");
                await this.siloHost.StartAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Orleans Silo started.");
            }
            catch (Exception exception)
            {
                this.logger.LogWarning("Exception starting Orleans silo: {Exception}", exception);
                this.startupException = ExceptionDispatchInfo.Capture(exception);

                if (this.applicationLifetime is AspNetLifetime aspNetLifetime)
                {
                    aspNetLifetime.StopApplication();
                }
                else if (this.applicationLifetime is HostingLifetime hostingLifetime)
                {
                    hostingLifetime.StopApplication();
                }

                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.startupException != null)
            {
                startupException.Throw();
            }

            this.logger.LogInformation("Stopping Orleans Silo");
            await this.siloHost.StopAsync(cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation("Orleans Silo stopped.");
        }

        private void ValidateSystemConfiguration(IEnumerable<IConfigurationValidator> configurationValidators)
        {
            foreach (var validator in configurationValidators)
            {
                validator.ValidateConfiguration();
            }
        }
    }
}
