using PragueParking2._1.Interfaces;
using PragueParking2._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using PragueParking.DataAccess;
namespace PragueParking.Tests
{
    [TestClass]
    public class ParkingTests
    {

        [TestMethod]
        public void Bus_ShouldTakeFourConsecutiveSpots()
        {
            // Arrange
            var garage = new ParkingGarage(100, 4);
            var bus = new Bus("BUS001");
            // Act
            bool result = garage.CheckInVehicle(bus);
            // Assert
            Assert.IsTrue(result, "Bussen borde registreras");
            // Kontrollera att bussen upptar exakt 4 platser
            var (spotNumbers, _) = garage.FindVehicle("BUS001");
            Assert.AreEqual(4, spotNumbers.Count, "Bussen borde uppta 4 konsekutiva platser");
        }
        // ✅ TEST 2: Bussen kan bara parkera på plats 1-50 (hög takhöjd)
        [TestMethod]
        public void Bus_ShouldOnlyParkInFirst50SpotsWithHighCeiling()
        {
            // Arrange
            var garage = new ParkingGarage(100, 4);
            // Act & Assert - Kontrollera att endast de första 50 platserna har hög takhöjd
            for (int i = 0; i < 50; i++)
            {
                Assert.IsTrue(garage.ParkingSpots[i].HasHighCeiling,
                    $"Plats {i + 1} borde ha hög takhöjd");
            }
            for (int i = 50; i < 100; i++)
            {
                Assert.IsFalse(garage.ParkingSpots[i].HasHighCeiling,
                    $"Plats {i + 1} borde INTE ha hög takhöjd");
            }
        }
        // ✅ TEST 3: Priserna ska baseras på storlek (5 CZK per enhet)
        [TestMethod]
        public void VehiclePrices_ShouldBeBasedOnSize()
        {
            // Arrange
            var bicycle = new Bicycle("B1");
            var mc = new MC("M1");
            var car = new Car("C1");
            var bus = new Bus("BUS1");
            // Act & Assert - Kontrollera att priset = storlek × 5 CZK
            Assert.AreEqual(5m, bicycle.GetPricePerHour(), "Bicycle: 1 enhet × 5 = 5 CZK");
            Assert.AreEqual(10m, mc.GetPricePerHour(), "MC: 2 enheter × 5 = 10 CZK");
            Assert.AreEqual(20m, car.GetPricePerHour(), "Car: 4 enheter × 5 = 20 CZK");
            Assert.AreEqual(80m, bus.GetPricePerHour(), "Bus: 16 enheter × 5 = 80 CZK");
        }
    }
}