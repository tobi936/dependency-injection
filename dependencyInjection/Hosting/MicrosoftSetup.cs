using Microsoft.Extensions.DependencyInjection;
using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;
namespace dependencyInjection.Hosting
{
	public static class MicrosoftSetup
	{
		public static void Run()
		{
			var services = new ServiceCollection();

			// MS DI: kein WithParameter - feste Konstruktor-Werte gehen nur ueber eine Factory-Lambda.
			services.AddSingleton<IChatLogger>(_ => new FileChatLogger("chat.log"));

			// Lifetimes: AddSingleton / AddScoped / AddTransient - das Grundgeruest, das jeder Container hat.
			// Property Injection gibt es hier nicht: ChatScreen.Audit bleibt null (nur Konstruktor-Injection).
			services.AddSingleton<IChatScreen, ChatScreen>();

			// Decorator kommt von Scrutor (Fremd-Paket). MS DI selbst hat KEINEN eingebauten Decorator.
			services.Decorate<IChatScreen, LoggingChatScreenDecorator>();

			// Keyed Services (seit .NET 8) = Ersatz fuer Named/IIndex. Aber: kein Metadata, keine Interception,
			// und jeder Messenger muss von Hand eingetragen werden (kein Scanning, Telegram fehlt hier deshalb).
			services.AddKeyedTransient<IMessageService, SMSMessageService>("sms");
			services.AddKeyedTransient<IMessageService, WhatsAppMessageService>("whatsapp");

			services.AddSingleton<UserRepository>(sp =>
		{
			var repo = new UserRepository();
			Seed(repo);
			return repo;
		});			services.AddTransient<ChatAppMicrosoft>();

		// MS DI hat KEIN eingebautes Lazy<T>-Binding wie Autofac (Autofac.Features.LazyDependencies).
		// SMSMessageService verlangt Lazy<IChatScreen> im Konstruktor - also registrieren wir
		// Lazy<IChatScreen> per Hand als Factory, die beim ersten .Value-Zugriff den
		// (durch Decorator ggf. verpackten) ChatScreen aus dem Provider holt.
		// Wichtig: GetRequiredService<IChatScreen>(), nicht new ChatScreen(...) -
		// so bekommt der SMS-Service den dekorierten ChatScreen inkl. Logging.
		services.AddSingleton<Lazy<IChatScreen>>(sp => new Lazy<IChatScreen>(() => sp.GetRequiredService<IChatScreen>()));

			// Beide Services sind IDisposable - im MS-Zweig werden BEIDE disposed, sobald der Container closed wird.
			// Es gibt kein .ExternallyOwned()-Pendant. Wer das braucht, muss auf ServiceProvider-Middleware ausweichen.
			services.AddSingleton<TrackedResourceNormalMs>();
			services.AddSingleton<TrackedResourceExternalMs>();

			// MS DI: kein OnActivated-Hook. Stattdessen wird die Init-Logik im Registrierungs-Lambda erledigt.
			// Das ist "Constructor-Injection mit uebergebenem Wert" - die saubere MS-Form fuer Lifecycle-Init.
			services.AddSingleton<GreetedServiceMs>(_ => new GreetedServiceMs("Hallo vom Microsoft DI Wrapper-Init"));

			// CyclicA + CyclicB: gleicher Aufbau wie im Autofac-Zweig, aber OHNE PropertiesAutowired().
			// CyclicA haelt B im Konstruktor (hart), CyclicB haelt A als Property (weich).
			// In MS DI fehlt das PropertiesAutowired-Pendant, also bleibt B.A einfach null.
			// CyclicC versucht zusaetzlich, A und B im eigenen Konstruktor zu holen -
			// das fuehrt zur zirkulaeren Abhaengigkeit, die MS DI nicht aufloesen kann
			// (im Gegensatz zu Autofac mit PropertiesAutowired()).
			// MS DI hat keine WithParameter-Variante - der Konstruktor-String kommt ueber das Factory-Lambda.
			services.AddSingleton<CyclicB>(_ => new CyclicB("Microsoft DI"));
			services.AddSingleton<CyclicA>(sp => new CyclicA("Microsoft DI", sp.GetRequiredService<CyclicB>()));
			services.AddSingleton<CyclicC>();

			// Delegate Factory: in MS DI muss der Func komplett von Hand gebaut werden.
			// ActivatorUtilities.CreateInstance<T>(sp) ist die MS-Form von "Container.Resolve<T>() mit Konstruktor-Injection".
			// Vorteil: funktioniert ohne explizite Registrierung der Messenger als konkrete Typen.
			services.AddSingleton<MessageRouter>(sp =>
			{
				Func<string, IMessageService> factory = channel => channel switch
				{
					"SMS" => ActivatorUtilities.CreateInstance<SMSMessageService>(sp),
					"WhatsApp" => ActivatorUtilities.CreateInstance<WhatsAppMessageService>(sp),
					_ => throw new InvalidOperationException($"Kein Messenger für '{channel}'")
				};
				return new MessageRouter("Microsoft DI", factory);
			});

			using var provider = services.BuildServiceProvider();
			MicrosoftShowcase.Run(provider);
			provider.GetRequiredService<ChatAppMicrosoft>().Run();
		}

		// MS DI hat keinen OnActivated-Hook - also wird der User-Seeding hier per Hand
		// im Registrierungs-Lambda erledigt (s. AddSingleton<UserRepository> weiter oben).
		// Logik 1:1 wie AutofacSetup.Seed, damit beide Container identische Demo-Daten haben.
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
