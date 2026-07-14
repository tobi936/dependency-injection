using dependencyInjection.Chat;
using dependencyInjection.Model;
using System;

namespace dependencyInjection.Messaging
{
	[Messenger("SMS", "kürzt lange Texte")]
	internal class SMSMessageService : IMessageService
	{
		private const int MaxSmsLength = 50;

		private readonly IChatScreen chatScreen;

		public SMSMessageService(IChatScreen chatScreen)
		{
			this.chatScreen = chatScreen;
		}

		public void Send(User from, User to, string message)
		{
			chatScreen.Send(from, to, "SMS", Shorten(message));
		}

		public void Broadcast(User from, string message)
		{
			chatScreen.Broadcast(from, "SMS", Shorten(message));
		}

		private static string Shorten(string message)
		{
			return message.Length <= MaxSmsLength
				? message
				: message.Substring(0, MaxSmsLength - 3) + "...";
		}
	}
}
