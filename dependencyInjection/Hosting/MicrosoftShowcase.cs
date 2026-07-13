using System;
using Microsoft.Extensions.DependencyInjection;
using dependencyInjection.Advanced;
using dependencyInjection.Diagnostics;

namespace dependencyInjection.Hosting
{
	internal static class MicrosoftShowcase
	{
		public static void Run(IServiceProvider provider)
		{
			const string container = "Microsoft DI";
			ContainerMetrics.Header(container, "Standard Features & Einschränkungen");

			FuncFactoryDemo(container, provider);
			WaitForUser();

			DisposalDemo(container, provider);
			WaitForUser();

			OnActivatedDemo(container, provider);
			WaitForUser();

			CircularDemo(container, provider);
			WaitForUser();
		}

		private static void WaitForUser()
		{
			Console.WriteLine();
			Console.WriteLine("Drücke eine beliebige Taste, um fortzufahren...");
			Console.ReadKey(true);
			Console.Clear();
		}

		private static void FuncFactoryDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Delegate Factory Test");
			ContainerMetrics.Line(container, "Microsoft DI unterstützt keine impliziten Func-Fabriken.", "red");
		}

		private static void DisposalDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Disposal Test");

			try
			{
				provider.GetRequiredService<TrackedResourceNormalMs>();
				provider.GetRequiredService<TrackedResourceExternalMs>();
			}
			catch (Exception ex)
			{
				ContainerMetrics.Event(container, "INFO", ex.Message, "yellow");
			}
		}

		private static void OnActivatedDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Lifecycle-Hook Test");

			try
			{
				provider.GetRequiredService<GreetedServiceMs>().SayHello();
			}
			catch (Exception ex)
			{
				ContainerMetrics.Event(container, "INFO", ex.Message, "yellow");
			}
		}

		private static void CircularDemo(string container, IServiceProvider provider)
		{
			ContainerMetrics.Header(container, "Zirkulare Abhängigkeit Test");

			try
			{
				var a = provider.GetRequiredService<CyclicA>();
				a.Touch();
			}
			catch (Exception ex)
			{
				ContainerMetrics.Event(container, "CRASH", $"Exception abgefangen: {ex.Message}", "red");
			}
		}
	}

	internal sealed class TrackedResourceNormalMs : IDisposable
	{
		private readonly TrackedResource inner;

		public TrackedResourceNormalMs()
		{
			inner = new TrackedResource("Microsoft DI", "NormalOwnedResource", false);
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
			inner = new TrackedResource("Microsoft DI", "ExternallyOwnedResource", true);
		}

		public void Dispose()
		{
			inner.Dispose();
		}
	}

	internal sealed class GreetedServiceMs
	{
		public string Greeting { get; private set; } = string.Empty;

		public void Init(string greeting)
		{
			Greeting = greeting;
		}

		public void SayHello()
		{
			ContainerMetrics.Line("Microsoft DI", $"GreetedService meldet: {Greeting}", "white");
		}
	}
}