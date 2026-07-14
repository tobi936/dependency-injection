using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;
using Microsoft.Extensions.DependencyInjection;

namespace dependencyInjection.Hosting
{
	public static class MicrosoftSetup
	{
		public static void Run(DemoMode mode)
		{
			var services = new ServiceCollection();

			services.AddSingleton<IChatLogger>(_ => new FileChatLogger("chat.log"));

			services.AddSingleton<IChatScreen, ChatScreen>();

			services.Decorate<IChatScreen, LoggingChatScreenDecorator>();

			services.AddKeyedTransient<IMessageService, SMSMessageService>("sms");
			services.AddKeyedTransient<IMessageService, WhatsAppMessageService>("whatsapp");

			services.AddTransient<SMSMessageService>();
			services.AddTransient<WhatsAppMessageService>();

			services.AddSingleton<UserRepository>(sp =>
			{
				var repo = new UserRepository();
				UserSeeder.Seed(repo);
				return repo;
			});
			services.AddTransient<ChatAppMicrosoft>();

			services.AddSingleton<TrackedResourceNormalMs>();
			services.AddSingleton<TrackedResourceExternalMs>();

			services.AddSingleton<GreetedServiceMs>(_ =>
			{
				var service = new GreetedServiceMs();
				service.Init("Hallo vom Microsoft DI Wrapper-Init");
				return service;
			});

			services.AddSingleton<CyclicB>(_ => new CyclicB("Microsoft.Extensions.DI"));
			services.AddSingleton<CyclicA>(sp => new CyclicA("Microsoft.Extensions.DI", sp.GetRequiredService<CyclicB>()));

			services.AddTransient<Func<string, Greeting>>(_ => name => new Greeting(name));
			services.AddTransient<GreetingConsumer>();

			using var provider = services.BuildServiceProvider();

			if (mode is DemoMode.Showcase or DemoMode.Beides)
			{
				MicrosoftShowcase.Run(provider);
			}

			if (mode is DemoMode.BasisChat or DemoMode.Beides)
			{
				provider.GetRequiredService<ChatAppMicrosoft>().Run();
			}
		}
	}
}