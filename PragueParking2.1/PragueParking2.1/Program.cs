using PragueParking2._1.Interfaces;
using PragueParking2._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using PragueParking.DataAccess;

namespace PragueParking2._1
{
    class Program
    {
        private static ParkingGarage? _garage;
        private static FileManager _fileManager = new FileManager();

        static void Main(string[] args)
        {
            InitializeGarage();

            AnsiConsole.Clear();
            ShowWelcomeScreen();

            bool running = true;
            while (running)
            {
                var choice = ShowMainMenu();

                switch (choice)
                {
                    case "Incheckning":
                        CheckInVehicle();
                        break;
                    case "Utcheckning":
                        CheckOutVehicle();
                        break;
                    case "Sök":
                        FindVehicle();
                        break;
                    case "Visa karta":
                        ShowParkingMap();
                        break;
                    case "Prislista":
                        ShowPriceList();
                        break;
                    case "Ladda om konfiguration":
                        ReloadConfiguration();
                        break;
                    case "Avsluta":
                        running = false;
                        break;
                }

                if (running)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Markup("[grey]Tryck på en tangent för att fortsätta...[/]");
                    Console.ReadKey();
                }
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[yellow]Tack för att du använde Prague Parking 2.1! Hej då![/]");
        }

        static void InitializeGarage()
        {
            var config = _fileManager.LoadConfiguration();
            _garage = new ParkingGarage(config.NumberOfSpots, config.SpotCapacity);

            var parkingData = _fileManager.LoadParkingData();

            // ⭐ Om ingen data finns, skapa testdata automatiskt
            if (parkingData == null)
            {
                AnsiConsole.MarkupLine("[yellow]Ingen data hittades. Skapar testdata...[/]");
                CreateTestData();
            }
            else
            {
                LoadVehiclesFromData(parkingData);
            }
        }

        static void CreateTestData()
        {
            // ⭐ Skapa testdata automatiskt
            var testVehicles = new List<IVehicle>
            {
                new Car("ABC123"),
                new Car("XYZ789"),
                new MC("MC001"),
                new MC("MC002"),
                new MC("MC003"),
                new Bicycle("CYC001"),
                new Bicycle("CYC002"),
                new Bicycle("CYC003"),
                new Bicycle("CYC004"),
                new Bus("BUS001"),
                new Car("CAR999")
            };

            foreach (var vehicle in testVehicles)
            {
                // Ställ in olika ankomsttider för att testa priser
                if (vehicle.VehicleType == "Car" && vehicle.RegistrationNumber == "ABC123")
                    vehicle.CheckInTime = DateTime.Now.AddHours(-3);
                else if (vehicle.VehicleType == "Bus")
                    vehicle.CheckInTime = DateTime.Now.AddHours(-1.5);
                else
                    vehicle.CheckInTime = DateTime.Now.AddMinutes(-30);

                _garage!.CheckInVehicle(vehicle);
            }

            SaveGarageData();
            AnsiConsole.MarkupLine("[green]✓ Testdata skapades framgångsrikt![/]");
            Thread.Sleep(2000);
        }

        static void LoadVehiclesFromData(ParkingData data)
        {
            foreach (var spotData in data.Spots)
            {
                if (spotData.SpotNumber > 0 && spotData.SpotNumber <= _garage!.ParkingSpots.Count)
                {
                    var spot = _garage.ParkingSpots[spotData.SpotNumber - 1];
                    foreach (var vehicleData in spotData.Vehicles)
                    {
                        IVehicle vehicle = vehicleData.Type switch
                        {
                            "Car" => new Car(vehicleData.RegistrationNumber),
                            "MC" => new MC(vehicleData.RegistrationNumber),
                            "Bus" => new Bus(vehicleData.RegistrationNumber),
                            "Bicycle" => new Bicycle(vehicleData.RegistrationNumber),
                            _ => new Car(vehicleData.RegistrationNumber)
                        };
                        vehicle.CheckInTime = vehicleData.CheckInTime;

                        // ⭐ KORRIGERING: Lägg till direkt utan verifiering
                        // (nödvändigt för bussar som upptar flera platser)
                        ((ParkingSpot)spot).Vehicles.Add(vehicle);
                    }
                }
            }
        }

        static void SaveGarageData()
        {
            var data = new ParkingData();
            foreach (var spot in _garage!.ParkingSpots)
            {
                if (!spot.IsEmpty())
                {
                    var spotData = new SpotData
                    {
                        SpotNumber = spot.SpotNumber,
                        Vehicles = spot.Vehicles.Select(v => new VehicleData
                        {
                            Type = v.VehicleType,
                            RegistrationNumber = v.RegistrationNumber,
                            CheckInTime = v.CheckInTime
                        }).ToList()
                    };
                    data.Spots.Add(spotData);
                }
            }
            _fileManager.SaveParkingData(data);
        }

        static void ShowWelcomeScreen()
        {
            AnsiConsole.MarkupLine(@"[yellow bold]
    ╔═══════════════════════════════════════════════╗
    ║                                               ║
    ║        VÄLKOMMEN TILL PRAGUE PARKING          ║
    ║                   SYSTEM                      ║
    ║                                               ║
    ╚═══════════════════════════════════════════════╝
[/]");
            AnsiConsole.MarkupLine("[dim]                Version 2.1[/]\n");
        }

        static string ShowMainMenu()
        {
            AnsiConsole.Clear();
            ShowWelcomeScreen();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Huvudmeny[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                "Incheckning",
                "Utcheckning",
                "Sök",
                "Visa karta",
                "Prislista",  // ⭐ ÄNDRAT från "Statistik" till "Prislista"
                "Ladda om konfiguration",
                "Avsluta"
                    }));
        }

        static void CheckInVehicle()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Incheckning av fordon ==[/]\n");

            var vehicleType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Fordonstyp?")
                    .AddChoices(new[] { "Car", "MC", "Bus", "Bicycle" }));

            var regNumber = AnsiConsole.Ask<string>("Registreringsnummer:");

            IVehicle vehicle = vehicleType switch
            {
                "Car" => new Car(regNumber),
                "MC" => new MC(regNumber),
                "Bus" => new Bus(regNumber),
                "Bicycle" => new Bicycle(regNumber),
                _ => new Car(regNumber)
            };

            if (_garage!.CheckInVehicle(vehicle))
            {
                SaveGarageData();
                var (spotNumbers, _) = _garage.FindVehicle(regNumber);

                if (spotNumbers.Count == 1)
                {
                    AnsiConsole.MarkupLine($"[green]✓ Fordon registrerat på plats {spotNumbers[0]}[/]");
                }
                else if (spotNumbers.Count > 1)
                {
                    string spots = string.Join(", ", spotNumbers);
                    AnsiConsole.MarkupLine($"[green]✓ Bus registrerat på platserna {spots}[/]");
                }
            }
            else
            {
                if (vehicle is Bus)
                {
                    AnsiConsole.MarkupLine("[red]✗ Inga 4 lediga platser i följd (platser 1-50) för bussen![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]✗ Ingen ledig plats![/]");
                }
            }
        }

        static void CheckOutVehicle()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Utcheckning av fordon ==[/]\n");

            var regNumber = AnsiConsole.Ask<string>("Registreringsnummer:");

            var (vehicle, spotNumbers, price) = _garage!.CheckOutVehicle(regNumber);

            if (vehicle != null)
            {
                SaveGarageData();

                var table = new Table();
                table.AddColumn("Detalj");
                table.AddColumn("Värde");
                table.AddRow("Fordon", vehicle.ToString());

                if (spotNumbers.Count == 1)
                {
                    table.AddRow("Plats", spotNumbers[0].ToString());
                }
                else
                {
                    table.AddRow("Platser", string.Join(", ", spotNumbers));
                }

                table.AddRow("Varaktighet", vehicle.GetParkedDuration().ToString(@"hh\:mm\:ss"));
                table.AddRow("Pris per timme", $"{vehicle.GetPricePerHour()} CZK");
                table.AddRow("Total pris", $"{price} CZK");

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine("\n[green]✓ Fordon utcheckat framgångsrikt![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]✗ Fordon hittades inte![/]");
            }
        }

        static void FindVehicle()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Sök fordon ==[/]\n");

            var regNumber = AnsiConsole.Ask<string>("Registreringsnummer:");

            var (spotNumbers, firstSpot) = _garage!.FindVehicle(regNumber);

            if (firstSpot != null && spotNumbers.Count > 0)
            {
                var vehicle = firstSpot.Vehicles.FirstOrDefault(v =>
                    v.RegistrationNumber.Equals(regNumber, StringComparison.OrdinalIgnoreCase));

                if (vehicle != null)
                {
                    var estimatedPrice = _garage.CalculatePrice(vehicle);

                    var table = new Table();
                    table.AddColumn("Detalj");
                    table.AddColumn("Värde");
                    table.AddRow("Fordon", vehicle.ToString());

                    if (spotNumbers.Count == 1)
                    {
                        table.AddRow("Plats", spotNumbers[0].ToString());
                    }
                    else
                    {
                        table.AddRow("Platser", string.Join(", ", spotNumbers));
                    }

                    table.AddRow("Ankomsttid", vehicle.CheckInTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    table.AddRow("Aktuell varaktighet", vehicle.GetParkedDuration().ToString(@"hh\:mm\:ss"));
                    table.AddRow("Pris per timme", $"{vehicle.GetPricePerHour()} CZK");
                    table.AddRow("Uppskattat pris", $"{estimatedPrice} CZK");

                    AnsiConsole.Write(table);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]✗ Fordon hittades inte![/]");
            }
        }

        static void ShowParkingMap()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Parkeringskarta ==[/]\n");

            // Skapa tabellen för kartan
            var table = new Table();
            table.AddColumn("Plats");
            table.AddColumn("Status");
            table.AddColumn("Fordon");

            foreach (var spot in _garage!.ParkingSpots)
            {
                string status;
                if (spot.IsEmpty())
                    status = "[green]LEDIG[/]";
                else if (spot.IsFull())
                    status = "[red]FULL[/]";
                else
                    status = "[yellow]DELVIS[/]";

                string vehicles = spot.Vehicles.Count > 0
                    ? string.Join(", ", spot.Vehicles.Select(v => $"{v.VehicleType}({v.RegistrationNumber})"))
                    : "-";

                // Markera platser med hög takhöjd
                string spotInfo = spot.HasHighCeiling ? $"{spot.SpotNumber} 🏢" : spot.SpotNumber.ToString();

                table.AddRow(spotInfo, status, vehicles);
            }

            // Allmän statistik
            int occupied = _garage.GetOccupiedSpots();
            int available = _garage.ParkingSpots.Count - occupied;

            string barChart = $@"[red]■■■■■[/] Upptagna: {occupied}
[green]■■■■■[/] Lediga: {available}";

            // ⭐ Informationspanel UTAN avsnittet "Parkerade fordon"
            var infoPanel = new Panel(
                new Markup($@"[bold yellow]=> Statistik[/]

{barChart}

Total platser: {_garage.ParkingSpots.Count}
Beläggning: {(occupied * 100.0 / _garage.ParkingSpots.Count):F1}%

[bold yellow]=> Regler:[/]

- 1 Bicycle = 1/4 plats (1 enhet)
- 1 MC = 1/2 plats (2 enheter)
- 1 Car = 1 plats (4 enheter)
- 1 Bus = 4 platser (16 enheter)

[bold yellow]=> Priser (CZK/timme):[/]

- Bicycle: 5 CZK
- MC: 10 CZK
- Car: 20 CZK
- Bus: 80 CZK

[bold yellow]=> Plafond:[/]

Platser 1-50 har högt tak för bussar
🏢 = Hög takhöjd

[bold yellow]=> Färgkod:[/]

[green]LEDIG[/]  = Ledig plats
[yellow]DELVIS[/] = Delvis upptagen
[red]FULL[/]   = Full plats"))
            {
                Header = new PanelHeader("[cyan]Information & Statistik[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };

            // Visa i två kolumner
            var layout = new Columns(table, infoPanel);
            AnsiConsole.Write(layout);
        }

        static void ShowStatistics()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Statistik ==[/]\n");

            var chart = new BarChart()
                .Width(60)
                .Label("[cyan bold]Parkeringsstatus[/]");

            int occupied = _garage!.GetOccupiedSpots();
            int available = _garage.ParkingSpots.Count - occupied;

            chart.AddItem("Upptagna", occupied, Color.Red);
            chart.AddItem("Lediga", available, Color.Green);

            AnsiConsole.Write(chart);

            // ⭐ Statistik per fordonstyp
            AnsiConsole.WriteLine("\n");
            var vehicleTable = new Table();
            vehicleTable.AddColumn("Fordonstyp");
            vehicleTable.AddColumn("Antal");
            vehicleTable.AddColumn("Pris/timme");

            var vehicleGroups = _garage.ParkingSpots
                .SelectMany(s => s.Vehicles)
                .GroupBy(v => v.VehicleType)
                .OrderBy(g => g.Key);

            foreach (var group in vehicleGroups)
            {
                var firstVehicle = group.First();
                vehicleTable.AddRow(group.Key, group.Count().ToString(), $"{firstVehicle.GetPricePerHour()} CZK");
            }

            AnsiConsole.Write(vehicleTable);
        }

        static void ShowPriceList()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Prislista ==[/]\n");

            // Huvudtabell för priser
            var priceTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Aqua);

            priceTable.AddColumn(new TableColumn("[bold yellow]Fordonstyp[/]").Centered());
            priceTable.AddColumn(new TableColumn("[bold yellow]Storlek (enheter)[/]").Centered());
            priceTable.AddColumn(new TableColumn("[bold yellow]Platser som behövs[/]").Centered());
            priceTable.AddColumn(new TableColumn("[bold yellow]Pris per timme[/]").Centered());
            priceTable.AddColumn(new TableColumn("[bold yellow]Pris per enhet[/]").Centered());

            // Lägg till data
            priceTable.AddRow("Bicycle", "1", "1/4 plats", "[green]5 CZK[/]", "5 CZK");
            priceTable.AddRow("MC", "2", "1/2 plats", "[blue]10 CZK[/]", "5 CZK");
            priceTable.AddRow("Car", "4", "1 plats", "[yellow]20 CZK[/]", "5 CZK");
            priceTable.AddRow("Bus", "16", "4 platser", "[red]80 CZK[/]", "5 CZK");

            AnsiConsole.Write(priceTable);

            // Ytterligare information
            var infoPanel = new Panel(
                new Markup(@"[bold yellow]Information om priser:[/]

- Priset baseras på fordonets storlek
- [green]5 CZK per enhet per timme[/]
- [bold]De första 10 minuterna är GRATIS[/]
- Priset räknas per påbörjad timme

[bold yellow]Exempel:[/]

- Parkera en bil i 2 timmar 30 min = [yellow]3 × 20 = 60 CZK[/]
- Parkera en MC i 45 min = [blue]1 × 10 = 10 CZK[/]
- Parkera en cykel i 5 min = [green]0 CZK (gratis!)[/]
- Parkera en bus i 1 timme 15 min = [red]2 × 80 = 160 CZK[/]

[bold yellow]Bussar:[/]

- Bussar kan endast parkera på platser 1-50
- Bussar behöver 4 konsekutiva tomma platser
- Högt tak krävs"))
            {
                Header = new PanelHeader("[cyan]Prisinfo[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };

            AnsiConsole.WriteLine();
            AnsiConsole.Write(infoPanel);
        }
        static void ReloadConfiguration()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[cyan]== Ladda om konfiguration ==[/]\n");

            var config = _fileManager.LoadConfiguration();

            AnsiConsole.MarkupLine($"[yellow]Nuvarande konfiguration:[/]");
            AnsiConsole.MarkupLine($"  Antal platser: {_garage!.ParkingSpots.Count}");
            AnsiConsole.MarkupLine($"  Platskapacitet: {config.SpotCapacity}");

            if (!AnsiConsole.Confirm("\nVill du ändra konfigurationen?"))
            {
                return;
            }

            // ⭐ Fråga efter det nya antalet platser
            int newSpots = AnsiConsole.Ask<int>("Nytt antal platser (nuvarande: " + _garage.ParkingSpots.Count + "):");

            // ⭐ VALIDERING: Kontrollera att vi inte tar bort upptagna platser
            if (newSpots < _garage.ParkingSpots.Count)
            {
                if (!_garage.CanReduceSpots(newSpots))
                {
                    AnsiConsole.MarkupLine("[red]✗ Kan inte minska antalet platser! Det finns fordon parkerade på platserna som skulle tas bort.[/]");
                    return;
                }
            }

            int newCapacity = AnsiConsole.Ask<int>("Ny platskapacitet (nuvarande: " + config.SpotCapacity + "):");

            // ⭐ Tillämpa ändringar
            _garage.ResizeGarage(newSpots, newCapacity);

            // ⭐ Spara den nya konfigurationen
            config.NumberOfSpots = newSpots;
            config.SpotCapacity = newCapacity;
            _fileManager.SaveConfiguration(config);

            SaveGarageData();

            AnsiConsole.MarkupLine("[green]✓ Konfigurationen uppdaterad![/]");
            AnsiConsole.MarkupLine($"  Nya antalet platser: {_garage.ParkingSpots.Count}");
            AnsiConsole.MarkupLine($"  Ny platskapacitet: {newCapacity}");
        }
    }
}