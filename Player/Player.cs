using MCGalaxy;

public enum ChatChannel {
    global,
    party,
    local,
}

public class OnlinePlayer {
    public ChatChannel channel = ChatChannel.local;

    public Player player;
    public OnlinePlayer(Player p) { player = p; }

    public ChatChannel Channel {
        get { return channel; }
    }

    public bool SwitchChannel(ChatChannel newChannel) {
        bool changed = newChannel != channel;
        channel = newChannel;
        return changed;
    }

    public void Message(ChatChannel channel, string message) {
        switch (channel) {
        case ChatChannel.global:
            Chat.MessageGlobal($"&6[GLOBAL] {player.ColoredName}&f: {message}");
            break;
        case ChatChannel.local:
            Chat.MessageFromLevel(player, $"{player.ColoredName}&f: {message}");
            break;
        case ChatChannel.party:
            Party party = Parties.GetParty(player);

            if (party == null) {
                player.MessageLines(
                    Formatter.BarsWrap("&cYou are not in a party!")
                        .Split('\n'));
                return;
            }

            party.Message(player, message);
            break;
        }
    }

    public void Message(string message) {
        switch (Channel) {
        case ChatChannel.global:
            Chat.MessageGlobal($"&6[GLOBAL] {player.ColoredName}&f: {message}");
            break;
        case ChatChannel.local:
            Chat.MessageFromLevel(player, $"{player.ColoredName}&f: {message}");
            break;
        case ChatChannel.party:
            Party party = Parties.GetParty(player);

            if (party == null) {
                channel = ChatChannel.local;
                player.MessageLines(
                    Formatter
                        .BarsWrap(
                            $"&cSince you are not in a party, you have been moved to the local chat.")
                        .Split('\n'));
                return;
            }

            party.Message(player, message);
            break;
        }
    }
}
