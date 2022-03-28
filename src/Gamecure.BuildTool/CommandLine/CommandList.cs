using Gamecure.Core.Common.Logging;

namespace Gamecure.BuildTool.CommandLine;

public record CommandResult(bool Succeeded, string Reason);

public class CommandList<TResult> where TResult : class, new()
{
    private readonly string _executable;
    private readonly List<ICommand> _commands = new();

    public CommandList(string executable)
    {
        _executable = executable;
    }
    public CommandList<TResult> WithCommand(ICommand command)
    {
        if (_commands.Any(c => c.Name == command.Name))
        {
            throw new InvalidOperationException($"Multiple definitions of command {command.Name}");
        }
        _commands.Add(command);
        return this;
    }

    public async Task<TResult?> Execute(string[] args)
    {
        if (args.Contains("--help"))
        {
            Print();
            return new TResult();
        }

        if (args.Length == 0)
        {
            Logger.Error("No command specified.");
            return null;
        }

        var command = _commands.SingleOrDefault(c => c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
        if (command == null)
        {
            Logger.Error($"Command {args[0]} does not exist.");
            return null;
        }

        return await command.Execute<TResult>(args.Skip(1).ToArray());
    }

    public void Print()
    {
        Console.WriteLine($"Usage: {_executable} [command] [options]\n");
        Console.WriteLine("Gamecure Build Tool - Creates a config and builds the executables.\n");


        Console.WriteLine("Command: help");
        Console.WriteLine($"Usage:\n {_executable} --help\n");

        foreach (var command in _commands)
        {
            command.Print(_executable);
        }
    }
}
