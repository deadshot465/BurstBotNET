using BurstBotShared.Shared.Models.Data;

namespace BurstBotNET.Handlers;

#pragma warning disable CA2252
public partial class Handlers
{
    private readonly Commands.Commands _commands;
    private readonly State _state;

    public Handlers(Commands.Commands commands, State state)
    {
        _commands = commands;
        _state = state;
    }
}