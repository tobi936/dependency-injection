using dependencyInjection.Diagnostics;

namespace dependencyInjection.Advanced
{
	// =====================================================================
	// CyclicA / CyclicB - Aufbrechen einer zirkulaeren Abhaengigkeit.
	//
	// Warum ueberhaupt zirkulaer?
	//   A braucht B UND B braucht A. Wuerden wir BEIDE Seiten per Konstruktor
	//   injizieren, gaebe es keinen "Startpunkt" mehr: Um A zu bauen, braucht
	//   der Container B; um B zu bauen, braucht der Container A. Das fuehrt
	//   zu einer unendlichen Aufrufkette und am Ende zu einer
	//   InvalidOperationException ("circular dependency").
	//
	// Loesungs-Idee: Konstruktor- vs. Property-Injection asymmetrisch einsetzen.
	//   - CyclicA holt sich B im KONSTRUKTOR (harte, sofort benoetigte Abhaengigkeit).
	//   - CyclicB haelt A nur als PROPERTY (weiche, optionale Abhaengigkeit).
	//
	// Warum bricht das den Kreis?
	//   1. Der Container startet beim Auftrag "Resolve<CyclicA>()".
	//   2. Um A zu bauen, sieht der Container: A-Konstruktor verlangt (string, CyclicB).
	//   3. Container resolvet zuerst CyclicB - B's Konstruktor verlangt nur einen string,
	//      also KEIN A. B wird sauber gebaut (B.A = null zu diesem Zeitpunkt).
	//   4. Jetzt baut der Container A mit der gerade gebauten B-Instanz.
	//      A.B ist also schon gesetzt, sobald A's Konstruktor durchlaeuft.
	//   5. PropertiesAutowired() laeuft NACH den Konstruktoren:
	//        - bei A: A.B hat keinen Setter (read-only), wird uebersprungen.
	//        - bei B: B.A hat einen Setzer, Autofac setzt B.A = aInstanz.
	//   Der Container muss also nie "beide gleichzeitig" kennen.
	//
	// Bei Microsoft DI wuerde dieser Trick scheitern, weil MS DI KEIN
	// PropertiesAutowired() kennt. B.A bleibt dort null. CyclicC versucht
	// zusaetzlich, A und B im eigenen Konstruktor zu holen - was dann eine
	// echte zirkulaere Konstruktor-Abhaengigkeit ausloest.
	// =====================================================================

	internal sealed class CyclicA
	{
		// Identifiziert am Ende, welcher Container (Autofac vs. MS DI) das Objekt gebaut hat.
		public string Container { get; }

		// Read-only Property: A haelt B ausschliesslich ueber den Konstruktor.
		// PropertiesAutowired() findet hier KEINEN Setter und laesst die Property in Ruhe.
		// Das ist asymmetrisch zu B.A - und genau diese Asymmetrie bricht den Kreis.
		public CyclicB B { get; }

		// Konstruktor: A verlangt B HARTE (Position 2). Container muss B also VOR A bauen.
		// Da B selbst kein A im Konstruktor verlangt, ist das problemlos moeglich.
		public CyclicA(string container, CyclicB b)
		{
			Container = container;
			B = b;
		}

		public void Touch()
		{
			// Zeigt im Konsolen-Output, dass B direkt nach dem Resolve gesetzt ist
			// (B wurde im Konstruktor uebergeben, also schon vor PropertiesAutowired()).
			ContainerMetrics.Line(Container, $"CyclicA.Touch() called, B is {(B is null ? "null" : "set")}", "yellow");
		}
	}

	internal sealed class CyclicB
	{
		public string Container { get; }

		// Rueckverweis auf A - BEWUSST als Property, NICHT als Konstruktor-Parameter.
		// Wuerde A hier im Konstruktor verlangt, gaebe es den klassischen
		// Konstruktor-Zyklus: A braucht B im Konstruktor, B braucht A im Konstruktor.
		// Mit der Property wird der Kreis auf eine einseitige "harte" Abhaengigkeit
		// (CyclicA -> CyclicB per Konstruktor) reduziert, plus eine "weiche"
		// Rueckverknuepfung (CyclicB -> A per Property).
		//
		// Autofac's .PropertiesAutowired() sorgt dafuer, dass diese Property
		// NACH den Konstruktoren mit der bereits gebauten CyclicA-Instanz gefuellt wird.
		public CyclicA? A { get; set; }

		public CyclicB(string container)
		{
			Container = container;
			// A wird hier ABSICHTLICH nicht angefasst - das ist der Trick.
			// Die Zuweisung passiert spaeter ueber Property-Injection.
		}

		public void Touch()
		{
			ContainerMetrics.Line(Container, $"CyclicB.Touch() called, A is {(A is null ? "null" : "set")}", "yellow");
		}
	}
}
