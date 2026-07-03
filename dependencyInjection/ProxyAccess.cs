using System.Runtime.CompilerServices;

// Interception noetig: Castle DynamicProxy baut Proxies in dieser Assembly.
// Damit es unsere internen Interfaces (IMessageService) proxien darf, geben wir sie frei.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
