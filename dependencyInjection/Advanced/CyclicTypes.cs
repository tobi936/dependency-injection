using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	// Feature: Bewusst zyklisch verschaltete Klassen fuer den "CircularDependency"-Demo-Block.
	// Beachte: das GEGENSEITIGE Feld ist als PROPERTY deklariert, nicht als Konstruktor-Parameter.
	// Grund: Autofac kann PropertiesAutowired() nutzen, um den Zyklus aufzubrechen -
	// die zweite Seite wird erst NACH der Konstruktion gesetzt. MS DI kann das nicht.
	internal sealed class CyclicA
	{
		public string Container { get; }
		public CyclicB? B { get; set; }

		public CyclicA(string container)
		{
			Container = container;
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
