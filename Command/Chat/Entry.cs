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
&e/chat [player] &7- &bSwitch to private chat with a player
&e/msg [player] [message] &7- &bSend a private message to a player
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
                OnlinePlayer target = OnlinePlayers.FindOnline(args[0]);
                if (target == null) {
                    p.MessageLines(
                        Formatter
                            .BarsWrap("&cThat player is currently not online.")
                            .Split('\n'));
                    return;
                }
                if (target.player == p) {
                    p.MessageLines(
                        Formatter.BarsWrap("&cYou cannot chat with yourself.")
                            .Split('\n'));
                    return;
                }
                OnlinePlayer source = OnlinePlayers.GetPlayer(p);
                source.chatPlayer = target.player;
                source.channel = ChatChannel.player;
                p.MessageLines(
                    Formatter
                        .BarsWrap(
                            $"&eYou are now chatting with {target.player.ColoredName}")
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
