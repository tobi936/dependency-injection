using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	internal sealed class CyclicA
	{
		public string Container { get; }

		public CyclicB B { get; }

		public CyclicA(string container, CyclicB b)
		{
			Container = container;
			B = b;
		}

		public void Touch()
		{
			ContainerMetrics.Line(Container, $"CyclicA.Touch() called, B is {(B is null ? "null" : "set")}", "yellow");
		}
	}

	internal sealed class CyclicB
	{
		public string Container { get; }

		public CyclicA? A { get; set; }

		public CyclicB(string container)
		{
			Container = container;
		}

		public void Touch()
		{
			ContainerMetrics.Line(Container, $"CyclicB.Touch() called, A is {(A is null ? "null" : "set")}", "yellow");
		}
	}
}
