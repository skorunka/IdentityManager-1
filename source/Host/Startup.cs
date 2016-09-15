using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityManager;
using IdentityManager.Configuration;
using IdentityManager.Host.InMemoryService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Builder;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Host
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();

			app.UseIdentityManager();
		}
	}

	internal static class Extensions
	{
		public static IApplicationBuilder UseIdentityManager(this IApplicationBuilder app)
		{
			var factory = new IdentityManagerServiceFactory();

			var rand = new Random();
			var users = Users.Get(rand.Next(5000, 20000));
			var roles = Roles.Get(rand.Next(15));

			factory.Register(new Registration<ICollection<InMemoryUser>>(users));
			factory.Register(new Registration<ICollection<InMemoryRole>>(roles));
			factory.IdentityManagerService = new Registration<IIdentityManagerService, InMemoryIdentityManagerService>();

			var options = new IdentityManagerOptions
			{
				Factory = factory
			};

			return app.UseOwinAppBuilder(builder => builder.UseIdentityManager(options));
		}

		private static IApplicationBuilder UseOwinAppBuilder(this IApplicationBuilder app, Action<IAppBuilder> configuration)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			return app.UseOwin(setup => setup(next =>
			{
				var builder = new AppBuilder();
				var lifetime = (IApplicationLifetime) app.ApplicationServices.GetService(typeof(IApplicationLifetime));

				// ReSharper disable once UseObjectOrCollectionInitializer
				var properties = new AppProperties(builder.Properties);
				properties.AppName = app.ApplicationServices.GetApplicationUniqueIdentifier();
				properties.OnAppDisposing = lifetime.ApplicationStopping;
				properties.DefaultApp = next;

				configuration(builder);

				return builder.Build<Func<IDictionary<string, object>, Task>>();
			}));
		}
	}
}