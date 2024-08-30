using MCGalaxy;
using MongoDB.Bson;

public class Friend {
    private Player player;
    private long relationBegin;

    public BsonValue asBson() {
        BsonValue output = new BsonDocument();
        output["name"] = player.name;
        output["relationBegin"] = relationBegin;
        return output;
    }

    public Friend(BsonDocument doc) {
        player = PlayerInfo.FindExact((string)doc["name"]);
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
}
