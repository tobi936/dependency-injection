using Autofac;
using Autofac.Extras.DynamicProxy;
using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Logging;
using dependencyInjection.Model;

namespace dependencyInjection.Hosting
{
	public static class AutofacSetup
	{
		public static void Run(DemoMode mode)
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule<MessagingModule>();

			builder.RegisterType<FileChatLogger>().As<IChatLogger>()
				.WithParameter("path", "chat.log")
				.SingleInstance();

			builder.RegisterType<PerformanceTrackingInterceptor>();
			builder.RegisterType<ChatScreen>().As<IChatScreen>()
				.PropertiesAutowired()
				.EnableInterfaceInterceptors()
				.InterceptedBy(typeof(PerformanceTrackingInterceptor))
				.SingleInstance();

			builder.RegisterDecorator<LoggingChatScreenDecorator, IChatScreen>();

			builder.RegisterType<UserRepository>()
				.OnActivated(e => UserSeeder.Seed(e.Instance))
				.SingleInstance();

			builder.RegisterType<ChatAppAutofac>().InstancePerLifetimeScope();

			builder.RegisterType<TrackedResourceExternal>().As<TrackedResourceExternal>()
				.ExternallyOwned()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrackedResourceNormal>().As<TrackedResourceNormal>()
				.InstancePerLifetimeScope();
				
			builder.RegisterType<GreetedService>()
				.OnActivated(e => e.Instance.Init("Hallo vom Autofac OnActivated-Hook"))
				.SingleInstance();


			builder.Register<CyclicA>(ctx => new CyclicA("Autofac", ctx.Resolve<CyclicB>()))
				.OnActivated(e => e.Instance.B.A = e.Instance)
				.SingleInstance();

			builder.RegisterType<CyclicB>().As<CyclicB>()
				.WithParameter("container", "Autofac")
				.SingleInstance();

			using var container = builder.Build();

			using var scope = container.BeginLifetimeScope();

			if (mode is DemoMode.Showcase or DemoMode.Beides)
			{
				AutofacShowcase.Run(scope);
			}

			if (mode is DemoMode.BasisChat or DemoMode.Beides)
			{
				scope.Resolve<ChatAppAutofac>().Run();
			}
		}
	}
}
