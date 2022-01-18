﻿using System.Text.Json;
using BurstBotShared.Shared.Models.Data;
using BurstBotShared.Shared.Models.Game.Serializables;
using BurstBotShared.Shared.Models.Game.NinetyNine.Serializables;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace BurstBotShared.Shared.Models.Game.NinetyNine;

public class NinetyNineDropDownEntity : ISelectMenuInteractiveEntity
{
    private readonly InteractionContext _context;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly State _state;
    private readonly ILogger<NinetyNineDropDownEntity> _logger;

    public NinetyNineDropDownEntity(InteractionContext context,
        IDiscordRestChannelAPI channelApi,
        IDiscordRestInteractionAPI interactionApi,
        State state,
        ILogger<NinetyNineDropDownEntity> logger)
    {
        _context = context;
        _channelApi = channelApi;
        _interactionApi = interactionApi;
        _state = state;
        _logger = logger;
    }

    public Task<Result<bool>> IsInterestedAsync(ComponentType componentType, string customId, CancellationToken ct = new())
    {
        return componentType is not ComponentType.SelectMenu
            ? Task.FromResult<Result<bool>>(false)
            : Task.FromResult<Result<bool>>(customId is "ninety_nine_user_selection");
    }
    public async Task<Result> HandleInteractionAsync(IUser user, string customId, IReadOnlyList<string> values,
    CancellationToken ct = new())
    {
        var hasMessage = _context.Message.IsDefined(out var message);
        if (!hasMessage) return Result.FromSuccess();

        var gameState = Utilities.Utilities
            .GetGameState<NinetyNineGameState, NinetyNinePlayerState, NinetyNineGameProgress>(user,
                _state.GameStates.NinetyNineGameStates.Item1,
                _state.GameStates.NinetyNineGameStates.Item2,
                _context,
                out var playerState);
        if (playerState == null) return Result.FromSuccess();

        var sanitizedCustomId = customId.Trim();

        if (!string.Equals(sanitizedCustomId, "ninety_nine_user_selection")) return Result.FromSuccess();

        return await PlayCollect(message!, gameState, values, ct);
    }

    private async Task<Result> PlayCollect(
    IMessage message,
    NinetyNineGameState gameState,
    IEnumerable<string> values,
    CancellationToken ct)
    {
        var extractedCard = ExtractCard(values);

        var currentPlayer = gameState
            .Players
            .First(p => p.Value.Order == gameState.CurrentPlayerOrder)
            .Value;

        await gameState.Channel!.Writer.WriteAsync(new Tuple<ulong, byte[]>(
            currentPlayer.PlayerId,
            JsonSerializer.SerializeToUtf8Bytes(new NinetyNineInGameRequest
            {
                GameId = gameState.GameId,
                PlayerId = currentPlayer.PlayerId,
                PlayCard = extractedCard,
                RequestType = NinetyNineInGameRequestType.Play
            })), ct);

        await Utilities.Utilities.DisableComponents(message, true, _channelApi, _logger, ct);

        return Result.FromSuccess();
    }
    private static Card ExtractCard(IEnumerable<string> values)
    {
        var selection = values.FirstOrDefault()!;
        var suit = selection[..1];
        var rank = selection[1..];
        var playedCard = Card.Create(suit, rank);
        return playedCard;
    }
}
