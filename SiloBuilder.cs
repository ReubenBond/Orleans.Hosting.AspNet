﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Internal wrapper type of <see cref="IHostBuilder"/> that scope all configuration extensions related to orleans.
    /// </summary>
    internal class SiloBuilder : ISiloBuilder
    {
        private readonly Dictionary<object, object> properties = new Dictionary<object, object>();
        private readonly List<Action<HostBuilderContext, ISiloBuilder>> configureSiloDelegates = new List<Action<HostBuilderContext, ISiloBuilder>>();
        private readonly List<Action<HostBuilderContext, IServiceCollection>> configureServicesDelegates = new List<Action<HostBuilderContext, IServiceCollection>>();

        /// <inheritdoc />
        public IDictionary<object, object> Properties => properties;

        public void Build(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            foreach (var configurationDelegate in this.configureSiloDelegates)
            {
                configurationDelegate(context, this);
            }

            serviceCollection.AddHostedService<SiloHostedService>();
            this.ConfigureDefaults();
            this.ConfigureApplicationParts(parts => parts.ConfigureDefaults());

            foreach (var configurationDelegate in this.configureServicesDelegates)
            {
                configurationDelegate(context, serviceCollection);
            }
        }

        public ISiloBuilder ConfigureSilo(Action<HostBuilderContext, ISiloBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            this.configureSiloDelegates.Add(configureDelegate);
            return this;
        }

        /// <inheritdoc />
        public ISiloBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            this.configureServicesDelegates.Add(configureDelegate);
            return this;
        }
    }
}
