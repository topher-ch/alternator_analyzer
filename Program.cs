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

Beatmap beatmap =
    BeatmapDecoder.Decode(
        "/home/christopher/RiderProjects/alternator_analyser/Cansol - Train of Thought (Nurend) [Last Stop].osu");
BeatSnapDivisor? singletapBeatSnapDivisor = timingService.SingletapBeatSnapDivisor(beatmap);
if (singletapBeatSnapDivisor == null)
{
    Console.WriteLine("no singletap beat snap divisor found");
    return;
}
List<AlternationService.AlternatedHitObject> alternatedHitObjects = alternationService.MapAlternation(beatmap, singletapBeatSnapDivisor.Value);
foreach (AlternationService.AlternatedHitObject alternatedObject in alternatedHitObjects)
{
    Console.WriteLine("Offset: " + alternatedObject.hitObject.StartTime);
    Console.WriteLine("Hand Assignment: " + alternatedObject.handAssignment);
}