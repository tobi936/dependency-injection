using dependencyInjection.Diagnostics;
using dependencyInjection.Messaging;
using dependencyInjection.Model;

namespace dependencyInjection.Advanced
{
	// Feature: Empfaenger des Delegate-Factory-Funcs.
	// Bekommt im Konstruktor Func<string, IMessageService> und kann pro Channel
	// dynamisch den passenden Messenger erzeugen - OHNE eine eigene Factory-Klasse zu schreiben.
	// Im Autofac-Setup wird der Func automatisch vom Container gebaut, im MS-Setup per Hand.
	internal sealed class MessageRouter
	{
		private readonly string container;
		private readonly Func<string, IMessageService> factory;

		public MessageRouter(string container, Func<string, IMessageService> factory)
		{
			this.container = container;
			this.factory = factory;
		}

		public void Dispatch(string channel, User from, string toName, string text, UserRepository users)
		{
			// Pro Aufruf: Factory entscheidet zur Laufzeit, welcher Messenger-Build-Plan genutzt wird.
			// Das ist sauberer als ein riesiger switch im Service selbst.
			var messenger = factory(channel);
			ContainerMetrics.Event(container, "DELEGATE-FACTORY", $"channel='{channel}' -> {messenger.GetType().Name}", "aqua");

			if (toName == "alle (Broadcast)")
			{
				messenger.Broadcast(from, text);
				return;
			}

			var to = users.Users.FirstOrDefault(u => u.Name == toName);
			if (to is not null)
			{
				messenger.Send(from, to, text);
			}
		}
	}
}
