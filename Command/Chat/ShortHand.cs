using MCGalaxy;

public class ChatGlobal : Command {
    public override string name {
        get { return "ac"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        OnlinePlayers.Message(p, ChatChannel.global, message);
    }

    public override void Help(Player p) {
        p.Message("&e/ac [message] &7- &bSend a message to the global chat.");
    }
}

public class ChatLocal : Command {
    public override string name {
        get { return "lc"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        OnlinePlayers.Message(p, ChatChannel.local, message);
    }

    public override void Help(Player p) {
        p.Message("&e/lc [message] &7- &bSend a message to the global chat.");
    }
}

public class ChatParty : Command {
    public override string name {
        get { return "pc"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        OnlinePlayers.Message(p, ChatChannel.party, message);
    }

    public override void Help(Player p) {
        p.Message("&e/pc [message] &7- &bSend a message to the global chat.");
    }
}
