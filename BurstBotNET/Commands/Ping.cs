using BurstBotNET.Shared.Interfaces;
using BurstBotNET.Shared.Models.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BurstBotNET.Commands;

public class Ping : ISlashCommand
{
    public DiscordApplicationCommand Command { get; init; }

    public Ping()
    {
        Command = new DiscordApplicationCommand("ping", "Returns the latency between the bot and Discord API.");
    }

    public async Task Handle(DiscordClient client, InteractionCreateEventArgs e,
        State state)
    {
        var startTime = DateTime.Now;
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Pinging..."));
        var latency = DateTime.Now - startTime;
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Pong!!\nLatency is {latency.Milliseconds} ms"));
    }
}