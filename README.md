
# Prague Parking 2.0 & 2.1

Intelligent parkeringshanteringssystem i **C#**, med **OOP**, gränssnitt och **JSON**-persistens.

---

## Översikt

**Prague Parking** är ett komplett parkeringshanteringssystem utvecklat i C# med objektorienterad programmering. Systemet hanterar 4 typer av fordon (bilar, motorcyklar, bussar, cyklar) med ett modernt användargränssnitt baserat på **Spectre.Console** och datapersistering i **JSON**.

### Tillgängliga versioner

- **Version 2.0**: Grundläggande implementation med OOP (arv, inkapsling)
- **Version 2.1**: Avancerad version med gränssnitt, fysiska begränsningar och validering

---

##  Funktioner

### Version 2.0

**Fordonshantering**
- Registrering (check-in) av fordon med tidsstämpel
- Utcheckning (check-out) med automatisk prisberäkning
- Sök fordon via registreringsnummer
- Stöd för 4 typer: Bil, MC, Buss, Cykel

**Intelligent parkeringssystem**
- 100 konfigurerbara platser
- Enhetssystem (1 plats = 4 enheter)
- 1 bil = 1 full plats (4 enheter)
- 2 MC = 1 plats (2 enheter vardera)
- 4 cyklar = 1 plats (1 enhet vardera)
- 1 buss = 4 sammanhängande platser (16 enheter)

**Prissättning**
- De första 10 minuterna är gratis
- Timpris räknas från påbörjad timme
- Pris: Bil (20 CZK/h), MC (10 CZK/h), Buss (80 CZK/h), Cykel (5 CZK/h)

**Datapersistering**
- Automatisk sparning i JSON efter varje operation
- Automatisk laddning vid start
- Konfiguration kan ändras via fil

**Användargränssnitt**
- Interaktiv meny med Spectre.Console
- Realtidsparkeringstavla
- Formaterade tabeller med färgkoder
- Detaljerad statistik

### Version 2.1 (Extra funktioner)

**Gränssnitt**
- `IVehicle`: Kontrakt för alla fordon
- `IParkingSpot`: Kontrakt för parkeringsplatser
- Programmering mot abstraktioner

**Realistiska fysiska begränsningar**
- Högt tak: endast platser 1-50 accepterar bussar
- Visuell indikator för platser med högt tak
- Strikt validering vid parkering

**Pris i fordon**
- Metod `GetPricePerHour()` i varje klass
- Logisk relation: 5 CZK per enhetsstorlek

**Automatisk generering av testdata**
- Skapar fordon automatiskt vid första start
- Underlättar tester och demonstration

**Avancerad konfigurationsvalidering**
- Förhindrar borttagning av upptagna platser
- Tydliga felmeddelanden
- Kontroll av datakonsistens

---

## Arkitektur

### Projektstruktur
```
PragueParking-Solution/
├── PragueParking2.0/                    # Huvudapplikation v2.0
│   ├── Models/
│   │   ├── Vehicle.cs                   # Abstrakt klass + ärvda klasser
│   │   ├── ParkingSpot.cs               # Parkeringsplatshantering
│   │   └── ParkingGarage.cs             # Garagelogik
│   ├── Program.cs                       # Ingångspunkt
│   ├── config.json                      # Konfiguration
│   └── PragueParking2.0.csproj
│
├── PragueParking.DataAccess/            # Delat klassbibliotek
│   ├── FileManager.cs                   # JSON-filhantering
│   │   └── PragueParking.DataAccess.csproj
│
└── PragueParking.Tests/                 # Enhetstester
    ├── ParkingTests.cs                  # MSTest-tester
    └── PragueParking.Tests.csproj

PragueParking2.1-Solution/
├── PragueParking2.1/                    # Huvudapplikation v2.1
│   ├── Interfaces/
│   │   ├── IVehicle.cs                  # Fordonsgränssnitt
│   │   └── IParkingSpot.cs              # Parkeringsplatsgränssnitt
│   ├── Models/
│   │   ├── Vehicle.cs                   # Implementerar IVehicle
│   │   ├── ParkingSpot.cs               # Implementerar IParkingSpot
│   │   └── ParkingGarage.cs             # Avancerad logik
│   ├── Program.cs
│   ├── config.json
│   └── PragueParking2.1.csproj
│
└── PragueParking.DataAccess/            # Delat bibliotek
    └── FileManager.cs
```

### Installationssteg

#### Alternativ 1: Klona repo (om GitHub)
```bash
git clone https://github.com/ditt-användarnamn/prague-parking.git
cd prague-parking
```

#### Alternativ 2: Ladda ner ZIP

1. Ladda ner projektets ZIP-fil
2. Extrahera till valfri mapp
