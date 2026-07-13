using Autofac;
using dependencyInjection.Chat;
using dependencyInjection.Logging;
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
			builder.RegisterType<ChatScreen>().As<IChatScreen>()
				.PropertiesAutowired()
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

			using var container = builder.Build();

			// Feature: Lifetime Scope - verschachtelter Scope; Autofac kann pro Scope sogar nachregistrieren.
			using var scope = container.BeginLifetimeScope();
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
