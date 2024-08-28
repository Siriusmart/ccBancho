using MCGalaxy;

public class HelpEntry : Command {
    public override string name {
        get { return "help"; }
    }
    public override string type {
        get { return "info"; }
    }

    public override void Use(Player p, string message) {
        string[] args = message.SplitSpaces();

        if (message == string.Empty) {
            p.MessageLines(Formatter
                               .BarsWrap(@$"&aBancho Commands:
&e/chat <args..> &7- &bManage player chat settings
&e/party <args..> &7- &bManage game parties")
                               .Split("\n"));
            return;
        }

        p.Message($"&cIncorrect usage: /help");
    }

    public override void Help(Player p) {
        p.Message("&e/chat <args..> &7- &bManage player chat settings.");
    }
}
