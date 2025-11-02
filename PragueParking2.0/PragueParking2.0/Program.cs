using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PragueParking.DataAccess;
using System.Threading.Tasks;
using Spectre.Console;
using PragueParking2._0.Models;




namespace PragueParking2._0
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
                    case "Ladda om konfiguration":
                        InitializeGarage();
                        AnsiConsole.MarkupLine("[green]Konfigurationen omladdad![/]");
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
            AnsiConsole.MarkupLine("[yellow]Tack för att du använde Prague Parking! Hej då![/]");
        }

        static void InitializeGarage()
        {
            var config = _fileManager.LoadConfiguration();
            _garage = new ParkingGarage(config.NumberOfSpots, config.SpotCapacity);

            foreach (var price in config.Prices)
            {
                _garage.PriceList.UpdatePrice(price.Key, price.Value);
            }

            var parkingData = _fileManager.LoadParkingData();
            if (parkingData != null)
            {
                LoadVehiclesFromData(parkingData);
            }
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
                        Vehicle vehicle = vehicleData.Type switch
                        {
                            "Car" => new Car(vehicleData.RegistrationNumber),
                            "MC" => new MC(vehicleData.RegistrationNumber),
                            "Bus" => new Bus(vehicleData.RegistrationNumber),
                            "Bicycle" => new Bicycle(vehicleData.RegistrationNumber),
                            _ => new Car(vehicleData.RegistrationNumber)
                        };
                        vehicle.CheckInTime = vehicleData.CheckInTime;
                        spot.AddVehicle(vehicle);
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
            AnsiConsole.MarkupLine("[dim]                Version 2.0[/]\n");
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

            Vehicle vehicle = vehicleType switch
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
                    AnsiConsole.MarkupLine("[red]✗ Inga 4 lediga platser i följd för bussen![/]");
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
                table.AddRow("Pris", $"{price} CZK");

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

                table.AddRow(
                    spot.SpotNumber.ToString(),
                    status,
                    vehicles
                );
            }

           
            var infoPanel = new Panel(
                new Markup($@"
[bold yellow]=> Regler:[/]

- 1 Car = 1 plats 
- 2 MC = 1 plats 
- 4 Bicycle = 1 plats 
- 1 Bus = 4 platser konsekutiva 

[bold yellow]=> Färgkod:[/]

[green]LEDIG[/]  = Ledig plats
[yellow]DELVIS[/] = Delvis upptagen
[red]FULL[/]   = Full plats"))
            {
                Header = new PanelHeader("[cyan]Information[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };

            
            
            var chart = new BarChart()
                .Width(40)
                .Label("[cyan bold]Parkeringsstatus[/]");

            int occupied = _garage!.GetOccupiedSpots();
            int available = _garage.ParkingSpots.Count - occupied;

            chart.AddItem("Upptagna", occupied, Color.Red);
            chart.AddItem("Lediga", available, Color.Green);

            
            var prices = _garage.PriceList.GetAllPrices();
            var priceTable = new Table();
            priceTable.AddColumn("Fordonstyp");
            priceTable.AddColumn("Pris (CZK/h)");

            foreach (var price in prices)
            {
                priceTable.AddRow(price.Key, price.Value.ToString());
            }

            
            var statsContent = new Rows(chart, new Text(""), priceTable);

            var statsPanel = new Panel(statsContent)
            {
                Header = new PanelHeader("[cyan]Statistiques & Prix[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };

            
            var rightColumn = new Rows(infoPanel, statsPanel);

            
            var layout = new Columns(table, rightColumn);
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

            AnsiConsole.WriteLine();
            var prices = _garage.PriceList.GetAllPrices();
            var priceTable = new Table();
            priceTable.AddColumn("Fordonstyp");
            priceTable.AddColumn("Pris (CZK/timme)");

            foreach (var price in prices)
            {
                priceTable.AddRow(price.Key, price.Value.ToString());
            }

            AnsiConsole.Write(priceTable);
        }
    }
}
