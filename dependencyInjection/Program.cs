using System.Text;
using Spectre.Console;
using dependencyInjection;

Console.OutputEncoding = Encoding.UTF8;

var container = AnsiConsole.Prompt(
	new SelectionPrompt<string>()
		.Title("Welcher [bold]DI-Container[/]?")
		.AddChoices("Autofac", "Microsoft DI"));

if (container == "Autofac")
{
	AutofacSetup.Run();
}
else
{
	MicrosoftSetup.Run();
}
