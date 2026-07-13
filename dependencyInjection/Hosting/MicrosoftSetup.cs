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

			services.AddSingleton<UserRepository>();
			services.AddTransient<ChatAppMicrosoft>();

			// Beide Services sind IDisposable - im MS-Zweig werden BEIDE disposed, sobald der Container closed wird.
			// Es gibt kein .ExternallyOwned()-Pendant. Wer das braucht, muss auf ServiceProvider-Middleware ausweichen.
			services.AddSingleton<TrackedResourceNormalMs>();
			services.AddSingleton<TrackedResourceExternalMs>();

			// MS DI: kein OnActivated-Hook. Stattdessen wird die Init-Logik im Registrierungs-Lambda erledigt.
			// Das ist "Constructor-Injection mit uebergebenem Wert" - die saubere MS-Form fuer Lifecycle-Init.
			services.AddSingleton<GreetedServiceMs>(_ => new GreetedServiceMs("Hallo vom Microsoft DI Wrapper-Init"));

			// CyclicA + CyclicB haengen per Property aneinander (PropertiesAutowired-Ersatz: manuell im Konstruktor).
			// CyclicC versucht beide im KONSTRUKTOR zu holen - das fuehrt zur zirkulaeren Abhaengigkeit,
			// die MS DI nicht aufloesen kann (im Gegensatz zu Autofac mit PropertiesAutowired()).
			// MS DI hat keine WithParameter-Variante - der Konstruktor-String kommt ueber das Factory-Lambda.
			services.AddSingleton<CyclicA>(_ => new CyclicA("Microsoft DI"));
			services.AddSingleton<CyclicB>(_ => new CyclicB("Microsoft DI"));
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
	}
}
