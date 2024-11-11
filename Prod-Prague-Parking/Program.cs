using System;
using System.Linq;
using System.IO;
using Spectre.Console;
using PragueParking.Models;
using System.Drawing;

namespace PragueParking
{
    class Program
    {
        static ParkeringsHus parkeringsHus = null!;
        static Konfiguration config = null!;
        static Prislista prislista = null!;

        static void Main(string[] args)
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

            try
            {
                config = Konfiguration.LäsInKonfiguration(configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade vid inläsning av konfigurationen: {ex.Message}");
                return;
            }

            parkeringsHus = new ParkeringsHus(config.AntalPlatser, config.PlatsStorlek);

            string prislistaPath = Path.Combine(AppContext.BaseDirectory, "prislista.txt");
            try
            {
                prislista = Prislista.LaddaPrislista(prislistaPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade vid inläsning av prislistan: {ex.Message}");
                return;
            }

            parkeringsHus.LaddaData("data.json");

            while (true)
            {
                Console.Clear();
                AnsiConsole.Write(
                    new FigletText("Prague Parking")
                        .Centered()
                        .Color(Spectre.Console.Color.Red));

                int val = AnsiConsole.Prompt(
                    new SelectionPrompt<int>()
                        .Title("Välj ett alternativ:")
                        .AddChoices(1, 2, 3, 4, 5, 6, 7)
                        .UseConverter(choice => choice switch
                        {
                            1 => "1. Parkera ett fordon",
                            2 => "2. Flytta ett fordon",
                            3 => "3. Ta bort ett fordon",
                            4 => "4. Sök efter ett fordon",
                            5 => "5. Visa parkeringsplatser",
                            6 => "6. Ladda om prislista",
                            7 => "7. Avsluta",
                            _ => choice.ToString(),
                        }));
                Console.Clear();

                switch (val)
                {
                    case 1:
                        Park();
                        break;
                    case 2:
                        Move();
                        break;
                    case 3:
                        Remove();
                        break;
                    case 4:
                        Search();
                        break;
                    case 5:
                        Display();
                        break;
                    case 6:
                        ReloadPriceList();
                        break;
                    case 7:
                        AnsiConsole.MarkupLine("[bold]Tack för att du använde Prague Parking. Hejdå![/]");
                        parkeringsHus.SparaData("data.json");
                        return;
                    default:
                        AnsiConsole.MarkupLine("[red]Ogiltigt val, försök igen.[/]");
                        break;
                }
            }
        }

        static void Park()
        {
            var fordonstyper = config.Fordonstyper.Select(ft => ft.Typ).ToList();
            var t = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Välj fordonstyp:")
                    .AddChoices(fordonstyper));

            AnsiConsole.Markup("Ange registreringsnummer (max 10 tecken): ");
            string? r = Console.ReadLine()?.ToUpper();
            if (string.IsNullOrWhiteSpace(r) || r.Length > 10)
            {
                AnsiConsole.MarkupLine("[red]Ogiltigt registreringsnummer.[/]");
                Pause();
                return;
            }
            if (parkeringsHus.FindFordon(r, out ParkeringsPlats? befintligPlats))
            {
                AnsiConsole.MarkupLine($"[red]Fordon med registreringsnummer {r} är redan parkerat på plats {befintligPlats.PlatsNummer}.[/]");
                Pause();
                return;
            }

            Fordon? nyttFordon = SkapaFordon(t, r);
            if (nyttFordon == null)
            {
                AnsiConsole.MarkupLine("[red]Kunde inte skapa fordonet.[/]");
                Pause();
                return;
            }

            nyttFordon.AnkomstTid = DateTime.Now;

            if (parkeringsHus.ParkeraFordon(nyttFordon))
            {
                AnsiConsole.MarkupLine($"[green]{t} med registreringsnummer {r} har parkerats.[/]");
                parkeringsHus.SparaData("data.json");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Inga lediga parkeringsplatser för {t}.[/]");
            }
            Pause();
        }

        static Fordon? SkapaFordon(string typ, string regNummer)
        {
            return typ switch
            {
                "Bil" => new Bil(regNummer),
                "MC" => new MC(regNummer),
                _ => null,
            };
        }

        static void Remove()
        {
            AnsiConsole.Markup("Ange registreringsnumret på fordonet som ska tas bort: ");
            string? r = Console.ReadLine()?.ToUpper();
            if (string.IsNullOrWhiteSpace(r))
            {
                AnsiConsole.MarkupLine("[red]Ogiltigt registreringsnummer.[/]");
                Pause();
                return;
            }
            if (parkeringsHus.TaBortFordon(r, out Fordon? fordon))
            {
                TimeSpan parkeringstid = DateTime.Now - fordon.AnkomstTid;
                string formattedTime = FormatTimeSpan(parkeringstid);
                int pris = prislista.BeräknaPris(fordon, parkeringstid);

                AnsiConsole.MarkupLine($"[yellow]Fordonet har tagits bort från parkeringsplatsen.[/]");
                AnsiConsole.MarkupLine($"[yellow]Parkerad tid: {formattedTime}.[/]");
                AnsiConsole.MarkupLine($"[yellow]Parkeringsavgiften är {pris} CZK.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            }

            parkeringsHus.SparaData("data.json");
            Pause();
        }

        static void Move()
        {
            AnsiConsole.Markup("Ange registreringsnumret på fordonet som ska flyttas: ");
            string? r = Console.ReadLine()?.ToUpper();

            AnsiConsole.Markup($"Ange nytt parkeringsplatsnummer (1-{config.AntalPlatser}): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n < 1 || n > config.AntalPlatser)
            {
                AnsiConsole.MarkupLine("[red]Ogiltigt parkeringsplatsnummer.[/]");
                Pause();
                return;
            }

            if (parkeringsHus.FlyttaFordon(r, n))
            {
                AnsiConsole.MarkupLine($"[green]Fordonet har flyttats till plats {n}.[/]");
                parkeringsHus.SparaData("data.json");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Kunde inte flytta fordonet.[/]");
            }
            Pause();
        }

        static void Search()
        {
            AnsiConsole.Markup("Ange registreringsnummer att söka efter: ");
            string? r = Console.ReadLine()?.ToUpper();
            if (string.IsNullOrWhiteSpace(r))
            {
                AnsiConsole.MarkupLine("[red]Ogiltigt registreringsnummer.[/]");
                Pause();
                return;
            }
            if (parkeringsHus.FindFordon(r, out ParkeringsPlats? plats))
            {
                AnsiConsole.MarkupLine($"[green]Fordon med registreringsnummer {r} är parkerat på plats {plats.PlatsNummer}.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Fordonet hittades inte.[/]");
            }
            Pause();
        }

        static void Display()
        {
            AnsiConsole.Clear();

            var table = new Table
            {
                Border = TableBorder.Rounded
            };
            table.AddColumn("Platsnummer");
            table.AddColumn("Fordon");

            foreach (var plats in parkeringsHus.ParkeringsPlatser)
            {
                string fordonInfo = plats.FordonPåPlatsen.Count > 0
                    ? string.Join(", ", plats.FordonPåPlatsen.Select(f => $"{f.Typ} ({f.RegistreringsNummer})"))
                    : "Ledig";
                table.AddRow(plats.PlatsNummer.ToString(), fordonInfo);
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("Tryck på en tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        static void ReloadPriceList()
        {
            string prislistaPath = Path.Combine(AppContext.BaseDirectory, "prislista.txt");
            try
            {
                prislista = Prislista.LaddaPrislista(prislistaPath);
                AnsiConsole.MarkupLine("[green]Prislistan har laddats om.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Kunde inte ladda om prislistan: {ex.Message}[/]");
            }
            Pause();
        }

        static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1)
                return $"{timeSpan.Seconds} sekunder";
            else if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minuter";
            else if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} timmar, {timeSpan.Minutes} minuter";
            else
                return $"{(int)timeSpan.TotalDays} dagar, {timeSpan.Hours} timmar";
        }

        static void Pause()
        {
            AnsiConsole.MarkupLine("Tryck på en tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
}