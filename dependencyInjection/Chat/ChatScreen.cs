using Spectre.Console;
using Spectre.Console.Rendering;
using dependencyInjection.Logging;
using dependencyInjection.Model;

namespace dependencyInjection.Chat
{
	internal record ChatMessage(string Counterpart, string Channel, string Text, DateTime Time, bool Outgoing);

	internal interface IChatScreen
	{
		void Send(User from, User to, string channel, string message);

		void Broadcast(User from, string channel, string message);

		void Render();
	}

	internal class ChatScreen : IChatScreen
	{
		private static readonly Color[] QuadrantColors = { Color.Green, Color.Blue, Color.Yellow, Color.Fuchsia };

		private readonly UserRepository userRepository;

		private readonly Dictionary<int, List<ChatMessage>> historyByUserId = new();

		public ChatScreen(UserRepository userRepository)
		{
			this.userRepository = userRepository;
		}

		// Feature: Property Injection - Autofac fuellt diese Property automatisch (per PropertiesAutowired).
		// Steht NICHT im Konstruktor. Bei Microsoft DI bleibt sie null (MS kann nur Konstruktor-Injection).
		public IChatLogger? Logger { get; set; }

		public void Send(User from, User to, string channel, string message)
		{
			HistoryOf(from).Add(new ChatMessage(to.Name, channel, message, DateTime.Now, Outgoing: true));
			HistoryOf(to).Add(new ChatMessage(from.Name, channel, message, DateTime.Now, Outgoing: false));
		}

		public void Broadcast(User from, string channel, string message)
		{
			HistoryOf(from).Add(new ChatMessage("alle", channel, message, DateTime.Now, Outgoing: true));
			foreach (var user in userRepository.Users)
			{
				if (user.Id != from.Id)
				{
					HistoryOf(user).Add(new ChatMessage(from.Name, channel, message, DateTime.Now, Outgoing: false));
				}
			}
		}

		public void Render()
		{
			Logger?.Log("render");
			Console.Clear();

			var users = userRepository.Users.Take(4).ToList();

			const int promptArea = 12;
			int panelHeight = Math.Max((SafeWindowHeight() - promptArea) / 2, 6);

			var grid = new Table().HideHeaders().Border(TableBorder.None).Expand();
			grid.AddColumn(new TableColumn(string.Empty));
			grid.AddColumn(new TableColumn(string.Empty));
			grid.AddRow(Quadrant(users, 0, panelHeight), Quadrant(users, 1, panelHeight));
			grid.AddRow(Quadrant(users, 2, panelHeight), Quadrant(users, 3, panelHeight));

			AnsiConsole.Write(grid);
		}

		private IRenderable Quadrant(List<User> users, int index, int panelHeight)
		{
			if (index >= users.Count)
			{
				return new Panel(new Markup(" ")).Expand();
			}

			var user = users[index];
			var color = QuadrantColors[index % QuadrantColors.Length];

			int contentLines = Math.Max(panelHeight - 2, 1);

			var messages = HistoryOf(user);
			var shown = messages.Skip(Math.Max(0, messages.Count - contentLines)).ToList();

			var lines = new List<IRenderable>();
			for (int i = shown.Count; i < contentLines; i++)
			{
				lines.Add(new Markup(" "));
			}
			foreach (var m in shown)
			{
				lines.Add(Line(m));
			}

			return new Panel(new Rows(lines))
				.Header(new PanelHeader($"[bold {color.ToMarkup()}]{Markup.Escape(user.Name)}[/]"))
				.BorderColor(color)
				.RoundedBorder()
				.Expand();
		}

		private static IRenderable Line(ChatMessage m)
		{
			var time = $"[grey]{m.Time:HH:mm}[/]";
			var badge = $"[{ChannelColor(m.Channel)}]● {Markup.Escape(m.Channel)}[/]";

			if (m.Outgoing)
			{
				var markup = new Markup($"[green]{Markup.Escape(m.Text)} → {Markup.Escape(m.Counterpart)}[/] {time} {badge}");
				return new Align(markup, HorizontalAlignment.Right);
			}

			var received = new Markup($"{time} [blue]{Markup.Escape(m.Counterpart)}:[/] {Markup.Escape(m.Text)} {badge}");
			return new Align(received, HorizontalAlignment.Left);
		}

		private static string ChannelColor(string channel)
		{
			return channel == "WhatsApp" ? "aqua" : "yellow";
		}

		private List<ChatMessage> HistoryOf(User user)
		{
			if (!historyByUserId.TryGetValue(user.Id, out var list))
			{
				list = new List<ChatMessage>();
				historyByUserId[user.Id] = list;
			}
			return list;
		}

		private static int SafeWindowHeight()
		{
			try
			{
				return Console.WindowHeight;
			}
			catch
			{
				return 24;
			}
		}
	}
}
