﻿using System.Text.Json.Serialization;
using BurstBotShared.Shared.Interfaces;

namespace BurstBotShared.Shared.Models.Localization.NinetyNine.Serializables;
public record NinetyNineLocalization : ILocalization<NinetyNineLocalization>
{
    [JsonPropertyName("commandList")] public Dictionary<string, string> CommandList { get; init; } = new();

    [JsonPropertyName("initialMessagePrefix")]
    public string InitialMessagePrefix { get; init; } = "";

    [JsonPropertyName("initialMessageDescription")]
    public string InitialMessageDescription { get; init; } = "";

    [JsonPropertyName("initialMessageTitle")]
    public string InitialMessageTitle { get; init; } = "";

    [JsonPropertyName("initialMessageFooter")]
    public string InitialMessageFooter { get; init; } = "";

    [JsonPropertyName("winTitle")] public string WinTitle { get; init; } = "";
    [JsonPropertyName("winDescription")] public string WinDescription { get; init; } = "";
    [JsonPropertyName("won")] public string Won { get; init; } = "";
    [JsonPropertyName("lost")] public string Lost { get; init; } = "";

    [JsonPropertyName("cards")] public string Cards { get; init; } = "";
    [JsonPropertyName("play")] public string Play { get; init; } = "";
    [JsonPropertyName("playMessage")] public string PlayMessage { get; init; } = "";
    [JsonPropertyName("turnMessageTitle")] public string TurnMessageTitle { get; init; } = "";
    [JsonPropertyName("showHelp")] public string ShowHelp { get; init; } = "";

    public Dictionary<string, string> AvailableCommands => CommandList;
};
