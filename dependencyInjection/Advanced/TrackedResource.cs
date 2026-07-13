using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	// Feature: Demo-Service, der im Konstruktor UND in Dispose() einen Konsolen-Event feuert.
	// Wird fuer zwei Varianten registriert:
	//   1. Normal -> Disposal durch den Container
	//   2. .ExternallyOwned() -> KEIN Disposal durch den Container
	// Der Architekt sieht im Konsolen-Output direkt, welche Instanz ueberlebt.
	internal sealed class TrackedResource : ITrackedResource, IDisposable
	{
		private readonly string container;
		private readonly bool externallyOwned;

		public string Name { get; }

		public TrackedResource(string container, string name, bool externallyOwned)
		{
			this.container = container;
			this.Name = name;
			this.externallyOwned = externallyOwned;
			// Gruen = "wurde geboren". Beim Disposal faerben wir rot, wenn es ExternallyOwned ist (unerwartet!).
			ContainerMetrics.Event(container, "INSTANTIATED", $"{name} (ExternallyOwned={externallyOwned})", "green");
		}

		public void Dispose()
		{
			// Rot = Disposal trotz ExternallyOwned (sollte im Normalfall NICHT passieren).
			// Gruen = planmaessiges Disposal durch den Container.
			var color = externallyOwned ? "red" : "green";
			ContainerMetrics.Event(container, "DISPOSED", Name, color);
		}
	}
}
