using Xunit;

namespace RandevuPlanlayici.Tests;

public class PlannerTests
{
    [Fact]
    public void FindsEarliestConflictWhenAppointmentsAreUnsorted()
    {
        Appointment[] appointments = [new("İkinci", 660, 30), new("İlk", 630, 30)];

        var conflict = Planner.FindConflict(appointments, 645, 45);

        Assert.Same(appointments[1], conflict);
    }

    [Fact]
    public void AllowsAppointmentsToTouchAtBothEnds()
    {
        Appointment[] appointments = [new("İlk", 570, 30), new("İkinci", 630, 30)];

        Assert.Null(Planner.FindConflict(appointments, 600, 30));
    }

    [Fact]
    public void SuggestsFirstGapLongEnoughForAppointment()
    {
        Appointment[] appointments = [new("Son", 720, 30), new("İlk", 630, 60)];

        Assert.Equal(750, Planner.FirstAvailable(appointments, 630, 45));
    }

    [Fact]
    public void AcceptsAnExactFitBetweenAppointments()
    {
        Appointment[] appointments = [new("İlk", 570, 30), new("İkinci", 630, 30)];

        Assert.Equal(600, Planner.FirstAvailable(appointments, 600, 30));
    }

    [Fact]
    public void ReturnsNoSuggestionWhenTheDayIsFull()
    {
        Appointment[] appointments = [new("Son kayıt", 1020, 60)];

        Assert.Null(Planner.FirstAvailable(appointments, 1020, 15));
    }

    [Theory]
    [InlineData(539, 15, false)]
    [InlineData(540, 15, true)]
    [InlineData(1070, 10, true)]
    [InlineData(1070, 11, false)]
    public void ChecksOpeningAndClosingBoundaries(int start, int duration, bool expected)
    {
        Assert.Equal(expected, Planner.FitsWorkday(start, duration));
    }
}
