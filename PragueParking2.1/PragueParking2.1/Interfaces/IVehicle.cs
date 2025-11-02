using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._1.Interfaces
{
    public interface IVehicle
    {
        string RegistrationNumber { get; set; }
        DateTime CheckInTime { get; set; }
        string VehicleType { get; }
        int SpaceRequired { get; }
        bool RequiresHighCeiling { get; }

        TimeSpan GetParkedDuration();
        decimal GetPricePerHour();
    }
}
