using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;

namespace dependencyInjection.Hosting
{
	public static class AutofacSetup
	{
		public static void Run()
		{
			var builder = new ContainerBuilder();

			// Feature: Modul einbinden - Scanning + Metadata + Interception stecken in MessagingModule.
			builder.RegisterModule<MessagingModule>();

			// Feature: WithParameter - fester Konstruktor-Wert bei der Registrierung.
			// MS DI kann das nur ueber eine Factory-Lambda (siehe MicrosoftSetup).
			builder.RegisterType<FileChatLogger>().As<IChatLogger>()
				.WithParameter("path", "chat.log")
				.SingleInstance();

			// Feature: SingleInstance = Singleton. (Autofac-Default ist InstancePerDependency = Transient.)
			// Feature: PropertiesAutowired - fuellt die Audit-Property automatisch. MS DI kann das nicht.
			// Feature: AOP-Performance-Tracking - .EnableInterfaceInterceptors() baut einen Castle-Proxy
			// um ChatScreen, .InterceptedBy() verkettet den Interceptor davor. Jeder Aufruf auf IChatScreen
			// laeuft durch PerformanceTrackingInterceptor, OHNE dass ChatScreen etwas davon weiss.
			// MS DI hat keinen eingebauten Interceptor-Mechanismus - dort braucht man Middleware/Filter-Pipeline.
			builder.RegisterType<PerformanceTrackingInterceptor>();
			builder.RegisterType<ChatScreen>().As<IChatScreen>()
				.PropertiesAutowired()
				.EnableInterfaceInterceptors()
				.InterceptedBy(typeof(PerformanceTrackingInterceptor))
				.SingleInstance();

			// Feature: Decorator eingebaut - umhuellt IChatScreen mit Logging. MS DI braucht dafuer Scrutor.
			builder.RegisterDecorator<LoggingChatScreenDecorator, IChatScreen>();

			// Feature: OnActivated-Hook - Callback sobald die Instanz fertig ist (hier: User seeden).
			// MS DI hat keine Lifecycle-Hooks.
			builder.RegisterType<UserRepository>()
				.OnActivated(e => Seed(e.Instance))
				.SingleInstance();

			// Feature: InstancePerLifetimeScope - eine Instanz pro Scope (Pendant zu AddScoped).
			builder.RegisterType<ChatAppAutofac>().InstancePerLifetimeScope();

			// Feature: Feingranulares Disposal - dieser Service wird vom Container NICHT disposed.
			// .ExternallyOwned() = "Eigentuemer kuendert sich selbst" - perfekt fuer Shared-Ressourcen.
			// MS DI hat keinen Mechanismus, um Disposal selektiv zu unterbinden.
			builder.RegisterType<TrackedResourceExternal>().As<TrackedResourceExternal>()
				.ExternallyOwned()
				.InstancePerLifetimeScope();

			// Feature: Normal registrierter IDisposable wird am Scope-Ende automatisch disposed.
			builder.RegisterType<TrackedResourceNormal>().As<TrackedResourceNormal>()
				.InstancePerLifetimeScope();

			// Feature: OnActivated - Lifecycle-Hook, der NACH dem Konstruktor laeuft.
			// Hier setzen wir eine Property, ohne den Konstruktor der Klasse zu vergiften.
			// MS DI kennt das nicht - dort braucht man einen separaten Init-Provider.
			builder.RegisterType<GreetedService>()
				.OnActivated(e => e.Instance.Init("Hallo vom Autofac OnActivated-Hook"))
				.SingleInstance();

			// Feature: PropertiesAutowired() loest zirkulaere Abhaengigkeiten auf.
			// CyclicA haelt CyclicB per Property (NICHT Konstruktor), Autofac setzt sie nach der Konstruktion.
			// MS DI wuerde hier mit einer zirkulaeren Konstruktor-Injection crashen.
			// WithParameter("container", ...) liefert den festen String fuer den Konstruktor -
			// Autofac hat keine "Magic" dafuer, das muss man explizit angeben (genau wie bei MessageRouter).
			builder.RegisterType<CyclicA>().As<CyclicA>()
				.WithParameter("container", "Autofac")
				.PropertiesAutowired()
				.SingleInstance();

			builder.RegisterType<CyclicB>().As<CyclicB>()
				.WithParameter("container", "Autofac")
				.PropertiesAutowired()
				.SingleInstance();

			// Feature: Delegate Factory - Autofac erzeugt den Func automatisch.
			// MessageRouter bekommt Func<string, IMessageService> und kann zur Laufzeit pro Channel
			// den passenden Messenger aus dem Container resolven - ohne eigene Factory-Klasse.
			// In MS DI muss der Func von Hand gebaut werden (siehe MicrosoftSetup).
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

			// Feature: Lifetime Scope - verschachtelter Scope; Autofac kann pro Scope sogar nachregistrieren.
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
