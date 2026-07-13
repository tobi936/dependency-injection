using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using dependencyInjection.Messaging;
using dependencyInjection.Model;

namespace dependencyInjection.Chat
{
	internal class ChatAppMicrosoft
	{
		private const string Quit = "Beenden";
		private const string BroadcastChoice = "alle (Broadcast)";

		private readonly UserRepository users;
		private readonly IChatScreen screen;
		private readonly IMessageService sms;
		private readonly IMessageService whatsapp;

		public ChatAppMicrosoft(UserRepository users, IChatScreen screen, [FromKeyedServices("sms")] IMessageService sms, [FromKeyedServices("whatsapp")] IMessageService whatsapp)
		{
			this.users = users;
			this.screen = screen;
			this.sms = sms;
			this.whatsapp = whatsapp;
		}

		public void Run()
		{
			SeedUsers();

			while (true)
			{
				screen.Render();

				var from = AskSender();
				if (from is null)
				{
					break;
				}

				var recipient = AskRecipient(from);
				var channel = AskChannel();
				var messenger = channel == "WhatsApp" ? whatsapp : sms;
				var text = AskText(from);

				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}

				Deliver(from, recipient, messenger, text);
			}
		}

		private void Deliver(User from, string recipient, IMessageService messenger, string text)
		{
			if (recipient == BroadcastChoice)
			{
				messenger.Broadcast(from, text);
				return;
			}

			var to = FindUser(recipient);
			if (to is not null)
			{
				messenger.Send(from, to, text);
			}
		}

		private void SeedUsers()
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

		private User? AskSender()
		{
			var name = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[bold]Wer schreibt?[/]")
					.AddChoices(users.Users.Select(u => u.Name).Append(Quit)));

			return name == Quit ? null : FindUser(name);
		}

		private string AskRecipient(User from)
		{
			return AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title($"[green]{from.Name}[/] schreibt an wen?")
					.AddChoices(users.Users.Where(u => u.Id != from.Id).Select(u => u.Name).Prepend(BroadcastChoice)));
		}

		private static string AskChannel()
		{
			return AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("Welcher [bold]Messenger[/]?")
					.AddChoices("SMS", "WhatsApp"));
		}

		private static string AskText(User from)
		{
			return AnsiConsole.Prompt(
				new TextPrompt<string>($"[green]{from.Name}[/] >")
					.AllowEmpty());
		}

		private User? FindUser(string name)
		{
			return users.Users.FirstOrDefault(u => u.Name == name);
		}
	}
}
