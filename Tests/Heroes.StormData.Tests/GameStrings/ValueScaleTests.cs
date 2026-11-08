namespace Heroes.StormData.GameStrings.Tests;

[TestClass]
public sealed class ValueScaleTests
{
    [TestMethod]
    public void Default_HasZeroValueAndNoScaling()
    {
        // arrange
        ValueScale scale = default;

        // assert
        scale.Value.Should().Be(0);
        scale.Scaling.Should().BeNull();
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
    public void Constructor_NegativeZero_NormalizesToPositiveZero()
    {
        // arrange
        // act
        ValueScale scale = new(-0.0, -0.0);

        // assert
        double.IsNegative(scale.Value).Should().BeFalse();
        double.IsNegative(scale.Scaling!.Value).Should().BeFalse();
    }
}
