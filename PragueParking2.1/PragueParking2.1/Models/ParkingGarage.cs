using PragueParking2._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2._1.Models
{
    public class ParkingGarage
    {
        public List<IParkingSpot> ParkingSpots { get; private set; }
        private const int HIGH_CEILING_SPOTS = 50; //  De första 50 platserna har hög takhöjd

        public ParkingGarage(int numberOfSpots, int spotCapacity)
        {
            ParkingSpots = new List<IParkingSpot>();

            //  Skapa de första 50 platserna MED hög takhöjd (för bussar)
            for (int i = 1; i <= Math.Min(HIGH_CEILING_SPOTS, numberOfSpots); i++)
            {
                ParkingSpots.Add(new ParkingSpot(i, spotCapacity, hasHighCeiling: true));
            }

            // Skapa de återstående platserna UTAN hög takhöjd
            for (int i = HIGH_CEILING_SPOTS + 1; i <= numberOfSpots; i++)
            {
                ParkingSpots.Add(new ParkingSpot(i, spotCapacity, hasHighCeiling: false));
            }
        }

        public bool CheckInVehicle(IVehicle vehicle)
        {
            // SPECIALFALL: BUSS kräver 4 konsekutiva platser med hög takhöjd
            if (vehicle is Bus)
            {
                return CheckInBus(vehicle);
            }

            // NORMALFALL: Car, MC, Bicycle
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

        private bool CheckInBus(IVehicle bus)
        {
            // Sök efter 4 konsekutiva LEDIGA platser med hög takhöjd (platser 1-50)
            for (int i = 0; i <= Math.Min(HIGH_CEILING_SPOTS - 4, ParkingSpots.Count - 4); i++)
            {
                bool fourConsecutiveEmpty = true;

                // Kontrollera om 4 konsekutiva platser är lediga OCH har hög takhöjd
                for (int j = 0; j < 4; j++)
                {
                    var spot = ParkingSpots[i + j];
                    if (!spot.IsEmpty() || !spot.HasHighCeiling)
                    {
                        fourConsecutiveEmpty = false;
                        break;
                    }
                }

                if (fourConsecutiveEmpty)
                {
                    
                    // Eftersom bussen (16 enheter) inte kan få plats i en enda plats (4 enheter)
                    for (int j = 0; j < 4; j++)
                    {
                        var spot = (ParkingSpot)ParkingSpots[i + j];
                        spot.Vehicles.Add(bus); // ⭐ DIREKT tillägg till listan
                    }
                    return true;
                }
            }
            return false;
        }

        public (IVehicle? vehicle, List<int> spotNumbers, decimal price) CheckOutVehicle(string registrationNumber)
        {
            List<int> spotNumbers = new List<int>();
            IVehicle? foundVehicle = null;

            // Sök efter fordonet på alla platser
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
                // Ta bort fordonet från alla platser där det finns
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

        public (List<int> spotNumbers, IParkingSpot? firstSpot) FindVehicle(string registrationNumber)
        {
            List<int> spotNumbers = new List<int>();
            IParkingSpot? firstSpot = null;

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

        public decimal CalculatePrice(IVehicle vehicle)
        {
            TimeSpan duration = vehicle.GetParkedDuration();

            //  De första 10 minuterna är gratis
            if (duration.TotalMinutes <= 10)
                return 0;

            int hoursStarted = (int)Math.Ceiling(duration.TotalHours);
            decimal pricePerHour = vehicle.GetPricePerHour();
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

        public bool CanReduceSpots(int newNumberOfSpots)
        {
            //  Kontrollera att inget fordon är parkerat bortom den nya gränsen
            for (int i = newNumberOfSpots; i < ParkingSpots.Count; i++)
            {
                if (!ParkingSpots[i].IsEmpty())
                {
                    return false;
                }
            }
            return true;
        }

        public void ResizeGarage(int newNumberOfSpots, int spotCapacity)
        {
            if (newNumberOfSpots < ParkingSpots.Count)
            {
                //  Minska antalet platser
                if (CanReduceSpots(newNumberOfSpots))
                {
                    ParkingSpots.RemoveRange(newNumberOfSpots, ParkingSpots.Count - newNumberOfSpots);
                }
            }
            else if (newNumberOfSpots > ParkingSpots.Count)
            {
                //  Lägg till platser
                int currentCount = ParkingSpots.Count;
                for (int i = currentCount + 1; i <= newNumberOfSpots; i++)
                {
                    bool hasHighCeiling = i <= HIGH_CEILING_SPOTS;
                    ParkingSpots.Add(new ParkingSpot(i, spotCapacity, hasHighCeiling));
                }
            }
        }
    }
}