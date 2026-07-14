using System.Diagnostics;
using Castle.DynamicProxy;

namespace dependencyInjection.Logging
{
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
