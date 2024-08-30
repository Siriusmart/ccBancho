using MCGalaxy;

public sealed class FriendEntry : Command {
    public override string name {
        get { return "friend"; }
    }
    public override string shortcut {
        get { return "f"; }
    }
    public override string type {
        get { return "social"; }
    }

    public static Type? GetSubcommand(string command) {
        switch (command) {
        case "add":
            return typeof(FriendAdd);
        default:
            return null;
        }
    }

    public override void Use(Player p, string message) {
        if (message == string.Empty) {
            p.MessageLines(Formatter
                               .FriendBarsWrap(@$"&aFriend Commands:
&e{Subcommand.FormatText("friend",FriendAdd.Name(), FriendAdd.Format())} &7- &b{FriendAdd.Description()}")
                               .Split("\n"));
            return;
        }

        string[] args = message.SplitSpaces();
        Type? command = GetSubcommand(args[0]);
        object[] parameters = { p, args[1..] };

        if (command == null) {
            parameters[1] = args;
            command = typeof(FriendAdd);
        }

        bool res = (bool)command.GetMethod("Run").Invoke(null, parameters);

        if (!res) {
            p.Message(
                $"&cIncorrect usage: {Subcommand.FormatText("friend",(string)command.GetMethod("Name").Invoke(null, null),(string?)command.GetMethod("Format").Invoke(null, null))}");
        }
    }

    public override void Help(Player p) {
        p.Message("&e/friend <args..> &7- &bManage friends.");
    }
}
