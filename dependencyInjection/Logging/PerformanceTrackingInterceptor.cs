using System.Diagnostics;
using Castle.DynamicProxy;

namespace dependencyInjection.Logging
{
	// Feature: AOP-Performance-Tracking via Castle DynamicProxy.
	// Stopwatch wird VOR invocation.Proceed() gestartet und NACH dem Aufruf gestoppt.
	// So misst man die tatsaechliche Dauer der Original-Methode (inklusive Decorator, falls vorhanden).
	// Architekten-Demo: kein einziger Aufruf in ChatScreen selbst wurde veraendert -
	// das Tracking ist komplett transparent ueber den Interface-Proxy eingeklinkt.
	internal class PerformanceTrackingInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				invocation.Proceed();
			}
			finally
			{
				stopwatch.Stop();
				Console.WriteLine($"[PERF] {invocation.TargetType?.Name}.{invocation.Method.Name}() took {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
			}
		}
	}
}
