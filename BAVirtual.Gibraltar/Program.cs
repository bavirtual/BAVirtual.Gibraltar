using BAVirtual.Gibraltar.Addons;
using BAVirtual.Gibraltar.Interfaces;
using Spectre.Console;

namespace BAVirtual.Gibraltar
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AddonSelection();
        }

        public static void AddonSelection()
        {
            var addon = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What addon are we adding this data to?")
                    .PageSize(3)
                    .AddChoices([
                        "FENIX"
                    ]));

            ProcessAddon(addon);

            var again = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Do you want to re-run the process?")
                    .PageSize(3)
                    .AddChoices([
                        "YES",
                        "NO"
                    ]));

            if (again == "NO")
            {
                Environment.Exit(1);
            }
            else
            {
                Console.Clear();
                AddonSelection();
            }
        }

        private static void ProcessAddon(string addon_str)
        {
            IAddon? addon = null;

            switch (addon_str)
            {
                case "FENIX":
                    addon = new Fenix();
                    break;
                default:
                    break;
            }

            if (addon != null)
            {
                try
                {
                    ProcessResponse success = addon.ProcessGibraltarData();
                    if (success.Success)
                    {
                        AnsiConsole.MarkupLine($"[green3_1]Gibraltar Data Inserted Successfully![/]");
                        Console.WriteLine("");
                        Thread.Sleep(2500);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[bold red]We were unable to insert the Gibraltar data...[/]");
                        AnsiConsole.WriteLine(string.IsNullOrEmpty(success.Info) ? "No Information Provided" : success.Info);
                        Console.WriteLine("");
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                    AnsiConsole.WriteLine("Press Enter to exit..");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold red]WE DON'T SUPPORT THAT ADDON YET![/]");
                Thread.Sleep(2500);
                Environment.Exit(1);
            }
        }
    }

    public class ProcessResponse
    {
        public bool Success { get; set; }
        public string? Info { get; set; }

        public ProcessResponse()
        {
            Success = false;
        }
    }
}