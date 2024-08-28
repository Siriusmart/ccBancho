using MCGalaxy;
using System.Collections.Generic;

public class OnlinePlayers {
    private static Dictionary<Player, OnlinePlayer> players =
        new Dictionary<Player, OnlinePlayer>();

    public static void PlayerConnect(Player p) {
        players.Add(p, new OnlinePlayer(p));
    }

    public static void PlayerDisconnect(Player p, string reason) {
        players.Remove(p);
    }

    public static void Init() {
        foreach (Player p in PlayerInfo.Online.Items) {
            players.Add(p, new OnlinePlayer(p));
        }
    }

    public static void Message(Player p, string content) {
        p.cancelchat = true;
        players[p].Message(content);
    }

    public static void Message(Player p, ChatChannel channel, string content) {
        p.cancelchat = true;
        players[p].Message(channel, content);
    }

    public static bool SwitchChannel(Player p, ChatChannel channel) {
        return players[p].SwitchChannel(channel);
    }
}
