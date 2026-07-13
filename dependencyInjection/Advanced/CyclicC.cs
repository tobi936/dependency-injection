using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	// Feature: Konstruktor-Bruecke fuer den MS-DI-Zyklus-Test.
	// CyclicC versucht A + B im KONSTRUKTOR zu holen. Da A und B im MS-Setup nicht
	// per PropertiesAutowired aufgeloest werden, entsteht ein klassischer Konstuktor-Zyklus,
	// den Microsoft DI nicht aufbrechen kann -> InvalidOperationException beim Resolve.
	internal sealed class CyclicC
	{
		public string Container { get; }
		public CyclicA A { get; }
		public CyclicB B { get; }

		public CyclicC(string container, CyclicA a, CyclicB b)
		{
			Container = container;
			A = a;
			B = b;
			ContainerMetrics.Line(container, "CyclicC created (MS DI: versucht A + B per Konstruktor zu holen - zirkulär)", "yellow");
		}
	}
}
