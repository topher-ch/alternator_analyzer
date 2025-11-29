using System.Runtime.InteropServices;
using OsuParsers.Beatmaps;
using alternator_analyser.Models;
using OsuParsers.Beatmaps.Objects;

namespace alternator_analyser.Services;

public class TimingService
{
    public BeatSnapDivisor? MostCommonBeatSnapDivisor(Beatmap beatmap)
    {
        // Beat snap divisor counts initialized to 0
        Dictionary<BeatSnapDivisor, int> beatSnapDivisorCounts = new Dictionary<BeatSnapDivisor, int>();
        foreach (BeatSnapDivisor beatSnapDivisor in Enum.GetValues<BeatSnapDivisor>())
        {
            beatSnapDivisorCounts.Add(beatSnapDivisor, 0);
        }
        
        // Red-line timing points
        List<TimingPoint> nonInheritedTimingPoints = NoninheritedTimingPoints(beatmap);
        if (nonInheritedTimingPoints.Count == 0)
            return null;
        int i = 0;
        TimingPoint currentTimingPoint = nonInheritedTimingPoints[i];
        TimingPoint? nextTimingPoint = (nonInheritedTimingPoints.Count > 1) ? nonInheritedTimingPoints[1] : null;
        Dictionary<BeatSnapDivisor, double> currentBeatSnapLengths = TimingPointBeatSnapLengths(currentTimingPoint);
        
        // Iterate through hit objects
        for (int j = 0; j < beatmap.HitObjects.Count - 1; j++)
        {
            // specify current and next hit objects
            HitObject prevHitObject = beatmap.HitObjects[j];
            HitObject nextHitObject = beatmap.HitObjects[j + 1];
            
            // make sure the right timing point is being used
            while (nextTimingPoint != null && prevHitObject.StartTime < nextTimingPoint.Offset)
            {
                currentTimingPoint = nextTimingPoint;
                nextTimingPoint = (nonInheritedTimingPoints.Count > i + 1) ? nonInheritedTimingPoints[i + 1] : null;
                i++;
                currentBeatSnapLengths = TimingPointBeatSnapLengths(currentTimingPoint);
            }

            // calculate the distance, find the first beat snap divisor that the distance is larger than,
            // and once found, increment and break
            int distance = nextHitObject.StartTime - prevHitObject.StartTime;
            BeatSnapDivisor closestBeatSnapDivisor = ClosestBeatSnapDivisor(distance, currentBeatSnapLengths);
            beatSnapDivisorCounts[closestBeatSnapDivisor]++;
        }
        
        // Find maximum count
        BeatSnapDivisor runningMax = BeatSnapDivisor.WHOLE;
        foreach (BeatSnapDivisor beatSnapDivisor in Enum.GetValues<BeatSnapDivisor>())
        {
            if (beatSnapDivisorCounts[beatSnapDivisor] >= beatSnapDivisorCounts[runningMax])
                runningMax = beatSnapDivisor;
        }
        return runningMax;
    }

    public static List<TimingPoint> NoninheritedTimingPoints(Beatmap beatmap)
    {
        List<TimingPoint> nonInheritedTimingPoints = new List<TimingPoint>();
        foreach (TimingPoint timingPoint in beatmap.TimingPoints)
        {
            if (!timingPoint.Inherited)
            {
                nonInheritedTimingPoints.Add(timingPoint);
            }
        }
        return nonInheritedTimingPoints;
    }
    
    public static Dictionary<BeatSnapDivisor, double> TimingPointBeatSnapLengths(TimingPoint redLine)
    {
        Dictionary<BeatSnapDivisor, double> beatSnapLengths = new Dictionary<BeatSnapDivisor, double>();
        double beatLength = redLine.BeatLength;
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
        foreach (BeatSnapDivisor beatSnapDivisor in Enum.GetValues<BeatSnapDivisor>())
        {
            if (distance < beatSnapLengths[beatSnapDivisor] - 3)
                continue;
            return beatSnapDivisor;
        }
        return BeatSnapDivisor.WHOLE;
    }
}