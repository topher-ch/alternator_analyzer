// See https://aka.ms/new-console-template for more information

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using alternator_analyser.Models;
using alternator_analyser.Services;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;

// TimingService timingService = new TimingService();
// AlternationService alternationService = new AlternationService();
// alternationService.RedDefaultHand = HandAssignment.RIGHT;
// alternationService.BlueDefaultHand = HandAssignment.RIGHT;
// alternationService.ResetOnFinishers = true;
// alternationService.ResetOnSingletapSnapDivisor = false;
// StatsService statsService = new StatsService();
//
// Beatmap beatmap =
//     BeatmapDecoder.Decode(
//         "C:\\Users\\chris\\RiderProjects\\alternator_analyzer\\Cansol - Train of Thought (Nurend) [Last Stop].osu");
// BeatSnapDivisor? mostCommonBeatSnapDivisor = timingService.MostCommonBeatSnapDivisor(beatmap);
// if (mostCommonBeatSnapDivisor == null)
// {
//     Console.WriteLine("no singletap beat snap divisor found");
//     return;
// }
// List<AlternationService.AlternatedHitObject> alternatedHitObjects = alternationService.MapAlternation(beatmap, mostCommonBeatSnapDivisor.Value);
// foreach (AlternationService.AlternatedHitObject alternatedObject in alternatedHitObjects)
// {
//     Console.WriteLine("Offset: " + alternatedObject.hitObject.StartTime);
//     Console.WriteLine("Hand Assignment: " + alternatedObject.handAssignment);
// }
// Console.WriteLine("");
// Dictionary<(BeatSnapDivisor beatSnapDivisor, int length), StatsService.Counts> stats = statsService.Stats(beatmap, BeatSnapDivisor.QUARTER, alternatedHitObjects);
// foreach ((BeatSnapDivisor beatSnapDivisor, int length) in stats.Keys)
// {
//     Console.WriteLine("beatSnapDivisor: " + beatSnapDivisor);
//     Console.WriteLine("length: " + length);
//     Console.WriteLine("leftCount: " + stats[(beatSnapDivisor, length)].LeftCount);
//     Console.WriteLine("rightCount: " + stats[(beatSnapDivisor, length)].RightCount);
//     Console.WriteLine("bothCount: " + stats[(beatSnapDivisor, length)].BothCount);
// }
// StatsService.Counts overall = statsService.OverallCounts(stats);
// StatsService.Counts overallNoSingletaps = statsService.OverallCountsNoSingletaps(stats);
// Console.WriteLine("overall: " + overall);
// Console.WriteLine("overallNoSingletaps: " + overallNoSingletaps);

ClientWebSocket webSocket = new ClientWebSocket();
await webSocket.ConnectAsync(new Uri("ws://localhost:24050/ws"), CancellationToken.None);
var buffer = new byte[8192 * 100];

while (webSocket.State == WebSocketState.Open)
{
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    if (result.MessageType == WebSocketMessageType.Close)
        break;
    
    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
    var data = JsonDocument.Parse(json);
    var root = data.RootElement;

    if (root.TryGetProperty("settings", out var settings) &&
        settings.TryGetProperty("folders", out var folders) &&
        folders.TryGetProperty("songs", out var songs) &&
        root.TryGetProperty("menu", out var menu) &&
        menu.TryGetProperty("bm", out var bm) &&
        bm.TryGetProperty("path", out var path) &&
        path.TryGetProperty("folder", out var folder) &&
        path.TryGetProperty("file", out var file))
    {
        String fullPath = Path.Combine(songs.ToString(), folder.ToString(), file.ToString());
        Console.WriteLine(fullPath);
    }
}
