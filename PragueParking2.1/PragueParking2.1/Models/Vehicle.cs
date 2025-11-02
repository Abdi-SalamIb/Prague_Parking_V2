using PragueParking2._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._1.Models
{
    public abstract class Vehicle : IVehicle
    {
        public string RegistrationNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public abstract string VehicleType { get; }
        public abstract int SpaceRequired { get; }
        public abstract bool RequiresHighCeiling { get; }

        protected Vehicle(string registrationNumber)
        {
            RegistrationNumber = registrationNumber;
            CheckInTime = DateTime.Now;
        }

        public TimeSpan GetParkedDuration()
        {
            return DateTime.Now - CheckInTime;
        }

        public abstract decimal GetPricePerHour();

        public override string ToString()
        {
            return $"{VehicleType}: {RegistrationNumber}";
        }
    }

    public class Car : Vehicle
    {
        public override string VehicleType => "Car";
        public override int SpaceRequired => 4;
        public override bool RequiresHighCeiling => false;

        public Car(string registrationNumber) : base(registrationNumber)
        {
        }

        public override decimal GetPricePerHour()
        {
            return 20m; // CZK par heure
        }
    }

    public class MC : Vehicle
    {
        public override string VehicleType => "MC";
        public override int SpaceRequired => 2;
        public override bool RequiresHighCeiling => false;

        public MC(string registrationNumber) : base(registrationNumber)
        {
        }

        public override decimal GetPricePerHour()
        {
            return 10m; // CZK per timme
        }
    }

    public class Bus : Vehicle
    {
        public override string VehicleType => "Bus";
        public override int SpaceRequired => 16;
        public override bool RequiresHighCeiling => true; //  Buss kräver hög takhöjd

        public Bus(string registrationNumber) : base(registrationNumber)
        {
        }

        public override decimal GetPricePerHour()
        {
            return 80m; // CZK per timme (80 för 16 enheter = 5 CZK/enhet)

        }
    }

    public class Bicycle : Vehicle
    {
        public override string VehicleType => "Bicycle";
        public override int SpaceRequired => 1;
        public override bool RequiresHighCeiling => false;

        public Bicycle(string registrationNumber) : base(registrationNumber)
        {
        }

        public override decimal GetPricePerHour()
        {
            return 5m; 
        }
    }
}