using MCGalaxy;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

public class OnlinePlayers {
    private static Dictionary<Player, OnlinePlayer> players =
        new Dictionary<Player, OnlinePlayer>();

    public static void PlayerConnect(Player p) {
        players.Add(p, new OnlinePlayer(p));
    }

    public static void PlayerDisconnect(Player p, string reason) {
        players[p].Logout();
        players.Remove(p);
    }

    public static void Init() {
        foreach (Player p in PlayerInfo.Online.Items) {
            players.Add(p, new OnlinePlayer(p));
        }
    }

    public static void Exit() {
        foreach (OnlinePlayer p in players.Values) {
            p.Logout();
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

    public static OnlinePlayer? GetPlayer(Player p) {
        if (!players.ContainsKey(p))
            return null;
        return players[p];
    }

    public static void ReplacePlayer(Player p) {
        foreach (OnlinePlayer player in players.Values) {
            player.ReplacePlayer(p);
        }
    }

    public static OnlinePlayer? FindOnline(string name) {
        foreach (Player p in PlayerInfo.Online.Items) {
            if (p.name.CaselessEq(name))
                return GetPlayer(p);
        }

        return null;
    }

    public static OnlinePlayer? FindOnlineExact(string name) {
        foreach (Player p in PlayerInfo.Online.Items) {
            if (p.name == name)
                return GetPlayer(p);
        }

        return null;
    }

    public static OnlinePlayer? Find(string name) {
        foreach (Player p in PlayerInfo.Online.Items) {
            if (p.name.CaselessEq(name))
                return GetPlayer(p);
        }

        string regexFilter = Regex.Escape(name);
        BsonRegularExpression regex =
            new BsonRegularExpression(regexFilter, "i");
        ;

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Regex("_id", regex);
        BsonDocument? doc = Bancho.BanchoPlayers.Find(filter).FirstOrDefault();

        if (doc == null)
            return null;

        Player player = new Player((string)doc["_id"]);
        if (doc.Contains("colour"))
            player.color = (string)doc["colour"];
        List<Friend> friends =
            doc.Contains("friends")
                ? doc["friends"]
                      .AsBsonArray
                      .Select(friend => new Friend(friend.ToBsonDocument()))
                      .ToList()
                : new List<Friend>();

        return new OnlinePlayer(player, friends);
    }

    public static OnlinePlayer? FindExact(string name) {
        foreach (Player p in PlayerInfo.Online.Items) {
            if (p.name == name) {
                return GetPlayer(p) ??
                       new OnlinePlayer(new Player(name), new List<Friend>());
            }
        }

        string regexFilter = Regex.Escape(name);
        BsonRegularExpression regex =
            new BsonRegularExpression(regexFilter, "i");
        ;

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Eq("_id", name);
        BsonDocument? doc = Bancho.BanchoPlayers.Find(filter).FirstOrDefault();

        if (doc == null)
            return null;

        Player player = new Player((string)doc["_id"]);
        if (doc.Contains("colour"))
            player.color = (string)doc["colour"];
        List<Friend> friends =
            doc.Contains("friends")
                ? doc["friends"]
                      .AsBsonArray
                      .Select(friend => new Friend(friend.ToBsonDocument()))
                      .ToList()
                : new List<Friend>();

        return new OnlinePlayer(player, friends);
    }
}
