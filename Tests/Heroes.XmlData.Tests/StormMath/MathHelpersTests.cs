namespace Heroes.XmlData.StormMath.Tests;

[TestClass]
public class MathHelpersTests
{
    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1235, 4)]
    [DataRow(-1235, 5)]
    [DataRow(123.456789, 10)]
    [DataRow(-123.456789, 11)]
    [DataRow(123.100000001, 5)]
    [DataRow(-123.100000001, 6)]
    [DataRow(0.04, 4)]
    [DataRow(0.123456, 8)]
    [DataRow(0.1234567, 8)]
    [DataRow(0.000001, 8)]
    [DataRow(0.0000001, 8)]
    [DataRow(0.00000001, 3)]
    [DataRow(0.000000001, 3)]
    public void GetCountOfDigits_HasNumber_ReturnsCount(double number, int count)
    {
        // arrange
        // act
        int result = MathHelpers.GetCountOfDigits(number);

        // assert
        result.Should().Be(count);
    }
}
