namespace dependencyInjection.Services
{
	internal interface IMessageService
	{
		void Send(User from, User to, string message);

		void Broadcast(User from, string message);
	}
}
