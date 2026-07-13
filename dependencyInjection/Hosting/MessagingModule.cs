using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;

namespace dependencyInjection.Hosting
{
	// Feature: Module - buendelt zusammengehoerige Registrierungen (wie ein Plugin-Paket).
	// MS DI kennt nur Extension-Methoden (services.AddMessaging()), aber kein echtes Modul-Konzept.
	internal class MessagingModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			// Feature: Assembly-Scanning - findet alle Klassen mit [Messenger]-Attribut automatisch.
			// Neue Messenger-Klasse anlegen = taucht von selbst auf. MS DI hat kein eingebautes Scanning.
			builder.RegisterAssemblyTypes(typeof(IMessageService).Assembly)
				.Where(t => t.GetCustomAttribute<MessengerAttribute>() != null)
				.As<IMessageService>()
				// Feature: Metadata - freie Zusatzinfos pro Registrierung, reicher als MS Keyed Services.
				.WithMetadata("Name", t => t.GetCustomAttribute<MessengerAttribute>()!.DisplayName)
				.WithMetadata("Note", t => t.GetCustomAttribute<MessengerAttribute>()!.Note)
				// Feature: Interception (AOP) - jeder Aufruf laeuft durch den Interceptor, ohne Boilerplate.
				.EnableInterfaceInterceptors()
				.InterceptedBy(typeof(CallLogInterceptor));

			builder.RegisterType<CallLogInterceptor>();
		}
	}
}
