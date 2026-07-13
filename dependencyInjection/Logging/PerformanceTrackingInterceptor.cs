using System.Diagnostics;
using Castle.DynamicProxy;

namespace dependencyInjection.Logging
{
	// =====================================================================
	// AOP-Performance-Tracking via Castle DynamicProxy.
	//
	// Was ist ein "Interceptor" in Castle.DynamicProxy?
	//   Castle erzeugt zur Laufzeit eine dynamische Subklasse (bzw. einen
	//   Implementierungs-Proxy), die das Interface IChatScreen implementiert
	//   und intern eine Referenz auf den echten ChatScreen haelt. Jeder
	//   Methodenaufruf auf IChatScreen geht ZUNAECHST in den Interceptor
	//   - erst DORT entscheidet der Code, ob und wann die Originalmethode
	//   tatsaechlich laeuft.
	//
	// Wichtige Begriffe rund um IInvocation:
	//   - invocation.Method     : das MethodInfo-Objekt des eigentlichen Aufrufs
	//   - invocation.Arguments  : die uebergebenen Parameter (z.B. from, to, channel)
	//   - invocation.TargetType : der Typ, auf dem die Methode eigentlich lebt
	//                             (hier: typeof(ChatScreen))
	//
	// Was macht invocation.Proceed() genau?
	//   Proceed() ist der "Weiter-Knopf" der Interceptor-Pipeline. Erst beim
	//   Aufruf von Proceed() wird die naechste Station in der Kette ausgeloest
	//   - das koennen weitere Interceptors sein ODER die eigentliche Methode
	//   des Zielobjekts. Lassen wir Proceed() weg, wird die Originalmethode
	//   komplett uebergangen. Rufen wir Proceed() mehrfach auf, laeuft die
	//   Originalmethode mehrfach (z.B. fuer Retry-Logik). Genau EINE Proceed()-
	//   Zeile entspricht also "fuehre die Zielmethode (ggf. plus weitere
	//   Interceptors) genau einmal aus".
	//
	// Wie wird die Zeitmessung "um die Zielmethode herum" gelegt?
	//   1. Stopwatch.StartNew() VOR Proceed()  -> misst ab jetzt.
	//   2. try { invocation.Proceed(); }       -> fuehrt die Originalmethode aus.
	//   3. finally { stopwatch.Stop(); ... }   -> Stopp-Zeitpunkt ist garantiert,
	//                                             auch wenn die Originalmethode
	//                                             eine Exception wirft.
	//   So umschliesst der Interceptor den Aufruf wie eine Klammer, OHNE den
	//   eigentlichen Service (ChatScreen) anzufassen - der weiss nicht einmal,
	//   dass er gemessen wird.
	// =====================================================================
	internal class PerformanceTrackingInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			// Zeitnahme starten, BEVOR die Zielmethode laeuft. Der try/finally-
			// Block stellt sicher, dass stopwatch.Stop() IMMER erreicht wird -
			// auch dann, wenn die Originalmethode eine Exception wirft.
			var stopwatch = Stopwatch.StartNew();

			try
			{
				// Hier geht's rein: Castle leitet den Aufruf an die naechste
				// Pipeline-Station weiter. In unserem Fall ist das die echte
				// ChatScreen-Methode (plus ggf. weitere Interceptors dazwischen).
				// Erst NACH Proceed() ist die Originalmethode komplett durchgelaufen.
				invocation.Proceed();
			}
			finally
			{
				// Egal ob Erfolg oder Exception: hier stoppen wir die Uhr.
				// TotalMilliseconds mit :F3 formatiert die Ausgabe auf Mikrosekunden
				// genau (3 Nachkommastellen) - praxisnah fuer Profiling-Output.
				stopwatch.Stop();
				Console.WriteLine($"[PERF] {invocation.TargetType?.Name}.{invocation.Method.Name}() took {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
			}
		}
	}
}
