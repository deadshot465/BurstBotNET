using System.Collections.Concurrent;
using System.Threading.Channels;
using BurstBotNET.Shared.Models.Game.BlackJack.Serializables;

namespace BurstBotNET.Shared.Models.Game.BlackJack;

public class BlackJackGameState
{
    public string GameId { get; set; } = "";
    public DateTime LastActiveTime { get; set; } = DateTime.Now;
    public ConcurrentDictionary<long, BlackJackPlayerState> Players { get; set; } = new(10, 6);
    public BlackJackGameProgress Progress { get; set; } = BlackJackGameProgress.NotAvailable;
    public int CurrentPlayerOrder { get; set; }
    public long PreviousPlayerId { get; set; }
    public string PreviousRequestType { get; set; } = "";
    public int HighestBet { get; set; }
    public int CurrentTurn { get; set; }
    public Channel<Tuple<long, string>>? Channel { get; set; }
}