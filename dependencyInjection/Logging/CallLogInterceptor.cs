using Castle.DynamicProxy;

namespace dependencyInjection.Logging
{
	internal class CallLogInterceptor : IInterceptor
	{
		private readonly IChatLogger logger;

		public CallLogInterceptor(IChatLogger logger)
		{
			this.logger = logger;
		}

		public void Intercept(IInvocation invocation)
		{
			logger.Log($"[AOP] {invocation.TargetType.Name}.{invocation.Method.Name}()");
			invocation.Proceed();
		}
	}
}
