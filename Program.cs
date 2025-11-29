// See https://aka.ms/new-console-template for more information

using alternator_analyser.Models;
using alternator_analyser.Services;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;

TimingService timingService = new TimingService();
AlternationService alternationService = new AlternationService();
alternationService.RedDefaultHand = HandAssignment.RIGHT;
alternationService.BlueDefaultHand = HandAssignment.RIGHT;
alternationService.ResetOnFinishers = true;
alternationService.ResetOnSingletapSnapDivisor = false;
StatsService statsService = new StatsService();

Beatmap beatmap =
    BeatmapDecoder.Decode(
        "C:\\Users\\chris\\RiderProjects\\alternator_analyzer\\Cansol - Train of Thought (Nurend) [Last Stop].osu");
BeatSnapDivisor? mostCommonBeatSnapDivisor = timingService.MostCommonBeatSnapDivisor(beatmap);
if (mostCommonBeatSnapDivisor == null)
{
    Console.WriteLine("no singletap beat snap divisor found");
    return;
}
List<AlternationService.AlternatedHitObject> alternatedHitObjects = alternationService.MapAlternation(beatmap, mostCommonBeatSnapDivisor.Value);
foreach (AlternationService.AlternatedHitObject alternatedObject in alternatedHitObjects)
{
    Console.WriteLine("Offset: " + alternatedObject.hitObject.StartTime);
    Console.WriteLine("Hand Assignment: " + alternatedObject.handAssignment);
}
Console.WriteLine("");
Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), StatsService.Counts> stats = statsService.Stats(beatmap, BeatSnapDivisor.QUARTER, alternatedHitObjects);
foreach ((BeatSnapDivisor beatSnapDivisor, int length) in stats.Keys)
{
    Console.WriteLine("beatSnapDivisor: " + beatSnapDivisor);
    Console.WriteLine("length: " + length);
    Console.WriteLine("leftCount: " + stats[(beatSnapDivisor, length)].LeftCount);
    Console.WriteLine("rightCount: " + stats[(beatSnapDivisor, length)].RightCount);
    Console.WriteLine("bothCount: " + stats[(beatSnapDivisor, length)].BothCount);
}
StatsService.Counts overall = statsService.OverallCounts(stats);
StatsService.Counts overallNoSingletaps = statsService.OverallCountsNoSingletaps(stats);
Console.WriteLine("overall: " + overall);
Console.WriteLine("overallNoSingletaps: " + overallNoSingletaps);