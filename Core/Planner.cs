namespace RandevuPlanlayici;

internal record Appointment(string Title, int Start, int Duration)
{
    public int End => Start + Duration;
}

internal static class Planner
{
    public const int Opens = 9 * 60;
    public const int Closes = 18 * 60;

    public static bool FitsWorkday(int start, int duration) =>
        start >= Opens && start < Closes && duration > 0 && duration <= Closes - start;

    public static Appointment? FindConflict(IEnumerable<Appointment> appointments, int start, int duration) =>
        appointments
            .Where(item => start < item.End && item.Start < start + duration)
            .MinBy(item => item.Start);

    public static int? FirstAvailable(IEnumerable<Appointment> appointments, int start, int duration)
    {
        int candidate = Math.Max(Opens, start);
        foreach (var item in appointments.OrderBy(item => item.Start))
        {
            if (item.End <= candidate)
                continue;
            if (item.Start - candidate >= duration)
                break;
            candidate = item.End;
        }

        return FitsWorkday(candidate, duration) ? candidate : null;
    }
}
