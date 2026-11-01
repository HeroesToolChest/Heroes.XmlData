namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class ValueScaleTests
{
    [TestMethod]
    public void Default_HasZeroValueAndNoScaling()
    {
        // arrange
        ValueScale scale = default;

        // act
        int valueDigits = scale.TotalValueDigits();
        int scalingDigits = scale.TotalScalingDigits();

        // assert
        scale.Value.Should().Be(0);
        scale.Scaling.Should().BeNull();
        valueDigits.Should().Be(1);
        scalingDigits.Should().Be(0);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(1.5)]
    [DataRow(-5.0)]
    [DataRow(-1_000.0)]
    [DataRow(999_998.0)]
    [DataRow(999_999.0)]
    [DataRow(-999_999.0)]
    public void Constructor_ValueOnly_WithinLimit_KeepsValueAndLeavesScalingNull(double value)
    {
        // arrange
        double expectedValue = value;

        // act
        ValueScale scale = new(value);

        // assert
        scale.Value.Should().Be(expectedValue);
        scale.Scaling.Should().BeNull();
    }

    [TestMethod]
    [DataRow(999_999.5)]
    [DataRow(1_000_000.0)]
    [DataRow(1_000_000_000_000.0)]
    [DataRow(double.MaxValue)]
    [DataRow(double.PositiveInfinity)]
    public void Constructor_ValueOnly_AboveMax_ClampsToMax(double value)
    {
        // arrange
        const double expectedValue = 999_999;

        // act
        ValueScale scale = new(value);

        // assert
        scale.Value.Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow(-999_999.5)]
    [DataRow(-1_000_000.0)]
    [DataRow(-1_000_000_000_000.0)]
    [DataRow(double.MinValue)]
    [DataRow(double.NegativeInfinity)]
    public void Constructor_ValueOnly_BelowMin_ClampsToMin(double value)
    {
        // arrange
        const double expectedValue = -999_999;

        // act
        ValueScale scale = new(value);

        // assert
        scale.Value.Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow(0.04)]
    [DataRow(0.035)]
    [DataRow(-0.04)]
    [DataRow(0.999999)]
    [DataRow(-0.999999)]
    public void Constructor_ScalingWithinLimit_KeepsScaling(double scaling)
    {
        // arrange
        // act
        ValueScale scale = new(1, scaling);

        // assert
        scale.Scaling.Should().Be(scaling);
    }

    [TestMethod]
    [DataRow(1.0, 0.999999)]
    [DataRow(9.5, 0.999999)]
    [DataRow(1_000_000_000.0, 0.999999)]
    [DataRow(double.PositiveInfinity, 0.999999)]
    [DataRow(-1.0, -0.999999)]
    [DataRow(-5.0, -0.999999)]
    [DataRow(double.NegativeInfinity, -0.999999)]
    public void Constructor_ScalingOneOrMore_ClampsBelowOne(double scaling, double expectedScaling)
    {
        // arrange
        // act
        ValueScale scale = new(1, scaling);

        // assert
        scale.Scaling.Should().Be(expectedScaling);
    }

    [TestMethod]
    public void Constructor_ValueAndScaling_SetsBoth()
    {
        // arrange
        double value = 12.5;
        double scaling = 0.25;

        // act
        ValueScale scale = new(value, scaling);

        // assert
        scale.Value.Should().Be(12.5);
        scale.Scaling.Should().Be(0.25);
    }

    [TestMethod]
    public void Constructor_ValueAndScaling_ClampsBoth()
    {
        // arrange
        double value = 5_000_000_000.0;
        double scaling = 1_000_000_000.0;

        // act
        ValueScale scale = new(value, scaling);

        // assert
        scale.Value.Should().Be(999_999);
        scale.Scaling.Should().Be(0.999999);
    }

    [TestMethod]
    [DataRow(0.0, 1)]
    [DataRow(-0.0, 1)] // normalized to 0
    [DataRow(5.0, 1)]
    [DataRow(10.0, 2)]
    [DataRow(12345.0, 5)]
    [DataRow(999_999.0, 6)]
    [DataRow(-999_999.0, 7)]
    [DataRow(-5.0, 2)]
    [DataRow(-100.0, 4)]
    [DataRow(0.5, 3)]
    [DataRow(0.25, 4)]
    [DataRow(0.125, 5)]
    [DataRow(1.5, 3)]
    [DataRow(-1.5, 4)]
    [DataRow(-0.5, 4)]
    [DataRow(3.14, 4)]
    [DataRow(123.5, 5)]
    [DataRow(0.123456789, 8)] // rounded to 6 decimal places
    public void TotalValueDigits_ReturnsExpectedCount(double value, int expected)
    {
        // arrange
        ValueScale scale = new(value);

        // act
        int result = scale.TotalValueDigits();

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void Constructor_NegativeZero_NormalizesToPositiveZero()
    {
        // arrange
        // act
        ValueScale scale = new(-0.0, -0.0);

        // assert
        double.IsNegative(scale.Value).Should().BeFalse();
        double.IsNegative(scale.Scaling!.Value).Should().BeFalse();
    }

    [TestMethod]
    public void TotalValueDigits_ValueAboveMax_CountsClampedValue()
    {
        // arrange
        ValueScale scale = new(1_000_000_000_000.0);

        // act
        int result = scale.TotalValueDigits();

        // assert
        result.Should().Be(6);
    }

    [TestMethod]
    public void TotalScalingDigits_NoScaling_ReturnsZero()
    {
        // arrange
        ValueScale scale = new(123.5);

        // act
        int result = scale.TotalScalingDigits();

        // assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void TotalScalingDigits_ZeroScaling_ReturnsOne()
    {
        // arrange
        ValueScale scale = new(123.5, 0);

        // act
        int result = scale.TotalScalingDigits();

        // assert
        result.Should().Be(1);
    }

    [TestMethod]
    [DataRow(0.04, 4)]
    [DataRow(0.035, 5)]
    [DataRow(-0.04, 5)]
    [DataRow(0.5, 3)]
    [DataRow(0.125, 5)]
    [DataRow(-0.5, 4)]
    [DataRow(0.999999, 8)]
    [DataRow(5.0, 8)] // clamped to 0.999999
    [DataRow(-5.0, 9)] // clamped to -0.999999
    public void TotalScalingDigits_ReturnsExpectedCount(double scaling, int expected)
    {
        // arrange
        ValueScale scale = new(1, scaling);

        // act
        int result = scale.TotalScalingDigits();

        // assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void TotalValueDigits_And_TotalScalingDigits_AreIndependent()
    {
        // arrange
        ValueScale scale = new(1234, 0.5);

        // act
        int valueDigits = scale.TotalValueDigits();
        int scalingDigits = scale.TotalScalingDigits();

        // assert
        valueDigits.Should().Be(4);
        scalingDigits.Should().Be(3);
    }
}
