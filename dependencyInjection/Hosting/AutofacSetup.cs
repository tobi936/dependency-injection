using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using dependencyInjection.Advanced;
using dependencyInjection.Chat;
using dependencyInjection.Hosting;
using dependencyInjection.Logging;
using dependencyInjection.Messaging;
using dependencyInjection.Model;

namespace dependencyInjection.Hosting
{
	public static class AutofacSetup
	{
		public static void Run()
		{
			var builder = new ContainerBuilder();

			// Feature: Modul einbinden - Scanning + Metadata + Interception stecken in MessagingModule.
			builder.RegisterModule<MessagingModule>();

			// Feature: WithParameter - fester Konstruktor-Wert bei der Registrierung.
			// MS DI kann das nur ueber eine Factory-Lambda (siehe MicrosoftSetup).
			builder.RegisterType<FileChatLogger>().As<IChatLogger>()
				.WithParameter("path", "chat.log")
				.SingleInstance();

			// Feature: SingleInstance = Singleton. (Autofac-Default ist InstancePerDependency = Transient.)
			// Feature: PropertiesAutowired - fuellt die Audit-Property automatisch. MS DI kann das nicht.
			// -----------------------------------------------------------------------------------
			// Feature: AOP-Performance-Tracking am Beispiel ChatScreen.
			//
			// 1) builder.RegisterType<PerformanceTrackingInterceptor>();
			//    -> der Interceptor selbst ist ein normaler Dienst. Er muss im Container
			//       registriert sein, damit .InterceptedBy(...) ihn ueber seinen Typ finden kann.
			//
			// 2) .EnableInterfaceInterceptors()
			//    -> AUTOFAC-FEATURE: schaltet den "Interface-Interception"-Modus ein.
			//       Konkret heisst das: Autofac baut fuer IChatScreen zur Laufzeit einen
			//       dynamischen Proxy (mit Castle.DynamicProxy) und gibt NUR diesen Proxy
			//       an den Aufrufer zurueck. Der echte ChatScreen lebt weiter im Container,
			//       der Aufrufer sieht ihn nie direkt. Dadurch kann der Interceptor
			//       VOR jedem Methodenaufruf einspringen.
			//       Wichtig: das funktioniert nur, weil IChatScreen ein Interface ist -
			//       Klassen ohne sichtbares Interface koennte Castle nicht "intercepten".
			//
			// 3) .InterceptedBy(typeof(PerformanceTrackingInterceptor))
			//    -> AUTOFAC-FEATURE: verkettet den genannten Interceptor in die Aufruf-
			//       Pipeline. Mehrere Interceptors sind moeglich - die Reihenfolge folgt
			//       der Registrierung. Jeder Aufruf auf IChatScreen landet ZUERST hier,
			//       dann (nach invocation.Proceed()) erst im echten ChatScreen.
			//
			// 4) .SingleInstance()
			//    -> optional, aber typisch: ein einziger ChatScreen-Proxy fuer den
			//       gesamten Container-Lebenszyklus. Spart Proxy-Allokationen.
			// -----------------------------------------------------------------------------------
			builder.RegisterType<PerformanceTrackingInterceptor>();
			builder.RegisterType<ChatScreen>().As<IChatScreen>()
				.PropertiesAutowired()
				.EnableInterfaceInterceptors()                       // baut den Castle-Proxy um IChatScreen
				.InterceptedBy(typeof(PerformanceTrackingInterceptor)) // verknuepft den Interceptor in die Aufrufkette
				.SingleInstance();

			// Feature: Decorator eingebaut - umhuellt IChatScreen mit Logging. MS DI braucht dafuer Scrutor.
			builder.RegisterDecorator<LoggingChatScreenDecorator, IChatScreen>();

			// Feature: OnActivated-Hook - Callback sobald die Instanz fertig ist (hier: User seeden).
			// MS DI hat keine Lifecycle-Hooks.
			builder.RegisterType<UserRepository>()
				.OnActivated(e => Seed(e.Instance))
				.SingleInstance();

			// Feature: InstancePerLifetimeScope - eine Instanz pro Scope (Pendant zu AddScoped).
			builder.RegisterType<ChatAppAutofac>().InstancePerLifetimeScope();

			// Feature: Feingranulares Disposal - dieser Service wird vom Container NICHT disposed.
			// .ExternallyOwned() = "Eigentuemer kuendert sich selbst" - perfekt fuer Shared-Ressourcen.
			// MS DI hat keinen Mechanismus, um Disposal selektiv zu unterbinden.
			builder.RegisterType<TrackedResourceExternal>().As<TrackedResourceExternal>()
				.ExternallyOwned()
				.InstancePerLifetimeScope();

			// Feature: Normal registrierter IDisposable wird am Scope-Ende automatisch disposed.
			builder.RegisterType<TrackedResourceNormal>().As<TrackedResourceNormal>()
				.InstancePerLifetimeScope();

			// Feature: OnActivated - Lifecycle-Hook, der NACH dem Konstruktor laeuft.
			// Hier setzen wir eine Property, ohne den Konstruktor der Klasse zu vergiften.
			// MS DI kennt das nicht - dort braucht man einen separaten Init-Provider.
			builder.RegisterType<GreetedService>()
				.OnActivated(e => e.Instance.Init("Hallo vom Autofac OnActivated-Hook"))
				.SingleInstance();

			// -----------------------------------------------------------------------------------
			// Feature: Asymmetrische Konstruktor/Property-Injection loest zirkulaere Abhaengigkeit.
			//
			//   CyclicA(string, CyclicB)         -> A holt sich B HART im Konstruktor.
			//   CyclicB(string) + B.A Property    -> B haelt A nur als optionale Property.
			//
			//   Wie loest das den Kreis?
			//     1. Resolve<CyclicA>() -> Container sieht A(string, B).
			//     2. Container resolvet B ZUERST: B(string) verlangt kein A, baut sauber durch.
			//     3. Container baut A mit der fertigen B-Instanz. A.B ist also schon gesetzt.
			//     4. OnActivated-Hook auf A laeuft NACH dem Konstruktor: setzt B.A = aInstanz
			//        (ohne neuen Resolve-Aufruf - der Hook bekommt die fertige Instanz).
			//     Genau EINE Seite des Kreises (B -> A) wird also erst NACH der Konstruktion
			//     verbunden -> der Container muss nie beide gleichzeitig kennen.
			//
			//   Warum KEIN PropertiesAutowired()?
			//     PropertiesAutowired wuerde beim Bau von B versuchen, B.A via Resolve<CyclicA>()
			//     zu befuellen. A's Lambda wuerde dann wieder Resolve<CyclicB>() rufen -> Kreis.
			//     OnActivated hingegen erhaelt die Instanz DIREKT, ohne nochmal zu resolven.
			//
			//   Warum Factory-Lambda statt WithParameter?
			//     A's Konstruktor verlangt (string, CyclicB). Die Lambda-Loesung delegiert an
			//     Autofacs normalen Resolve<CyclicB>()-Pfad - so bekommt A den voll integrierten B.
			//
			//   .SingleInstance() ist hier wichtig: ohne Singleton gaebe es pro Resolve neue
			//   A/B-Instanzen, und die Property-Verknuepfung waere jeweils nur einseitig.
			// -----------------------------------------------------------------------------------
			builder.Register<CyclicA>(ctx => new CyclicA("Autofac", ctx.Resolve<CyclicB>()))
				.OnActivated(e => e.Instance.B.A = e.Instance)            // setzt B.A = a NACHDEM beide Konstruktoren durch sind
				.SingleInstance();                                          // A und B leben als ein einziges Paar

			builder.RegisterType<CyclicB>().As<CyclicB>()
				.WithParameter("container", "Autofac")                     // fix: Konstruktor-Argument "container"
				.SingleInstance();                                          // ohne SingleInstance waere A/B nicht dasselbe Paar

			// Feature: Delegate Factory - Autofac erzeugt den Func automatisch.
			// MessageRouter bekommt Func<string, IMessageService> und kann zur Laufzeit pro Channel
			// den passenden Messenger aus dem Container resolven - ohne eigene Factory-Klasse.
			// In MS DI muss der Func von Hand gebaut werden (siehe MicrosoftSetup).
			builder.Register<MessageRouter>(ctx =>
			{
				var local = ctx.Resolve<ILifetimeScope>();
				Func<string, IMessageService> factory = channel =>
				{
					var t = typeof(IMessageService).Assembly.GetTypes()
						.FirstOrDefault(x => typeof(IMessageService).IsAssignableFrom(x)
							&& !x.IsInterface
							&& x.GetCustomAttribute<MessengerAttribute>() is { } attr
							&& attr.DisplayName == channel)
						?? throw new InvalidOperationException($"Kein Messenger für '{channel}'");
					return (IMessageService)local.Resolve(t);
				};
				return new MessageRouter("Autofac", factory);
			})
			.SingleInstance();

			using var container = builder.Build();

			// Feature: Lifetime Scope - verschachtelter Scope; Autofac kann pro Scope sogar nachregistrieren.
			using var scope = container.BeginLifetimeScope();
			AutofacShowcase.Run(scope);
			scope.Resolve<ChatAppAutofac>().Run();
		}

		private static void Seed(UserRepository users)
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
	}
}
