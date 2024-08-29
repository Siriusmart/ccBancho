using MCGalaxy;
using MongoDB.Bson;
using MongoDB.Driver;

public enum ChatChannel {
    global,
    party,
    local,
}

public class OnlinePlayer {
    private long joinTime;
    public ChatChannel channel = ChatChannel.local;

    public Player player;
    public OnlinePlayer(Player p) {
        joinTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        player = p;
        party = Parties.GetParty(p);

        BsonDocument playerInfo =
            Bancho.BanchoPlayers.Find(item => item["_id"] == p.name)
                .FirstOrDefault();

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Eq("_id", p.name);
        UpdateDefinition<BsonDocument> update =
            Builders<BsonDocument>.Update.Set("lastSeen", joinTime);
        BsonDocument? saved =
            Bancho.BanchoPlayers.FindOneAndUpdate(filter, update);

        if (saved == null) {
            playerInfo = new BsonDocument();
            playerInfo["_id"] = p.name;
            playerInfo["lastSeen"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            playerInfo["playTime"] = 0;
            playerInfo["friends"] = new BsonArray();
            Bancho.BanchoPlayers.InsertOne(playerInfo);
        } else {
            if (saved.Contains("friends")) {
                friends = saved["friends"]
                              .AsBsonArray
                              .Select(item => new Friend(item.ToBsonDocument()))
                              .ToList();
            }
        }
    }

    public void Logout() {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Eq("_id", player.name);
        UpdateDefinition<BsonDocument> update =
            Builders<BsonDocument>.Update.Set("lastSeen", now).Inc("playTime", now - joinTime).Set("friends", friends);
        UpdateResult? res = Bancho.BanchoPlayers.UpdateOne(filter, update);
    }

    public Party? party;

    private List<Player> recievedRequests = new List<Player>();
    private List<Friend> friends = new List<Friend>();

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
