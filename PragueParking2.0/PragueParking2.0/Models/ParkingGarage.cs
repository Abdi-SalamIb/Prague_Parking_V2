using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._0.Models
{
    public class PriceList
    {
        private Dictionary<string, decimal> _prices;
        public PriceList()
        {
            _prices = new Dictionary<string, decimal>
            {
                { "Car", 20m },
                { "MC", 10m },
                { "Bus", 40m },
                { "Bicycle", 5m }
            };
        }
        public decimal GetPrice(string vehicleType)
        {
            return _prices.TryGetValue(vehicleType, out decimal price) ? price : 0m;
        }
        public void UpdatePrice(string vehicleType, decimal price)
        {
            _prices[vehicleType] = price;
        }
        public Dictionary<string, decimal> GetAllPrices()
        {
            return new Dictionary<string, decimal>(_prices);
        }
    }

    public class ParkingGarage
    {
        public List<ParkingSpot> ParkingSpots { get; set; }
        public PriceList PriceList { get; set; }

        public ParkingGarage(int numberOfSpots, int spotCapacity)
        {
            ParkingSpots = new List<ParkingSpot>();

            
            for (int i = 1; i <= numberOfSpots; i++)
            {
                ParkingSpots.Add(new ParkingSpot(i, spotCapacity));
            }

            PriceList = new PriceList();
        }

        public bool CheckInVehicle(Vehicle vehicle)
        {
            
            if (vehicle.RequiresConsecutiveSpots && vehicle is Bus)
            {
                return CheckInBus(vehicle);
            }

           
            foreach (var spot in ParkingSpots)
            {
                if (spot.CanFitVehicle(vehicle))
                {
                    spot.AddVehicle(vehicle);
                    return true;
                }
            }
            return false;
        }

        private bool CheckInBus(Vehicle bus)
        {
            
            for (int i = 0; i <= ParkingSpots.Count - 4; i++)
            {
                bool fourConsecutiveEmpty = true;

                
                for (int j = 0; j < 4; j++)
                {
                    if (!ParkingSpots[i + j].IsEmpty())
                    {
                        fourConsecutiveEmpty = false;
                        break;
                    }
                }

                if (fourConsecutiveEmpty)
                {
                   
                    for (int j = 0; j < 4; j++)
                    {
                        ParkingSpots[i + j].AddVehicle(bus);
                    }
                    return true;
                }
            }
            return false;
        }

        public (Vehicle? vehicle, List<int> spotNumbers, decimal price) CheckOutVehicle(string registrationNumber)
        {
            List<int> spotNumbers = new List<int>();
            Vehicle? foundVehicle = null;

            
            foreach (var spot in ParkingSpots)
            {
                var vehicle = spot.Vehicles.FirstOrDefault(v =>
                    v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase));

                if (vehicle != null)
                {
                    foundVehicle = vehicle;
                    spotNumbers.Add(spot.SpotNumber);
                }
            }

            if (foundVehicle != null)
            {
                
                foreach (var spotNumber in spotNumbers)
                {
                    var spot = ParkingSpots[spotNumber - 1];
                    spot.RemoveVehicle(registrationNumber);
                }

                decimal price = CalculatePrice(foundVehicle);
                return (foundVehicle, spotNumbers, price);
            }

            return (null, new List<int>(), 0);
        }

        public (List<int> spotNumbers, ParkingSpot? firstSpot) FindVehicle(string registrationNumber)
        {
            List<int> spotNumbers = new List<int>();
            ParkingSpot? firstSpot = null;

            foreach (var spot in ParkingSpots)
            {
                if (spot.Vehicles.Any(v => v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    spotNumbers.Add(spot.SpotNumber);
                    if (firstSpot == null)
                        firstSpot = spot;
                }
            }

            return (spotNumbers, firstSpot);
        }

        public decimal CalculatePrice(Vehicle vehicle)
        {
            TimeSpan duration = vehicle.GetParkedDuration();
            if (duration.TotalMinutes <= 10)
                return 0;
            int hoursStarted = (int)Math.Ceiling(duration.TotalHours);
            decimal pricePerHour = PriceList.GetPrice(vehicle.VehicleType);
            return hoursStarted * pricePerHour;
        }

        public int GetOccupiedSpots()
        {
            return ParkingSpots.Count(s => !s.IsEmpty());
        }

        public int GetAvailableSpots()
        {
            return ParkingSpots.Count(s => !s.IsFull());
        }
    }
}
