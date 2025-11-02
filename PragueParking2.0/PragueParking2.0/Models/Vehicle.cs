using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._0.Models
{
    public abstract class Vehicle
    {
        public string RegistrationNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public abstract string VehicleType { get; }
        public abstract int SpaceRequired { get; }
        public abstract bool RequiresConsecutiveSpots { get; }

        protected Vehicle(string registrationNumber)
        {
            RegistrationNumber = registrationNumber;
            CheckInTime = DateTime.Now;
        }

        public TimeSpan GetParkedDuration()
        {
            return DateTime.Now - CheckInTime;
        }

        public override string ToString()
        {
            return $"{VehicleType}: {RegistrationNumber}";
        }
    }

    public class Car : Vehicle
    {
        public override string VehicleType => "Car";
        public override int SpaceRequired => 4;
        public override bool RequiresConsecutiveSpots => false;

        public Car(string registrationNumber) : base(registrationNumber)
        {
        }
    }

    public class MC : Vehicle
    {
        public override string VehicleType => "MC";
        public override int SpaceRequired => 2;
        public override bool RequiresConsecutiveSpots => false;

        public MC(string registrationNumber) : base(registrationNumber)
        {
        }
    }

    public class Bus : Vehicle
    {
        public override string VehicleType => "Bus";
        public override int SpaceRequired => 4;
        public override bool RequiresConsecutiveSpots => true;

        public Bus(string registrationNumber) : base(registrationNumber)
        {
        }
    }

    public class Bicycle : Vehicle
    {
        public override string VehicleType => "Bicycle";
        public override int SpaceRequired => 1;
        public override bool RequiresConsecutiveSpots => false;

        public Bicycle(string registrationNumber) : base(registrationNumber)
        {
        }
    }
}