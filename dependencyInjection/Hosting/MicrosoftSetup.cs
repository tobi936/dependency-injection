using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;
using Microsoft.Extensions.DependencyInjection;
using System;

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
			services.AddTransient<TelegramMessageService>();

			services.AddSingleton<UserRepository>(sp =>
			{
				var repo = new UserRepository();
				UserSeeder.Seed(repo);
				return repo;
			});
			services.AddTransient<ChatAppMicrosoft>();

			services.AddSingleton<Lazy<IChatScreen>>(sp => new Lazy<IChatScreen>(() => sp.GetRequiredService<IChatScreen>()));

			services.AddSingleton<TrackedResourceNormalMs>();
			services.AddSingleton<TrackedResourceExternalMs>();

			services.AddSingleton<GreetedServiceMs>(_ =>
			{
				var service = new GreetedServiceMs();
				service.Init("Hallo vom Microsoft DI Wrapper-Init");
				return service;
			});

			services.AddSingleton<CyclicB>();
			services.AddSingleton<CyclicA>();

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