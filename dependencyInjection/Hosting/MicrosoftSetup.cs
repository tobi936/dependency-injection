using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

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

			services.AddTransient<SMSMessageService>();
			services.AddTransient<WhatsAppMessageService>();
			services.AddTransient<TelegramMessageService>();

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

			services.AddSingleton<MessageRouter>(sp =>
			{
				var messengerMapping = typeof(IMessageService).Assembly.GetTypes()
					.Where(x => typeof(IMessageService).IsAssignableFrom(x)
						&& !x.IsInterface
						&& x.GetCustomAttribute<MessengerAttribute>() != null)
					.ToDictionary(
						x => x.GetCustomAttribute<MessengerAttribute>()!.DisplayName,
						x => x
					);

				Func<string, IMessageService> factory = channel =>
				{
					if (!messengerMapping.TryGetValue(channel, out var type))
					{
						throw new InvalidOperationException($"Kein Messenger für '{channel}'");
					}
					return (IMessageService)sp.GetRequiredService(type);
				};

				return new MessageRouter("Microsoft DI", factory);
			});

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