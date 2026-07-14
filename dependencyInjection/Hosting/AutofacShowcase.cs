using System;
using Autofac;
using dependencyInjection.Advanced;
using dependencyInjection.Diagnostics;

namespace dependencyInjection.Hosting
{
	internal static class AutofacShowcase
	{
		public static void Run(ILifetimeScope scope)
		{
			const string container = "Autofac";
			ContainerMetrics.Header(container, "Erweiterte Features");

			DisposalDemo(container, scope);
			WaitForUser();

			OnActivatedDemo(container, scope);
			WaitForUser();

			CircularDemo(container, scope);
			WaitForUser();

			DelegateFactoryDemo(container, scope);
			WaitForUser();
		}

		private static void WaitForUser()
		{
			Console.WriteLine();
			Console.WriteLine("Drücke eine beliebige Taste, um die nächste Demo anzuzeigen...");
			Console.ReadKey(true);
			Console.Clear();
		}

		private static void DisposalDemo(string container, ILifetimeScope scope)
		{
			ContainerMetrics.Header(container, "Feingranulares Disposal: .ExternallyOwned()");

			scope.Resolve<TrackedResourceNormal>();
			scope.Resolve<TrackedResourceExternal>();
		}

		private static void OnActivatedDemo(string container, ILifetimeScope scope)
		{
			ContainerMetrics.Header(container, "Lifecycle-Hook: .OnActivated(...)");

			scope.Resolve<GreetedService>().SayHello();
		}

		private static void CircularDemo(string container, ILifetimeScope scope)
		{
			ContainerMetrics.Header(container, "Zirkulare Abhängigkeit: PropertiesAutowired()");

			var a = scope.Resolve<CyclicA>();
			a.Touch();
			a.B?.Touch();
		}

		private static void DelegateFactoryDemo(string container, ILifetimeScope scope)
		{
			ContainerMetrics.Header(container, "Delegate-Factory: Func<string, T> ohne eigene Registrierung");

			scope.Resolve<GreetingConsumer>().Demo(container);
		}
	}

	internal sealed class TrackedResourceNormal : IDisposable
	{
		private readonly TrackedResource inner;

		public TrackedResourceNormal()
		{
			inner = new TrackedResource("Autofac", "NormalOwnedResource", false);
		}

		public void Dispose()
		{
			inner.Dispose();
		}
	}

	internal sealed class TrackedResourceExternal : IDisposable
	{
		private readonly TrackedResource inner;

		public TrackedResourceExternal()
		{
			inner = new TrackedResource("Autofac", "ExternallyOwnedResource", true);
		}

		public void Dispose()
		{
			inner.Dispose();
		}
	}

	internal sealed class GreetedService
	{
		public string Greeting { get; private set; } = string.Empty;
		public DateTime CreatedAt { get; private set; }

		public GreetedService()
		{
			CreatedAt = DateTime.Now;
		}

		internal void Init(string greeting)
		{
			Greeting = greeting;
			ContainerMetrics.Event("Autofac", "ON-ACTIVATED", $"GreetedService initialisiert mit '{greeting}' um {CreatedAt:HH:mm:ss.fff}", "magenta");
		}

		public void SayHello()
		{
			ContainerMetrics.Line("Autofac", $"GreetedService meldet: {Greeting}", "white");
		}
	}
}