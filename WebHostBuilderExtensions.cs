using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System;
using System.Globalization;
using System.Linq;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        private static readonly string ConfigurationMarker = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Configures the host builder to host an Orleans silo.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="configureDelegate">The delegate used to configure the silo.</param>
        /// <returns>The host builder.</returns>
        /// <remarks>
        /// Calling this method multiple times on the same <see cref="IHostBuilder"/> instance will result in one silo being configured.
        /// However, the effects of <paramref name="configureDelegate"/> will be applied once for each call.
        /// </remarks>
        public static IWebHostBuilder UseOrleans(
            this IWebHostBuilder hostBuilder,
            Action<HostBuilderContext, ISiloBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));

            // Track how many times `UseOrleans` is called.
            int.TryParse(hostBuilder.GetSetting(ConfigurationMarker), NumberStyles.None, CultureInfo.InvariantCulture, out var thisCallNumber);
            ++thisCallNumber;
            hostBuilder.UseSetting(ConfigurationMarker, thisCallNumber.ToString(CultureInfo.InvariantCulture));

            return hostBuilder.ConfigureServices((context, services) =>
            {
                var registration = services.FirstOrDefault(s => s.ServiceType.Equals(typeof(SiloBuilder)));
                SiloBuilder siloBuilder;
                if (registration == null)
                {
                    siloBuilder = new SiloBuilder();
                    services.AddSingleton(siloBuilder);
                }
                else
                {
                    siloBuilder = (SiloBuilder)registration.ImplementationInstance;
                }

                siloBuilder.ConfigureSilo(configureDelegate);

                // Check if this is the final call to `UseOrleans`
                int.TryParse(hostBuilder.GetSetting(ConfigurationMarker), NumberStyles.None, CultureInfo.InvariantCulture, out var callNumber);
                if (callNumber == thisCallNumber)
                {
                    var hostContext = new HostBuilderContext(siloBuilder.Properties)
                    {
                        HostingEnvironment = new HostingEnvironment(context.HostingEnvironment),
                        Configuration = context.Configuration
                    };
                    siloBuilder.Build(hostContext, services);
                }
            });
        }

        /// <summary>
        /// Configures the host builder to host an Orleans silo.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="configureDelegate">The delegate used to configure the silo.</param>
        /// <returns>The host builder.</returns>
        /// <remarks>
        /// Calling this method multiple times on the same <see cref="IHostBuilder"/> instance will result in one silo being configured.
        /// However, the effects of <paramref name="configureDelegate"/> will be applied once for each call.
        /// </remarks>
        public static IWebHostBuilder UseOrleans(this IWebHostBuilder hostBuilder, Action<ISiloBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            return hostBuilder.UseOrleans((ctx, siloBuilder) => configureDelegate(siloBuilder));
        }
    }
}
