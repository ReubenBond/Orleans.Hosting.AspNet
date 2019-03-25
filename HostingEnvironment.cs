using Microsoft.Extensions.FileProviders;
using IExtensionsHostingEnviroment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using IAspNetHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Microsoft.AspNetCore.Hosting
{
    internal class HostingEnvironment : IExtensionsHostingEnviroment
    {
        private readonly IAspNetHostingEnvironment context;

        public HostingEnvironment(IAspNetHostingEnvironment context)
        {
            this.context = context;
        }

        public string EnvironmentName
        {
            get => context.EnvironmentName;
            set => context.EnvironmentName = value;
        }

        public string ApplicationName
        {
            get => context.ApplicationName;
            set => context.ApplicationName = value;
        }

        public string ContentRootPath
        {
            get => context.ContentRootPath;
            set => context.ContentRootPath = value;
        }

        public IFileProvider ContentRootFileProvider
        {
            get => context.ContentRootFileProvider;
            set => context.ContentRootFileProvider = value;
        }
    }
}
