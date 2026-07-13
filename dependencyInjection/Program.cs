using System.Text;
using Spectre.Console;
using dependencyInjection.Hosting;

Console.OutputEncoding = Encoding.UTF8;

var container = AnsiConsole.Prompt(
	new SelectionPrompt<string>()
		.Title("Welcher [bold]DI-Container[/]?")
		.AddChoices("Autofac", "Microsoft DI"));

var demo = AnsiConsole.Prompt(
	new SelectionPrompt<string>()
		.Title("Was zeigen?")
		.AddChoices("Basis-Chat", "Feature-Showcase", "Beides"));

var mode = demo switch
{
	"Basis-Chat" => DemoMode.BasisChat,
	"Feature-Showcase" => DemoMode.Showcase,
	_ => DemoMode.Beides
};

if (container == "Autofac")
{
	AutofacSetup.Run(mode);
}
else
{
	MicrosoftSetup.Run(mode);
}
