using MCGalaxy;

public sealed class ChatEntry : Command {
    public override string name {
        get { return "chat"; }
    }
    public override string type {
        get { return "social"; }
    }

    public override void Use(Player p, string message) {
        string[] args = message.SplitSpaces();

        if (message == string.Empty) {
            p.MessageLines(Formatter
                               .BarsWrap(@$"&aChat Commands:
&e/chat global &7- &bSwitch to global chat
&e/chat local &7- &bSwitch to local chat
&e/chat party &7- &bSwitch to party chat
&e/ac [message] &7- &bSend a message in the global chat
&e/lc [message] &7- &bSend a message in the local chat
&e/pc [message] &7- &bSend a message in the party chat")
                               .Split("\n"));
            return;
        }

        if (args.Count() == 1) {
            bool changed;
            switch (args[0]) {
            case "global":
                changed = OnlinePlayers.SwitchChannel(p, ChatChannel.global);
                break;
            case "local":
                changed = OnlinePlayers.SwitchChannel(p, ChatChannel.local);
                break;
            case "party":
                if (Parties.GetParty(p) == null) {
                    p.MessageLines(
                        Formatter.BarsWrap("&cYou are not in a party!")
                            .Split('\n'));
                    return;
                }
                changed = OnlinePlayers.SwitchChannel(p, ChatChannel.party);
                break;
            default:
                p.MessageLines(Formatter.BarsWrap("&cChannel does not exist.")
                                   .Split('\n'));
                return;
            }

            if (changed) {
                p.MessageLines(
                    Formatter
                        .BarsWrap(
                            $"&aYou are now in the {args[0].ToUpper()} chat.")
                        .Split('\n'));
            } else {
                p.MessageLines(
                    Formatter.BarsWrap("&eYou are already in this chat.")
                        .Split('\n'));
            }
            return;
        }

        p.Message($"&cIncorrect usage: /chat [channel]");
    }

    public override void Help(Player p) {
        p.Message("&e/chat <args..> &7- &bManage player chat settings.");
    }
}
