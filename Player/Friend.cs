using MCGalaxy;
using MongoDB.Bson;

public class Friend {
    public volatile Player player;
    public long relationBegin;

    public BsonDocument asBson() {
        BsonDocument output = new BsonDocument();
        output["name"] = player.name;
        output["relationBegin"] = relationBegin;
        return output;
    }

    public Friend(BsonDocument doc) {
        string name = (string)doc["name"];
        OnlinePlayer exact = OnlinePlayers.FindExact(name);
        player = exact == null ? new Player(name) : exact.player;
        relationBegin = (long)doc.GetValue(
            "relationBegin", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    public Friend(Player p, long begin) {
        player = p;
        relationBegin = begin;
    }

    public static (Friend, Friend) FriendPair(Player one, Player two) {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (new Friend(one, now), new Friend(two, now));
    }

    public Player UnderlyingPlayer {
        get { return player; }
    }

    public void ReplacePlayer(Player p) {
        if (p.name == player.name)
            player = p;
    }
}
