using Spectre.Console;

namespace dependencyInjection.Diagnostics
{
	internal static class ContainerMetrics
	{
		public static void Line(string container, string message, string color = "white")
		{
			AnsiConsole.MarkupLine($"[grey][[{Markup.Escape(container)}]][/] [{color}]{Markup.Escape(message)}[/]");
		}

		public static void Event(string container, string label, string detail, string color)
		{
			AnsiConsole.MarkupLine($"[grey][[{Markup.Escape(container)}]][/] [bold {color}]{Markup.Escape(label)}[/] [grey]-[/] {Markup.Escape(detail)}");
		}

		public static void Header(string container, string title)
		{
			AnsiConsole.Write(new Rule($"[bold yellow]{Markup.Escape(container)}[/] [grey]-[/] {Markup.Escape(title)}").LeftJustified());
		}
	}
}
