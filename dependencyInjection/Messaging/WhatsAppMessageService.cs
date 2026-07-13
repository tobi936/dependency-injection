using dependencyInjection.Chat;
using dependencyInjection.Model;

namespace dependencyInjection.Messaging
{
	[Messenger("WhatsApp", "lange Texte ok")]
	internal class WhatsAppMessageService : IMessageService
	{
		private readonly IChatScreen chatScreen;

		public WhatsAppMessageService(IChatScreen chatScreen)
		{
			this.chatScreen = chatScreen;
		}

		public void Send(User from, User to, string message)
		{
			chatScreen.Send(from, to, "WhatsApp", message);
		}

		public void Broadcast(User from, string message)
		{
			chatScreen.Broadcast(from, "WhatsApp", message);
		}
	}
}
