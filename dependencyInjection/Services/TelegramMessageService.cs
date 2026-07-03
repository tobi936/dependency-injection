namespace dependencyInjection.Services
{
	[Messenger("Telegram", "Cloud-Sync")]
	internal class TelegramMessageService : IMessageService
	{
		private readonly IChatScreen chatScreen;

		public TelegramMessageService(IChatScreen chatScreen)
		{
			this.chatScreen = chatScreen;
		}

		public void Send(User from, User to, string message)
		{
			chatScreen.Send(from, to, "Telegram", message);
		}

		public void Broadcast(User from, string message)
		{
			chatScreen.Broadcast(from, "Telegram", message);
		}
	}
}
