using alternator_analyser.Models;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;

namespace alternator_analyser.Services;

public class StatsService
{
    public record Counts
    {
        public int LeftCount { get; set; }
        public int RightCount { get; set; }
        public int BothCount { get; set; }
    }
    
    public Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), Counts> Stats(Beatmap beatmap, BeatSnapDivisor alternatedBeatSnapDivisor,
        List<AlternationService.AlternatedHitObject> alternatedHitObjects)
    {
        Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), Counts> patternCounts =
            new Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), Counts>();

        // Red-line timing points
        List<TimingPoint> nonInheritedTimingPoints = TimingService.NoninheritedTimingPoints(beatmap);
        if (nonInheritedTimingPoints.Count == 0)
            return null;
        int i = 0;
        TimingPoint currentTimingPoint = nonInheritedTimingPoints[i];
        TimingPoint? nextTimingPoint = (nonInheritedTimingPoints.Count > 1) ? nonInheritedTimingPoints[1] : null;
        Dictionary<BeatSnapDivisor, double> currentBeatSnapLengths = TimingService.TimingPointBeatSnapLengths(currentTimingPoint);

        BeatSnapDivisor beatSnapDivisors = 0;
        HandAssignment patternHandStart = alternatedHitObjects[0].handAssignment;
        int patternLength = 1;
        
        // Iterate through alternated hit objects
        for (int j = 0; j < alternatedHitObjects.Count - 1; j++)
        {
            // specify previous and next hit objects
            AlternationService.AlternatedHitObject prevHitObject = alternatedHitObjects[j];
            AlternationService.AlternatedHitObject nextHitObject = alternatedHitObjects[j + 1];
            
            // make sure the right timing point is being used
            while (nextTimingPoint != null && prevHitObject.hitObject.StartTime < nextTimingPoint.Offset)
            {
                currentTimingPoint = nextTimingPoint;
                nextTimingPoint = (nonInheritedTimingPoints.Count > i + 1) ? nonInheritedTimingPoints[i + 1] : null;
                i++;
                currentBeatSnapLengths = TimingService.TimingPointBeatSnapLengths(currentTimingPoint);
            }
            
            // calculate distance and find the closest beat snap divisor
            int distance = nextHitObject.hitObject.StartTime - prevHitObject.hitObject.StartTime;
            BeatSnapDivisor closestBeatSnapDivisor = TimingService.ClosestBeatSnapDivisor(distance, currentBeatSnapLengths);

            // if it is not the end of a pattern, update length and divisors and continue to the next hitobjects
            if (closestBeatSnapDivisor >= alternatedBeatSnapDivisor)
            {
                beatSnapDivisors |= closestBeatSnapDivisor;
                patternLength += 1;
                continue;
            }

            // otherwise, it is the end of a pattern, increment the respective count, initializing counts first if necessary
            if (!patternCounts.ContainsKey((beatSnapDivisors, patternLength)))
            {
                patternCounts[(beatSnapDivisors, patternLength)] = new Counts();
            }
            switch (patternHandStart)
            {
                case HandAssignment.LEFT:
                    patternCounts[(beatSnapDivisors, patternLength)].LeftCount++;
                    break;
                case HandAssignment.RIGHT:
                    patternCounts[(beatSnapDivisors, patternLength)].RightCount++;
                    break;
                case HandAssignment.BOTH:
                    patternCounts[(beatSnapDivisors, patternLength)].BothCount++;
                    break;
            }
            
            // then reset pattern trackers
            beatSnapDivisors = 0;
            patternHandStart = nextHitObject.handAssignment;
            patternLength = 1;
        }
        return patternCounts;
    }

    public Counts OverallCounts(Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), Counts> counts)
    {
        Counts overall = new Counts();
        foreach ((BeatSnapDivisor beatSnapDivisor, int length) in counts.Keys)
        {
            overall.LeftCount += counts[(beatSnapDivisor, length)].LeftCount;
            overall.RightCount += counts[(beatSnapDivisor, length)].RightCount;
            overall.BothCount += counts[(beatSnapDivisor, length)].BothCount;
        }
        return overall;
    }
    
    public Counts OverallCountsNoSingletaps(Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), Counts> counts)
    {
        Counts overall = new Counts();
        foreach ((BeatSnapDivisor beatSnapDivisor, int length) in counts.Keys)
        {
            if (beatSnapDivisor == 0)
                continue;
            overall.LeftCount += counts[(beatSnapDivisor, length)].LeftCount;
            overall.RightCount += counts[(beatSnapDivisor, length)].RightCount;
            overall.BothCount += counts[(beatSnapDivisor, length)].BothCount;
        }
        return overall;
    }
}