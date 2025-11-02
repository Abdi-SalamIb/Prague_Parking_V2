using PragueParking2._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._1.Models
{
    public class ParkingSpot : IParkingSpot
    {
        public int SpotNumber { get; private set; }
        public int Capacity { get; private set; }
        public bool HasHighCeiling { get; private set; } 
        public List<IVehicle> Vehicles { get; private set; }

        public ParkingSpot(int spotNumber, int capacity, bool hasHighCeiling = false)
        {
            SpotNumber = spotNumber;
            Capacity = capacity;
            HasHighCeiling = hasHighCeiling;
            Vehicles = new List<IVehicle>();
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

        public bool CanFitVehicle(IVehicle vehicle)
        {
            // Kontrollera om fordonet kräver hög takhöjd
            if (vehicle.RequiresHighCeiling && !HasHighCeiling)
            {
                return false;
            }

            return GetAvailableSpace() >= vehicle.SpaceRequired;
        }

        public bool AddVehicle(IVehicle vehicle)
        {
            if (CanFitVehicle(vehicle))
            {
                Vehicles.Add(vehicle);
                return true;
            }
            return false;
        }

        public IVehicle? RemoveVehicle(string registrationNumber)
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