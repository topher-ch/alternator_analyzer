using OsuParsers.Beatmaps;
using alternator_analyser.Models;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Decoders;
using OsuParsers.Enums;

namespace alternator_analyser.Services;

public class TimingService(AlternationService alternationService)
{
    public void OnBeatmapChanged(string path)
    {
        Console.WriteLine(path);
        var beatmap = BeatmapDecoder.Decode(path);
        if (beatmap.GeneralSection.Mode != Ruleset.Taiko)
            return;
        alternationService.OnBeatmapChanged(beatmap);
    }

    public static List<TimingPoint> NonInheritedTimingPoints(Beatmap beatmap)
    {
        return beatmap.TimingPoints
            .Where(tp => !tp.Inherited)
            .ToList();
    }
    
    public static Dictionary<BeatSnapDivisor, double> TimingPointBeatSnapLengths(TimingPoint redLine)
    {
        var beatSnapLengths = new Dictionary<BeatSnapDivisor, double>();
        var beatLength = redLine.BeatLength;
        beatSnapLengths[BeatSnapDivisor.WHOLE] = beatLength;
        beatSnapLengths[BeatSnapDivisor.HALF] = beatLength / 2;
        beatSnapLengths[BeatSnapDivisor.THIRD] = beatLength / 3;
        beatSnapLengths[BeatSnapDivisor.QUARTER] = beatLength / 4;
        beatSnapLengths[BeatSnapDivisor.FIFTH] = beatLength / 5;
        beatSnapLengths[BeatSnapDivisor.SIXTH] = beatLength / 6;
        beatSnapLengths[BeatSnapDivisor.SEVENTH] = beatLength / 7;
        beatSnapLengths[BeatSnapDivisor.EIGHTH] = beatLength / 8;
        beatSnapLengths[BeatSnapDivisor.NINTH] = beatLength / 9;
        beatSnapLengths[BeatSnapDivisor.TWELFTH] = beatLength / 12;
        beatSnapLengths[BeatSnapDivisor.SIXTEENTH] = beatLength / 16;
        return beatSnapLengths;
    }

    public static BeatSnapDivisor ClosestBeatSnapDivisor(int distance, Dictionary<BeatSnapDivisor, double> beatSnapLengths)
    {
        var closest = BeatSnapDivisor.SIXTEENTH;
        var closestDistance = double.MaxValue;
        foreach (var divisor in Enum.GetValues<BeatSnapDivisor>())
        {
            var candidateDistance = distance - beatSnapLengths[divisor];
            if (candidateDistance < -5)
                continue;
            if (candidateDistance > closestDistance)
                continue;
            closest = divisor;
            closestDistance = candidateDistance;
        }
        return closest;
    }
}