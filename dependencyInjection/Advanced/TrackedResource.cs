using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
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
			ContainerMetrics.Event(container, "INSTANTIATED", $"{name} (ExternallyOwned={externallyOwned})", "green");
		}

		public void Dispose()
		{
			var color = externallyOwned ? "red" : "green";
			ContainerMetrics.Event(container, "DISPOSED", Name, color);
		}
	}
}
