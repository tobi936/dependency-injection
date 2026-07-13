using Microsoft.Extensions.DependencyInjection;
using dependencyInjection.Advanced;
using dependencyInjection.Diagnostics;
using dependencyInjection.Model;

namespace dependencyInjection.Hosting
{
	// Feature: Pendant zu AutofacShowcase - demonstriert die gleichen vier Faelle im MS-DI-Zweig.
	// Hier fehlen die Autofac-Features (Func-Factory, ExternallyOwned, OnActivated, Property-Injection).
	// Der Architekt sieht die Demos NEBENEINANDER und kann direkt vergleichen, was nur Autofac kann.
	internal static class MicrosoftShowcase
	{
		public static void Run(IServiceProvider provider)
		{
			const string container = "Microsoft DI";
			ContainerMetrics.Header(container, "Erweiterte Features");

			FuncFactoryDemo(container, provider);
			DisposalDemo(container, provider);
			OnActivatedDemo(container, provider);
			CircularDemo(container, provider);
		}

		// Demo 1: Func-Factory. In MS DI muss der Func von Hand im Registrierungs-Lambda gebaut werden.
		// ActivatorUtilities.CreateInstance<T>(sp) ist die MS-Form von "Container.Resolve<T>() mit Konstruktor-Injection".
		// Optisch gleich wie bei Autofac, aber ohne Auto-Generation.
		private static void FuncFactoryDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Delegate Factory: Func<string, IMessageService> via ActivatorUtilities");

			var router = provider.GetRequiredService<MessageRouter>();
			var users = provider.GetRequiredService<UserRepository>();
			var from = users.Users.First();
			var to = users.Users.Skip(1).First();

			router.Dispatch("SMS", from, to.Name, "Hi via Func", users);
			router.Dispatch("WhatsApp", from, "alle (Broadcast)", "Hallo alle", users);
		}

		// Demo 2: Disposal ohne .ExternallyOwned().
		// Beide Services werden disposed, sobald der Container geschlossen wird.
		// Es gibt KEINEN Weg in MS DI, das Disposal fuer einzelne Services zu unterbinden.
		private static void DisposalDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Disposal: kein .ExternallyOwned() - beide Services werden disposed");

			provider.GetRequiredService<TrackedResourceNormalMs>();
			provider.GetRequiredService<TrackedResourceExternalMs>();
		}

		// Demo 3: Lifecycle-Hook -Workaround in MS DI.
		// Statt .OnActivated() muessen wir die Init-Logik in den Konstruktor oder das Factory-Lambda stecken.
		// Hier: Registrierungs-Lambda ruft den richtigen Konstruktor mit dem Greeting-String auf.
		private static void OnActivatedDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Lifecycle-Hook: kein OnActivated - manuelle Init im Wrapper");

			provider.GetRequiredService<GreetedServiceMs>().SayHello();
		}

		// Demo 4: Zirkulaere Abhaengigkeit - Microsoft DI crasht.
		// CyclicC versucht A + B im Konstruktor zu holen. A und B haben kein PropertiesAutowired-Pendant,
		// also entsteht ein klassischer Konstuktor-Zyklus -> InvalidOperationException.
		// Der try/catch ist dafuer da, dass die App weiter laeuft und der Architekt die Fehlermeldung sieht.
		private static void CircularDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Zirkulare Abhängigkeit: Microsoft DI crasht mit CircularDependencyException");

			try
			{
				var c = provider.GetRequiredService<CyclicC>();
				c.A.Touch();
				c.B.Touch();
			}
			catch (Exception ex)
			{
				ContainerMetrics.Event(container, "CRASH", ex.GetType().Name + ": " + ex.Message, "red");
			}
		}
	}

	// MS-spezifische Wrapper (Namespace-Kollision mit den Autofac-Wrappern vermeiden).
	internal sealed class TrackedResourceNormalMs : IDisposable
	{
		private readonly TrackedResource inner;

		public TrackedResourceNormalMs()
		{
			inner = new TrackedResource("Microsoft DI", "NormalOwnedResource", externallyOwned: false);
		}

		public void Dispose()
		{
			inner.Dispose();
		}
	}

	internal sealed class TrackedResourceExternalMs : IDisposable
	{
		private readonly TrackedResource inner;

		public TrackedResourceExternalMs()
		{
			inner = new TrackedResource("Microsoft DI", "ExternallyOwnedResource", externallyOwned: true);
		}

		public void Dispose()
		{
			inner.Dispose();
		}
	}

	// Workaround: Greeting wird direkt im Konstruktor uebergeben.
	// In MS DI ist "Init-Logik nach Konstruktion" nicht vorgesehen -
	// der saubere Weg ist, den Wert via Konstruktor reinzureichen.
	internal sealed class GreetedServiceMs
	{
		public string Greeting { get; }
		public DateTime CreatedAt { get; }

		public GreetedServiceMs(string greeting)
		{
			Greeting = greeting;
			CreatedAt = DateTime.Now;
			ContainerMetrics.Event("Microsoft DI", "POST-CTOR-INIT", $"GreetedService im Wrapper initialisiert mit '{greeting}' um {CreatedAt:HH:mm:ss.fff}", "magenta");
		}

		public void SayHello()
		{
			ContainerMetrics.Line("Microsoft DI", $"GreetedService meldet: {Greeting}", "white");
		}
	}
}
