using System.Collections.Immutable;
using System.ComponentModel;
using BurstBotShared.Shared;
using BurstBotShared.Shared.Extensions;
using BurstBotShared.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Utilities = BurstBotShared.Shared.Utilities.Utilities;

namespace BurstBotNET.Commands;

public class About : CommandGroup, ISlashCommand
{
    private const string AboutTextPath = "Assets/localization/bot/about.txt";
    private static readonly Lazy<string> AboutText = new(() => File.ReadAllText(AboutTextPath));
    
    private readonly InteractionContext _context;
    private readonly IDiscordRestUserAPI _userApi;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly ILogger<About> _logger;

    public About(
        InteractionContext context,
        IDiscordRestUserAPI userApi,
        IDiscordRestInteractionAPI interactionApi,
        ILogger<About> logger)
    {
        _context = context;
        _userApi = userApi;
        _interactionApi = interactionApi;
        _logger = logger;
    }

    public static string Name => "about";

    public static string Description => "Show information about All Burst bot.";

    public static ImmutableArray<IApplicationCommandOption> ApplicationCommandOptions => ImmutableArray<IApplicationCommandOption>.Empty;

    public static Tuple<string, string, ImmutableArray<IApplicationCommandOption>> GetCommandTuple()
    {
        return new Tuple<string, string, ImmutableArray<IApplicationCommandOption>>(
            Name, Description, ApplicationCommandOptions);
    }

    [Command("about")]
    [Description("Show information about All Burst bot.")]
    public async Task<IResult> Handle()
    {
        var bot = await Utilities.GetBotUser(_userApi, _logger);
        
        if (bot == null) return Result.FromSuccess();

        var result = await _interactionApi
            .EditOriginalInteractionResponseAsync(
                _context.ApplicationID,
                _context.Token,
                embeds: new[]
                {
                    new Embed(
                        Author: new EmbedAuthor("Jack of All Trades", IconUrl: bot.GetAvatarUrl()),
                        Colour: BurstColor.Burst.ToColor(),
                        Thumbnail: new EmbedThumbnail(Constants.BurstLogo),
                        Description: AboutText.Value,
                        Footer: new EmbedFooter("All Burst: Development 1.2 | 2021-12-31")
                    )
                });

        return result.IsSuccess ? Result.FromSuccess() : Result.FromError(result);
    }

    public override string ToString() => "about";
}