using VirtoCommerce.InventoryModule.Data.Extensions;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    public class GeoExtensionsTests
    {
        [Fact]
        public void CalculateDistance_GetPointIsNull_ShouldBeNull()
        {
            //Arrange
            var ff1GeoLocation = default(string);
            var ff21GeoLocation = default(string);

            // Act
            var distance = ff1GeoLocation.CalculateDistance(ff21GeoLocation);

            //Assert
            Assert.Null(distance);
        }

        [Fact]
        public void CalculateDistance_GetPointIsEmpty_ShouldBeNull()
        {
            //Arrange
            var ff1GeoLocation = string.Empty;
            var ff21GeoLocation = string.Empty;

            // Act
            var distance = ff1GeoLocation.CalculateDistance(ff21GeoLocation);

            //Assert
            Assert.Null(distance);
        }
    }
}
