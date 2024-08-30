using MCGalaxy;
using MCGalaxy.Tasks;
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

    private Player player;

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

    public Dictionary<Player, long> recievedRequests =
        new Dictionary<Player, long>();
    private List<Friend> friends = new List<Friend>();

    /// 0: player offline
    /// 1: request sent
    /// 2: you are now friends with player
    /// 3: not sent, you are already friends with that player
    public int FriendAdd(Player p) {
        if (!PlayerInfo.Online.Contains(p))
            return 0;

        if (HasFriend(p))
            return 3;

        OnlinePlayer target = OnlinePlayers.GetPlayer(p);

        if (target.recievedRequests.ContainsKey(player)) {
            target.recievedRequests.Remove(player);

            (Friend, Friend) pair = Friend.FriendPair(player, p);
            target.friends.Add(pair.Item1);
            friends.Add(pair.Item2);
            return 2;
        }

        target.recievedRequests.Add(player,
                                    DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        Server.MainScheduler.QueueOnce(
            RemoveRequest, target,
            new TimeSpan(0, 0, Bancho.Config.FriendCooldown));
        return 1;
    }

    /// less than 0: no cooldown
    /// anything else: seconds
    public long FriendRequestCooldownRemaining(Player p) {
        if (p == null)
            return -1;
        if (recievedRequests.ContainsKey(p))
            return Bancho.Config.FriendCooldown -
                   (DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                    recievedRequests[p]);
        return -1;
    }

    /// { requester }
    private void RemoveRequest(SchedulerTask task) {
        OnlinePlayer target = (OnlinePlayer)task.State;

        if (!target.recievedRequests.ContainsKey(player))
            return;

        target.recievedRequests.Remove(player);

        player.MessageLines(
            Formatter
                .BarsWrap(
                    $"&eFriend request to {target.player.ColoredName} &ehas expired.")
                .Split('\n'));
        target.player.MessageLines(
            Formatter
                .BarsWrap(
                    $"&eFriend request from {player.ColoredName} &ehas expired.")
                .Split('\n'));
    }

    public bool HasFriend(Player p) {
        return friends.Any(friend => friend.UnderlyingPlayer == p);
    }

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
