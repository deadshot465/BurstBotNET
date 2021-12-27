using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json.Serialization;
using BurstBotShared.Shared.Interfaces;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BurstBotShared.Shared.Models.Game.BlackJack.Serializables;

public record RawBlackJackGameState : IRawState<BlackJackGameState, RawBlackJackGameState, BlackJackGameProgress>
{
    [JsonPropertyName("game_id")] 
    [JsonProperty("game_id")] 
    public string GameId { get; init; } = "";
    
    [JsonPropertyName("last_active_time")] 
    [JsonProperty("last_active_time")] 
    public string LastActiveTime { get; init; } = "";
    
    [JsonPropertyName("players")] 
    [JsonProperty("players")] 
    public Dictionary<ulong, RawBlackJackPlayerState> Players { get; init; } = new();
    
    [JsonPropertyName("progress")] 
    [JsonProperty("progress")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public BlackJackGameProgress Progress { get; init; }
    
    [JsonPropertyName("current_player_order")] 
    [JsonProperty("current_player_order")] 
    public int CurrentPlayerOrder { get; init; }
    
    [JsonPropertyName("highest_bet")] 
    [JsonProperty("highest_bet")] 
    public int HighestBet { get; init; }
    
    [JsonPropertyName("current_turn")]
    [JsonProperty("current_turn")] 
    public int CurrentTurn { get; init; }
    
    [JsonPropertyName("previous_player_id")]
    [JsonProperty("previous_player_id")] 
    public ulong PreviousPlayerId { get; init; }
    
    [JsonPropertyName("previous_request_type")]
    [JsonProperty("previous_request_type")]
    public string PreviousRequestType { get; init; } = "";

    public static RawBlackJackGameState FromState(IState<BlackJackGameState, RawBlackJackGameState, BlackJackGameProgress> state)
    {
        var gameState = state as BlackJackGameState;
        return new RawBlackJackGameState
        {
            CurrentPlayerOrder = gameState!.CurrentPlayerOrder,
            CurrentTurn = gameState.CurrentTurn,
            GameId = gameState.GameId,
            HighestBet = gameState.HighestBet,
            LastActiveTime = gameState.LastActiveTime.ToString(CultureInfo.InvariantCulture),
            Players = gameState.Players.ToDictionary(pair => pair.Key, pair => ((IState<BlackJackPlayerState, RawBlackJackPlayerState, BlackJackGameProgress>)pair.Value).ToRaw()),
            PreviousPlayerId = gameState.PreviousPlayerId,
            PreviousRequestType = gameState.PreviousRequestType,
            Progress = gameState.Progress
        };
    }

    public async Task<BlackJackGameState> ToState(DiscordGuild guild)
    {
        var players = Players
            .AsEnumerable()
            .Select(async pair =>
            {
                var (key, value) = pair;
                var playerState = await value.ToState(guild);
                return KeyValuePair.Create(key, playerState);
            })
            .ToList();

        var playerList = new List<KeyValuePair<ulong, BlackJackPlayerState>>();
        foreach (var task in players)
        {
            var pair = await task;
            playerList.Add(pair);
        }

        return new BlackJackGameState
        {
            CurrentPlayerOrder = CurrentPlayerOrder,
            CurrentTurn = CurrentTurn,
            GameId = GameId,
            HighestBet = HighestBet,
            LastActiveTime = DateTime.Parse(LastActiveTime, CultureInfo.InvariantCulture),
            Players = new ConcurrentDictionary<ulong, BlackJackPlayerState>(playerList),
            PreviousPlayerId = PreviousPlayerId,
            PreviousRequestType = PreviousRequestType,
            Progress = Progress
        };
    }
};