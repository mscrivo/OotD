using System.Drawing;

namespace OotD.Core.Tests.Utility;

public class UnsafeNativeMethodsTests
{
    [Fact]
    public void WindowMessageConstants_ShouldExistAsNestedClasses()
    {
        // Test that the nested classes exist
        var wmType = typeof(OotD.Utility.UnsafeNativeMethods).GetNestedType("WM",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        var vkType = typeof(OotD.Utility.UnsafeNativeMethods).GetNestedType("VK",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        var htType = typeof(OotD.Utility.UnsafeNativeMethods).GetNestedType("HT",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        var bitType = typeof(OotD.Utility.UnsafeNativeMethods).GetNestedType("Bit",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        // Assert
        wmType.Should().NotBeNull("WM nested class should exist");
        vkType.Should().NotBeNull("VK nested class should exist");
        htType.Should().NotBeNull("HT nested class should exist");
        bitType.Should().NotBeNull("Bit nested class should exist");
    }

    [Theory]
    [InlineData(0x12345678, 0x1234)]
    [InlineData(0x7BCDEF00, 0x7BCD)] // Changed from 0xABCDEF00 to avoid uint conversion
    [InlineData(0x00001234, 0x0000)]
    [InlineData(0x7FFF0000, 0x7FFF)] // Changed from 0xFFFF0000 to avoid uint conversion
    public void Bit_HiWord_ShouldReturnHighOrder16Bits(int input, int expected)
    {
        // Act
        var result = OotD.Utility.UnsafeNativeMethods.Bit.HiWord(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0x12345678, 0x5678)]
    [InlineData(0x7BCDEF00, 0xEF00)] // Changed from 0xABCDEF00 to avoid uint conversion
    [InlineData(0x00001234, 0x1234)]
    [InlineData(0x7FFF0000, 0x0000)] // Changed from 0xFFFF0000 to avoid uint conversion
    public void Bit_LoWord_ShouldReturnLowOrder16Bits(int input, int expected)
    {
        // Act
        var result = OotD.Utility.UnsafeNativeMethods.Bit.LoWord(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Bit_HiWordAndLoWord_ShouldBeComplementary()
    {
        // Arrange
        const int testValue = 0x12345678;

        // Act
        var hiWord = OotD.Utility.UnsafeNativeMethods.Bit.HiWord(testValue);
        var loWord = OotD.Utility.UnsafeNativeMethods.Bit.LoWord(testValue);

        // Assert
        hiWord.Should().Be(0x1234);
        loWord.Should().Be(0x5678);

        // Verify the words can reconstruct the original (approximately)
        var reconstructed = (hiWord << 16) | (loWord & 0xFFFF);
        reconstructed.Should().Be(testValue);
    }

    [Fact]
    public void WINDOWPOS_Structure_ShouldExistAndBeStruct()
    {
        // Test that the WINDOWPOS structure exists
        var windowPosType = typeof(OotD.Utility.UnsafeNativeMethods).GetNestedType("WINDOWPOS",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        // Assert
        windowPosType.Should().NotBeNull("WINDOWPOS structure should exist");
        windowPosType!.IsValueType.Should().BeTrue("WINDOWPOS should be a struct");
        windowPosType.Name.Should().Be("WINDOWPOS");
    }

    [Fact]
    public void UnsafeNativeMethods_ShouldBeStaticClass()
    {
        // Assert
        var type = typeof(OotD.Utility.UnsafeNativeMethods);
        type.IsClass.Should().BeTrue();
        type.IsAbstract.Should().BeTrue(); // Static classes are abstract and sealed
        type.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void BitOperations_ShouldWorkWithEdgeCases()
    {
        // Test edge cases for bit operations

        // Zero
        OotD.Utility.UnsafeNativeMethods.Bit.HiWord(0).Should().Be(0);
        OotD.Utility.UnsafeNativeMethods.Bit.LoWord(0).Should().Be(0);

        // Positive values
        OotD.Utility.UnsafeNativeMethods.Bit.HiWord(0x7FFFFFFF).Should().Be(0x7FFF);
        OotD.Utility.UnsafeNativeMethods.Bit.LoWord(0x7FFFFFFF).Should().Be(0xFFFF);

        // Minimum value
        OotD.Utility.UnsafeNativeMethods.Bit.HiWord(int.MinValue).Should().Be(0x8000);
        OotD.Utility.UnsafeNativeMethods.Bit.LoWord(int.MinValue).Should().Be(0x0000);
    }
}
