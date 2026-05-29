using RadImplementationProject.Tasks;
using RadImplementationProject.Hashing;
using Spectre.Console.Cli;
using Spectre.Console;

namespace RadImplementationProject
{
    public static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var app = new CommandApp();
                app.Configure(cfg =>
                {
                    cfg.SetApplicationName("RAD Implementation Project");
                    cfg.AddCommand<HashFunctionsCommand>("hash-functions")
                        .WithDescription("Compare speed of multiply-shift vs. multiply-mod-prime");
                    cfg.AddCommand<QuadraticSumsCommand>("quadratic-sums")
                        .WithDescription("Second moment calculation speed of multiply-shift vs. multiply-mod-prime");
                    cfg.AddCommand<CountSketchCommand>("count-sketch")
                        .WithDescription("Count-Sketch experiment with 100 estimates");
                });
                return app.Run(args);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[bold red]Error:[/] ", ex);
                return 1;
            }
        }
    }
}
