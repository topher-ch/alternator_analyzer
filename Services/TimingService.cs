using System.Runtime.InteropServices;
using OsuParsers.Beatmaps;
using alternator_analyser.Models;
using OsuParsers.Beatmaps.Objects;

namespace alternator_analyser.Services;

public class TimingService
{
    public BeatSnapDivisor? SingletapBeatSnapDivisor(Beatmap beatmap)
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
            // calculate the distance between the current and next hit object
            HitObject prevHitObject = beatmap.HitObjects[j];
            HitObject nextHitObject = beatmap.HitObjects[j + 1];
            double distance = nextHitObject.StartTime - prevHitObject.StartTime;

            // find the first beat snap divisor that the distance is larger than, once found, increment and break
            foreach (BeatSnapDivisor beatSnapDivisor in Enum.GetValues<BeatSnapDivisor>())
            {
                if (distance < currentBeatSnapLengths[beatSnapDivisor] - 3)
                    continue;
                beatSnapDivisorCounts[beatSnapDivisor]++;
                break;
            }
            
            // if the next hit object is past the next timing point, update current and next timing points
            if (nextTimingPoint != null && nextHitObject.StartTime >= nextTimingPoint.Offset)
            {
                currentTimingPoint = nextTimingPoint;
                nextTimingPoint = (nonInheritedTimingPoints.Count > i + 1) ?  nonInheritedTimingPoints[i + 1] : null;
                currentBeatSnapLengths = TimingPointBeatSnapLengths(currentTimingPoint);
            }
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

    public List<TimingPoint> NoninheritedTimingPoints(Beatmap beatmap)
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
    
    public Dictionary<BeatSnapDivisor, double> TimingPointBeatSnapLengths(TimingPoint redLine)
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
}