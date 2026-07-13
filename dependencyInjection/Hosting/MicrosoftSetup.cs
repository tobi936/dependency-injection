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
		public static void Run()
		{
			var services = new ServiceCollection();

			services.AddSingleton<IChatLogger>(_ => new FileChatLogger("chat.log"));

			services.AddSingleton<IChatScreen, ChatScreen>();

			services.Decorate<IChatScreen, LoggingChatScreenDecorator>();

			services.AddKeyedTransient<IMessageService, SMSMessageService>("sms");
			services.AddKeyedTransient<IMessageService, WhatsAppMessageService>("whatsapp");

			services.AddSingleton<UserRepository>(sp =>
			{
				var repo = new UserRepository();
				Seed(repo);
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
			MicrosoftShowcase.Run(provider);
			provider.GetRequiredService<ChatAppMicrosoft>().Run();
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