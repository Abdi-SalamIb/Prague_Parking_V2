using PragueParking.DataAccess;
using PragueParking2._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Tests
{
    [TestClass]
    public class ParkingTests
    {
        [TestMethod]
        public void Car_ShouldTakeOneFullSpot()
        {
            // Arrange
            var garage = new ParkingGarage(10, 4);
            var car = new Car("ABC123");

            // Act
            bool result = garage.CheckInVehicle(car);
            var spot = garage.ParkingSpots[0];

            // Assert
            Assert.IsTrue(result, "Bilen borde vara registrerad");
            Assert.IsTrue(spot.IsFull(), "Platsen borde vara helt upptagen");
            Assert.AreEqual(1, spot.Vehicles.Count, "Det borde finnas 1 fordon");
            Assert.AreEqual(0, spot.GetAvailableSpace(), "Det borde inte finnas något ledigt utrymme");
        }

        [TestMethod]
        public void TwoMotorcycles_ShouldShareOneSpot()
        {
            // Arrange
            var garage = new ParkingGarage(10, 4);
            var mc1 = new MC("MC001");
            var mc2 = new MC("MC002");

            // Act
            bool result1 = garage.CheckInVehicle(mc1);
            bool result2 = garage.CheckInVehicle(mc2);
            var spot = garage.ParkingSpots[0];

            // Assert
            Assert.IsTrue(result1, "Den första motorcykeln borde vara registrerad");
            Assert.IsTrue(result2, "Den andra motorcykeln borde vara registrerad");
            Assert.AreEqual(2, spot.Vehicles.Count, "Det borde finnas 2 motorcyklar på samma plats");
            Assert.IsTrue(spot.IsFull(), "Platsen borde vara helt upptagen");
        }

        [TestMethod]
        public void Bus_ShouldTakeFourConsecutiveSpots()
        {
            // Arrange
            var garage = new ParkingGarage(10, 4);
            var bus = new Bus("BUS123");

            // Act
            bool result = garage.CheckInVehicle(bus);

            // Assert
            Assert.IsTrue(result, "Bussen borde vara registrerad");

            // Kontrollera att bussen upptar de första fyra platserna
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(garage.ParkingSpots[i].IsFull(), $"Plats {i + 1} borde vara upptagen av bussen");
                Assert.AreEqual(1, garage.ParkingSpots[i].Vehicles.Count, $"Plats {i + 1} borde innehålla bussen");
                Assert.AreEqual("BUS123", garage.ParkingSpots[i].Vehicles[0].RegistrationNumber);
            }

            // Kontrollera att den femte platsen är tom
            Assert.IsTrue(garage.ParkingSpots[4].IsEmpty(), "Den femte platsen borde vara tom");
        }
    }
}
