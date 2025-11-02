using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._0.Models
{
    public class ParkingSpot
    {
        public int SpotNumber { get; set; }
        public int Capacity { get; set; }
        public List<Vehicle> Vehicles { get; set; }

        public ParkingSpot(int spotNumber, int capacity)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
            Vehicles = new List<Vehicle>();
        }

        public bool IsEmpty()
        {
            return Vehicles.Count == 0;
        }

        public bool IsFull()
        {
            int usedSpace = Vehicles.Sum(v => v.SpaceRequired);
            return usedSpace >= Capacity;
        }

        public int GetAvailableSpace()
        {
            int usedSpace = Vehicles.Sum(v => v.SpaceRequired);
            return Capacity - usedSpace;
        }

        public bool CanFitVehicle(Vehicle vehicle)
        {
            return GetAvailableSpace() >= vehicle.SpaceRequired;
        }

        public bool AddVehicle(Vehicle vehicle)
        {
            if (CanFitVehicle(vehicle))
            {
                Vehicles.Add(vehicle);
                return true;
            }
            return false;
        }

        public Vehicle? RemoveVehicle(string registrationNumber)
        {
            var vehicle = Vehicles.FirstOrDefault(v =>
                v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase));

            if (vehicle != null)
            {
                Vehicles.Remove(vehicle);
                return vehicle;
            }
            return null;
        }

        public string GetDisplayStatus()
        {
            if (IsEmpty())
                return "[ ]";

            if (IsFull())
                return "[X]";

            return "[~]";
        }
    }
}
