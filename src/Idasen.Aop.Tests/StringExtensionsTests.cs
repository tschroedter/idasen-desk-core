using FluentAssertions ;

namespace Idasen.Aop.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void MaskMacAddress_ForShortString_ReturnsOriginal()
        {
            // Arrange
            var input = "1234";

            // Act
            var result = input.MaskMacAddress();

            // Assert
            result.Should().Be("1234");
        }

        [TestMethod]
        public void MaskMacAddress_ForValidMacAddress_MasksCorrectly()
        {
            // Arrange
            var input = "AA:BB:CC:DD:EE:FF";

            // Act
            var result = input.MaskMacAddress();

            // Assert
            result.Should().Be("***-EE:FF");
        }
    }
}
