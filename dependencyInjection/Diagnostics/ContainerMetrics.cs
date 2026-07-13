using Spectre.Console;

namespace dependencyInjection.Diagnostics
{
	// Feature: Metrik-Schicht - kleine "printf"-Helfer, die jeder Container fuer seine Demo-Ausgabe nutzt.
	// So sieht man im laufenden Konsolen-Output, welcher Container was wann instanziiert/disposed.
	// Vorteil fuer den Architekten: direkter visueller Vergleich Autofac vs. Microsoft DI nebeneinander.
	internal static class ContainerMetrics
	{
		// Einzeiliger Log mit Containername als Prefix in grau.
		public static void Line(string container, string message, string color = "white")
		{
			AnsiConsole.MarkupLine($"[grey][[{Markup.Escape(container)}]][/] [{color}]{Markup.Escape(message)}[/]");
		}

		// Hervorgehobenes Event - Label in Farbe, Detail in grau. Fuer Status-Meldungen (INSTANTIATED, DISPOSED, ...).
		public static void Event(string container, string label, string detail, string color)
		{
			AnsiConsole.MarkupLine($"[grey][[{Markup.Escape(container)}]][/] [bold {color}]{Markup.Escape(label)}[/] [grey]-[/] {Markup.Escape(detail)}");
		}

		// Sektionstrenner im Konsolen-Output - macht den Wechsel zwischen den Demo-Bloecken sichtbar.
		public static void Header(string container, string title)
		{
			AnsiConsole.Write(new Rule($"[bold yellow]{Markup.Escape(container)}[/] [grey]-[/] {Markup.Escape(title)}").LeftJustified());
		}
	}
}
