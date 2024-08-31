using MCGalaxy;

public class ChatMsg : Command {
    public override string name {
        get { return "msg"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        string[] args = message.SplitSpaces();
        if (args.Length < 2) {
            p.Message("&cIncorrect usage /msg [player] [message]");
            return;
        }
        OnlinePlayer target = OnlinePlayers.FindOnline(args[0]);
        OnlinePlayer sender = OnlinePlayers.GetPlayer(p);

        sender.MessagePlayer(target, args[1..].Join(" "));
    }

    public override void Help(Player p) {
        p.Message("&e/msg [player] [message] &7- &bSend a private message to " +
                  "a player.");
    }
}

public class ChatReply : Command {
    public override string name {
        get { return "reply"; }
    }
    public override string shortcut {
        get { return "r"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        if (message == string.Empty) {
            p.Message("&cIncorrect usage /reply [message]");
            return;
        }
        OnlinePlayer sender = OnlinePlayers.GetPlayer(p);

        sender.Reply(message);
    }

    public override void Help(Player p) {
        p.Message(
            "&e/reply [message] &7- &bReply to the previous private message.");
    }
}
