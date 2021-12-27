using System.Globalization;
using BurstBotShared.Shared.Interfaces;
using BurstBotShared.Shared.Models.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BurstBotNET.Commands.ChinesePoker;

using CommandGroup = Dictionary<string, Func<DiscordClient, InteractionCreateEventArgs, State, Task>>;

#pragma warning disable CA2252
public partial class ChinesePoker : ISlashCommand
{
    public const string GameName = "Chinese Poker";
    private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
    private readonly CommandGroup _dispatchables;

    static ChinesePoker()
    {
        AvailableRanks = Enumerable
            .Range(2, 9)
            .Select(n => n.ToString())
            .Concat(new[] { "a", "j", "q", "k" })
            .ToArray();
    }

    public ChinesePoker()
    {
        Command = new DiscordApplicationCommand("chinese_poker", "Play a Chinese poker-like game with other 3 people.",
            new[]
            {
                new DiscordApplicationCommandOption("join",
                    "Request to be enqueued to the waiting list to match with other players.",
                    ApplicationCommandOptionType.SubCommand, options: new[]
                    {
                        new DiscordApplicationCommandOption("base_bet",
                            "The base bet. Each player's final reward will be units won/lost multiplied by this.",
                            ApplicationCommandOptionType.Number, true),
                        new DiscordApplicationCommandOption("player2", "(Optional) The 2nd player you want to invite.",
                            ApplicationCommandOptionType.User, false),
                        new DiscordApplicationCommandOption("player3", "(Optional) The 3rd player you want to invite.",
                            ApplicationCommandOptionType.User, false),
                        new DiscordApplicationCommandOption("player4", "(Optional) The 4th player you want to invite.",
                            ApplicationCommandOptionType.User, false)
                    })
            });

        _dispatchables = new CommandGroup
        {
            { "join", Join }
        };
    }

    public DiscordApplicationCommand Command { get; init; }

    public async Task Handle(DiscordClient client, InteractionCreateEventArgs e, State state)
    {
        await _dispatchables[e.Interaction.Data.Options.ElementAt(0).Name].Invoke(client, e, state);
    }

    public override string ToString()
    {
        return "chinese_poker";
    }
}