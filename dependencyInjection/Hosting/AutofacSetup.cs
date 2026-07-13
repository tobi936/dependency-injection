using Autofac;
using Autofac.Extras.DynamicProxy;
using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;
using System.Reflection;

namespace dependencyInjection.Hosting
{
	public static class AutofacSetup
	{
		public static void Run()
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
				.OnActivated(e => Seed(e.Instance))
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

			builder.Register<MessageRouter>(ctx =>
			{
				var local = ctx.Resolve<ILifetimeScope>();
				Func<string, IMessageService> factory = channel =>
				{
					var t = typeof(IMessageService).Assembly.GetTypes()
						.FirstOrDefault(x => typeof(IMessageService).IsAssignableFrom(x)
							&& !x.IsInterface
							&& x.GetCustomAttribute<MessengerAttribute>() is { } attr
							&& attr.DisplayName == channel)
						?? throw new InvalidOperationException($"Kein Messenger für '{channel}'");
					return (IMessageService)local.Resolve(t);
				};
				return new MessageRouter("Autofac", factory);
			})
			.SingleInstance();

			using var container = builder.Build();

			using var scope = container.BeginLifetimeScope();
			AutofacShowcase.Run(scope);
			scope.Resolve<ChatAppAutofac>().Run();
		}

		private static void Seed(UserRepository users)
		{
			if (users.Users.Count > 0)
			{
				return;
			}

			users.Add(new User { Id = 1, Name = "John" });
			users.Add(new User { Id = 2, Name = "Jane" });
			users.Add(new User { Id = 3, Name = "Bob" });
			users.Add(new User { Id = 4, Name = "Alice" });
		}
	}
}
