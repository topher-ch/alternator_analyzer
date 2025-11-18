using alternator_analyser.Models;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps.Objects.Taiko;
using OsuParsers.Enums.Beatmaps;

namespace alternator_analyser.Services;

public class AlternationService
{
    public HandAssignment RedDefaultHand;
    public HandAssignment BlueDefaultHand;
    public bool ResetOnFinishers;
    public bool ResetOnSingletapSnapDivisor;
    
    public class AlternatedHitObject
    {
        public HitObject hitObject { get; set; }
        public HandAssignment  handAssignment { get; set; }
    }
    
    public List<AlternatedHitObject> MapAlternation(Beatmap beatmap, BeatSnapDivisor singletapBeatSnapDivisor)
    {
        List<AlternatedHitObject> alternatedHitObjects = new List<AlternatedHitObject>();
        
        // Red-line timing points
        List<TimingPoint> nonInheritedTimingPoints = TimingService.NoninheritedTimingPoints(beatmap);
        if (nonInheritedTimingPoints.Count == 0)
            return null;
        int i = 0;
        TimingPoint currentTimingPoint = nonInheritedTimingPoints[i];
        TimingPoint? nextTimingPoint = (nonInheritedTimingPoints.Count > 1) ? nonInheritedTimingPoints[1] : null;
        Dictionary<BeatSnapDivisor, double> currentBeatSnapLengths = TimingService.TimingPointBeatSnapLengths(currentTimingPoint);
        
        // Iterate through hit objects, find the hand assignment and append to the list
        AlternatedHitObject? prevAltHitObject = null;
        for (int j = 0; j < beatmap.HitObjects.Count; j++)
        {
            // specify next hit object
            HitObject nextHitObject = beatmap.HitObjects[j];
            
            // make sure the right timing point is being used
            while (nextTimingPoint != null && prevAltHitObject != null && 
                   prevAltHitObject.hitObject.StartTime < nextTimingPoint.Offset)
            {
                currentTimingPoint = nextTimingPoint;
                nextTimingPoint = (nonInheritedTimingPoints.Count > i + 1) ? nonInheritedTimingPoints[i + 1] : null;
                i++;
                currentBeatSnapLengths = TimingService.TimingPointBeatSnapLengths(currentTimingPoint);
            }

            // find the next hand assignment
            HandAssignment handAssignment = NextHandAssignment(prevAltHitObject, nextHitObject, currentBeatSnapLengths,
                singletapBeatSnapDivisor);
            AlternatedHitObject alternatedHitObject = new AlternatedHitObject();
            alternatedHitObject.hitObject = nextHitObject;
            alternatedHitObject.handAssignment = handAssignment;
            
            // append the new alternated hit object and update previous alternated hit object
            alternatedHitObjects.Add(alternatedHitObject);
            prevAltHitObject = alternatedHitObject;
        }
        
        return alternatedHitObjects;
    }

    public HandAssignment NextHandAssignment(AlternatedHitObject? prevAltHitObject, HitObject nextHitObject, 
        Dictionary<BeatSnapDivisor, double> currentBeatSnapLengths, BeatSnapDivisor singletapBeatSnapDivisor)
    {
        // if there is no previous object
        if (prevAltHitObject == null)
        {
            switch (nextHitObject)
            {
                // a spinner or drumroll is always BOTH
                case TaikoSpinner or TaikoDrumroll:
                    return HandAssignment.BOTH;
                // if a finisher and resetOnFinishers then BOTH, otherwise red/blueDefault
                case TaikoHit taikoHit:
                    if (taikoHit.IsBig)
                        if (ResetOnFinishers)
                            return HandAssignment.BOTH;
                    return (taikoHit.Color == TaikoColor.Red) ? RedDefaultHand : BlueDefaultHand;
            }

            Console.WriteLine("NextHandAssignment failed for first object");
            return HandAssignment.BOTH;
        }

        // otherwise prevAltHitObject exists
        switch (nextHitObject)
        {
            // a spinner or drumroll is always BOTH
            case TaikoSpinner or TaikoDrumroll:
                return HandAssignment.BOTH;
            // if a finisher and resetOnFinishers then BOTH
            case TaikoHit taikoHit:
                if (taikoHit.IsBig)
                    if (ResetOnFinishers)
                        return HandAssignment.BOTH;
                // otherwise defaulted/alternated depending on distance
                int distance = nextHitObject.StartTime - prevAltHitObject.hitObject.StartTime;
                BeatSnapDivisor closestBeatSnapDivisor =
                    TimingService.ClosestBeatSnapDivisor(distance, currentBeatSnapLengths);
                if (prevAltHitObject.handAssignment == HandAssignment.BOTH 
                    || (ResetOnSingletapSnapDivisor && closestBeatSnapDivisor >= singletapBeatSnapDivisor))
                {
                    return (taikoHit.Color == TaikoColor.Red) ? RedDefaultHand : BlueDefaultHand;
                }
                return prevAltHitObject.handAssignment == HandAssignment.LEFT ? HandAssignment.RIGHT : HandAssignment.LEFT;
        }
        
        Console.WriteLine("NextHandAssignment failed for non-first object");
        return HandAssignment.BOTH;
    }
}