using Microsoft.Extensions.DependencyInjection;
using dependencyInjection.Chat;
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

			using var provider = services.BuildServiceProvider();
			provider.GetRequiredService<ChatAppMicrosoft>().Run();
		}
	}
}
