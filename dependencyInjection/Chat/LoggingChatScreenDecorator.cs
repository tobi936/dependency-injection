using dependencyInjection.Logging;
using dependencyInjection.Model;

namespace dependencyInjection.Chat
{
	internal class LoggingChatScreenDecorator : IChatScreen
	{
		private readonly IChatScreen inner;
		private readonly IChatLogger logger;

		public LoggingChatScreenDecorator(IChatScreen inner, IChatLogger logger)
		{
			this.inner = inner;
			this.logger = logger;
		}

		public void Send(User from, User to, string channel, string message)
		{
			logger.Log($"{from.Name} -> {to.Name} ({channel}): {message}");
			inner.Send(from, to, channel, message);
		}

		public void Broadcast(User from, string channel, string message)
		{
			logger.Log($"{from.Name} -> alle ({channel}): {message}");
			inner.Broadcast(from, channel, message);
		}

		public void Render()
		{
			inner.Render();
		}
	}
}
