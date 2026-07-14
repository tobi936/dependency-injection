using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	internal sealed class Greeting
	{
		private readonly string name;

		public Greeting(string name)
		{
			this.name = name;
		}

		public string Say()
		{
			return $"Hallo, {name}!";
		}
	}

	internal sealed class GreetingConsumer
	{
		private readonly Func<string, Greeting> factory;

		public GreetingConsumer(Func<string, Greeting> factory)
		{
			this.factory = factory;
		}

		public void Demo(string container)
		{
			ContainerMetrics.Line(container, factory("Anna").Say(), "cyan");
			ContainerMetrics.Line(container, factory("Ben").Say(), "cyan");
		}
	}
}
