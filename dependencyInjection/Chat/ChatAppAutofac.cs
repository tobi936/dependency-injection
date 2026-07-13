using Autofac.Features.Metadata;
using Spectre.Console;
using dependencyInjection.Messaging;
using dependencyInjection.Model;

namespace dependencyInjection.Chat
{
	internal class ChatAppAutofac
	{
		private const string Quit = "Beenden";
		private const string BroadcastChoice = "alle (Broadcast)";

		private readonly UserRepository users;
		private readonly IChatScreen screen;
		private readonly IReadOnlyList<Meta<Lazy<IMessageService>>> messengers;

		public ChatAppAutofac(UserRepository users, IChatScreen screen, IEnumerable<Meta<Lazy<IMessageService>>> messengers)
		{
			this.users = users;
			this.screen = screen;
			this.messengers = messengers.ToList();
		}

		public void Run()
		{
			while (true)
			{
				screen.Render();

				var from = AskSender();
				if (from is null)
				{
					break;
				}

				var recipient = AskRecipient(from);
				var messenger = AskMessenger();
				var text = AskText(from);

				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}

				Deliver(from, recipient, messenger, text);
			}
		}

		private IMessageService AskMessenger()
		{
			var byLabel = messengers.ToDictionary(
				m => $"{m.Metadata["Name"]} ({m.Metadata["Note"]})",
				m => m.Value);

			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("Welcher [bold]Messenger[/]?")
					.AddChoices(byLabel.Keys));

			return byLabel[choice].Value;
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
