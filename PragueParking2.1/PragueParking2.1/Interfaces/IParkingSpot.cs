using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._1.Interfaces
{
    public interface IParkingSpot
    {
        int SpotNumber { get; }
        int Capacity { get; }
        bool HasHighCeiling { get; }
        List<IVehicle> Vehicles { get; }

        bool IsEmpty();
        bool IsFull();
        int GetAvailableSpace();
        bool CanFitVehicle(IVehicle vehicle);
        bool AddVehicle(IVehicle vehicle);
        IVehicle? RemoveVehicle(string registrationNumber);
    }
}
