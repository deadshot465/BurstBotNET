using System.Globalization;
using BurstBotShared.Shared;
using BurstBotShared.Shared.Extensions;
using BurstBotShared.Shared.Interfaces;
using BurstBotShared.Shared.Models.Game.NinetyNine.Serializables;
using BurstBotShared.Shared.Models.Game.Serializables;
using BurstBotShared.Shared.Models.Game.NinetyNine;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;
using Microsoft.Extensions.Logging;
using Utilities = BurstBotShared.Shared.Utilities.Utilities;

namespace BurstBotNET.Commands.NinetyNine;

using NinetyNineGame = IGame<NinetyNineGameState, RawNinetyNineGameState, NinetyNine, NinetyNinePlayerState, NinetyNineGameProgress, NinetyNineInGameRequestType>;
public partial class NinetyNine
{
    private static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;
    
    private async Task<IResult> Join(float baseBet, NinetyNineDifficulty difficulty, NinetyNineVariation variation, params IUser?[] users)
    {
        var joinResult = await Game.GenericJoinGame(
            baseBet, users, GameType.NinetyNine, "/ninety_nine/join",
            _state, _context, _interactionApi, _userApi, _logger
            );
        if (joinResult == null) return Result.FromSuccess();

        switch (joinResult.JoinStatus.StatusType)
        {
            case GenericJoinStatusType.Start:
                {
                    var result = await Game.GenericStartGame(
                        _context, joinResult.Reply, joinResult.InvokingMember, joinResult.BotUser,
                        joinResult.JoinStatus, GameName, "/chinese_poker/join/confirm",
                        joinResult.MentionedPlayers.Select(s => s.Value),
                        _state, 3, _interactionApi, _channelApi,
                        _guildApi, _logger);

                    if (!result.HasValue)
                    {
                        var failureResult = await _interactionApi
                            .EditOriginalInteractionResponseAsync(
                                _context.ApplicationID,
                                _context.Token,
                                ErrorMessages.HandleReactionFailed);
                        return !failureResult.IsSuccess ? Result.FromError(failureResult) : Result.FromSuccess();
                    }

                    var (members, matchData) = result.Value;
                    var guild = await Utilities.GetGuildFromContext(_context, _interactionApi, _logger);
                    if (!guild.HasValue) return Result.FromSuccess();

                    foreach (var member in members)
                    {
                        var textChannel =
                            await _state.BurstApi.CreatePlayerChannel(guild.Value, joinResult.BotUser, member,
                            _guildApi, _logger);
                        var playerState = new NinetyNinePlayerState
                        {
                            AvatarUrl = member.GetAvatarUrl(),
                            GameId = matchData.GameId ?? "",
                            PlayerId = member.User.Value.ID.Value,
                            PlayerName = member.GetDisplayName(),
                            TextChannel = textChannel,
                            Order = 0
                        };
                        await NinetyNineGame.AddPlayerState(matchData.GameId ?? "", guild.Value, playerState,new NinetyNineInGameRequest
                        {
                            AvatarUrl = playerState.AvatarUrl,
                            ChannelId = playerState.TextChannel!.ID.Value,
                            ClientType = ClientType.Discord,
                            GameId = playerState.GameId,
                            PlayerId = playerState.PlayerId,
                            PlayerName = playerState.PlayerName,
                            RequestType = NinetyNineInGameRequestType.Deal
                        },_state.GameStates.NinetyNineGameStates.Item1,_state.GameStates.NinetyNineGameStates.Item2);

                        _ = Task.Run(() => StartListening(matchData.GameId ?? "", _state,
                            _channelApi, _guildApi,
                            _logger));
                    }
                    break;
                }
            case GenericJoinStatusType.Matched:
                {
                    var matchedMessageResult = await _interactionApi
                        .EditOriginalInteractionResponseAsync(
                        _context.ApplicationID,
                        _context.Token,
                        joinResult.Reply);
                    if (!matchedMessageResult.IsSuccess) return Result.FromError(matchedMessageResult);

                    var followUpResult = await _interactionApi
                        .CreateFollowupMessageAsync(
                        _context.ApplicationID,
                        _context.Token,
                        embeds: new[]
                        {
                            Utilities.BuildGameEmbed(joinResult.InvokingMember, joinResult.BotUser,
                                joinResult.JoinStatus, GameName, "",
                                null)
                        });
                    if (!followUpResult.IsSuccess) return Result.FromError(followUpResult);

                    var guild = await Utilities.GetGuildFromContext(_context, _interactionApi, _logger);
                    if (!guild.HasValue) return Result.FromSuccess();

                    var textChannel = await _state.BurstApi.CreatePlayerChannel(guild.Value, joinResult.BotUser, joinResult.InvokingMember,
                   _guildApi, _logger);

                    var playerState = new NinetyNinePlayerState
                    {
                        AvatarUrl = joinResult.InvokingMember.GetAvatarUrl(),
                        GameId = joinResult.JoinStatus.GameId ?? "",
                        PlayerId = joinResult.InvokingMember.User.Value.ID.Value,
                        PlayerName = joinResult.InvokingMember.GetDisplayName(),
                        TextChannel = textChannel,
                        Order = 0,
                    };
                    await NinetyNineGame.AddPlayerState(joinResult.JoinStatus.GameId ?? "", guild.Value, playerState, new NinetyNineInGameRequest
                    {
                        AvatarUrl = playerState.AvatarUrl,
                        ChannelId = playerState.TextChannel!.ID.Value,
                        ClientType = ClientType.Discord,
                        GameId = playerState.GameId,
                        PlayerId = playerState.PlayerId,
                        PlayerName = playerState.PlayerName,
                        RequestType = NinetyNineInGameRequestType.Deal
                    }, _state.GameStates.NinetyNineGameStates.Item1, _state.GameStates.NinetyNineGameStates.Item2);

                    _ = Task.Run(() =>
                        StartListening(joinResult.JoinStatus.GameId ?? "", _state, _channelApi, _guildApi, _logger));
                    break;
                }
            case GenericJoinStatusType.Waiting:
                {
                    var waitingMessageResult = await _interactionApi
                        .EditOriginalInteractionResponseAsync(
                        _context.ApplicationID,
                        _context.Token,
                        joinResult.Reply);
                    if (!waitingMessageResult.IsSuccess) return Result.FromError(waitingMessageResult);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var waitingResult = await _state.BurstApi.WaitForGame<NinetyNinePlayerState>(
                                joinResult.JoinStatus, _context, joinResult.MentionedPlayers,
                                joinResult.BotUser, "", _interactionApi, _guildApi, _logger);

                            if (!waitingResult.HasValue)
                                throw new Exception($"Failed to get waiting result for {GameName}.");
                            var guild = await Utilities.GetGuildFromContext(_context, _interactionApi, _logger);
                            if (!guild.HasValue) return;

                            var (matchData, playerStates) = waitingResult.Value;
                            foreach (var player in playerStates)
                            {
                                await NinetyNineGame.AddPlayerState(matchData.GameId ?? "", guild.Value, player,new NinetyNineInGameRequest
                                {
                                    AvatarUrl = player.AvatarUrl,
                                    ChannelId = player.TextChannel!.ID.Value,
                                    ClientType = ClientType.Discord,
                                    GameId = player.GameId,
                                    PlayerId = player.PlayerId,
                                    PlayerName = player.PlayerName,
                                    RequestType = NinetyNineInGameRequestType.Deal
                                },_state.GameStates.NinetyNineGameStates.Item1,_state.GameStates.NinetyNineGameStates.Item2);

                                await Task.Delay(TimeSpan.FromSeconds(1));

                                _ = Task.Run(() => StartListening(matchData.GameId ?? "", _state,
                                    _channelApi,
                                    _guildApi,
                                    _logger));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("WebSocket failed: {Exception}", ex);
                        }
                });
                    break;
                }
        }
        return Result.FromSuccess();
        //var mentionedPlayers = Game
        //    .BuildPlayerList(_context, users)
        //    .ToImmutableArray();

        //var result = await _interactionApi
        //    .EditOriginalInteractionResponseAsync(_context.ApplicationID, _context.Token,
        //        $"Difficulty: {difficulty}\nVariation: {variation}\nInvited players: {string.Join(' ', mentionedPlayers.Select(p => $"<@!{p}>"))}");

        //return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
    }
}