// See https://aka.ms/new-console-template for more information

using alternator_analyser.Models;
using alternator_analyser.Services;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;

TimingService timingService = new TimingService();

Beatmap beatmap =
    BeatmapDecoder.Decode(
        "/home/christopher/RiderProjects/alternator_analyser/Cansol - Train of Thought (Nurend) [Last Stop].osu");
Console.WriteLine(timingService.SingletapBeatSnapDivisor(beatmap));

beatmap = BeatmapDecoder.Decode("/home/christopher/RiderProjects/alternator_analyser/DM DOKURO - Reality Check Through The Skull (T w i g) [Determination].osu");
Console.WriteLine(timingService.SingletapBeatSnapDivisor(beatmap));