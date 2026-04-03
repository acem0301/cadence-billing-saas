using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace CadenceBilling.UnitTests;

public class BillingCadenceTests
{
    private static BillingCadence CreateCadence(BillingFrequency frequency, DateOnly nextBillingDate) => new()
    {
        Id = Guid.NewGuid(),
        Description = "Test Cadence",
        Amount = 500,
        Frequency = frequency,
        NextBillingDate = nextBillingDate,
        CustomerId = Guid.NewGuid(),
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public void AdvanceNextBillingDate_Weekly_AddsSevenDays()
    {
        var original = new DateOnly(2026, 1, 1);
        var cadence = CreateCadence(BillingFrequency.Weekly, original);

        cadence.AdvanceNextBillingDate();

        cadence.NextBillingDate.Should().Be(original.AddDays(7));
    }

    [Fact]
    public void AdvanceNextBillingDate_BiWeekly_AddsFifteenDays()
    {
        var original = new DateOnly(2026, 1, 1);
        var cadence = CreateCadence(BillingFrequency.BiWeekly, original);

        cadence.AdvanceNextBillingDate();

        cadence.NextBillingDate.Should().Be(original.AddDays(15));
    }

    [Fact]
    public void AdvanceNextBillingDate_Monthly_AddsOneMonth()
    {
        var original = new DateOnly(2026, 1, 31);
        var cadence = CreateCadence(BillingFrequency.Monthly, original);

        cadence.AdvanceNextBillingDate();

        cadence.NextBillingDate.Should().Be(original.AddMonths(1));
    }

    [Fact]
    public void AdvanceNextBillingDate_Monthly_HandlesEndOfMonth()
    {
        var original = new DateOnly(2026, 1, 31);
        var cadence = CreateCadence(BillingFrequency.Monthly, original);

        var act = () => cadence.AdvanceNextBillingDate();

        act.Should().NotThrow();
        cadence.NextBillingDate.Month.Should().Be(2);
    }

    [Fact]
    public void AdvanceNextBillingDate_UnknownFrequency_Throws()
    {
        var cadence = CreateCadence((BillingFrequency)99, new DateOnly(2026, 1, 1));

        var act = () => cadence.AdvanceNextBillingDate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown frequency*");
    }
}