using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Text.Json;
using System.Text.Json.Serialization;

namespace PragueParking.DataAccess
{
    public class FileManager
    {
        private readonly string _dataFilePath;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileManager(string dataFilePath = "parking_data.json",
                          string configFilePath = "config.json")
        {
            _dataFilePath = dataFilePath;
            _configFilePath = configFilePath;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new VehicleConverter() }
            };
        }

        public ParkingData? LoadParkingData()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return null;

                string json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<ParkingData>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid laddning av data: {ex.Message}");
                return null;
            }
        }

        public bool SaveParkingData(ParkingData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(_dataFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid säkerhetskopiering: {ex.Message}");
                return false;
            }
        }

        public Configuration LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    var defaultConfig = new Configuration
                    {
                        NumberOfSpots = 100,
                        SpotCapacity = 4,
                        Prices = new Dictionary<string, decimal>
                        {
                            { "Car", 20m },
                            { "MC", 10m },
                            { "Bus", 40m },
                            { "Bicycle", 5m }
                        }
                    };
                    SaveConfiguration(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<Configuration>(json, _jsonOptions)
                       ?? new Configuration();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid laddning av konfigurationen: {ex.Message}");
                return new Configuration();
            }
        }

        public bool SaveConfiguration(Configuration config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid säkerhetskopiering av konfigurationen: {ex.Message}");
                return false;
            }
        }
    }

    public class ParkingData
    {
        public List<SpotData> Spots { get; set; } = new List<SpotData>();
    }

    public class SpotData
    {
        public int SpotNumber { get; set; }
        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();
    }

    public class VehicleData
    {
        public string Type { get; set; } = "";
        public string RegistrationNumber { get; set; } = "";
        public DateTime CheckInTime { get; set; }
    }

    public class Configuration
    {
        public int NumberOfSpots { get; set; } = 100;
        public int SpotCapacity { get; set; } = 4;
        public Dictionary<string, decimal> Prices { get; set; } = new Dictionary<string, decimal>();
    }

    public class VehicleConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                return doc.RootElement.Clone();
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
