using MCGalaxy;
using MCGalaxy.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

public enum ChatChannel {
    global,
    party,
    player,
    local,
}

public class OnlinePlayer {
    private long joinTime;
    public ChatChannel channel = ChatChannel.local;
    public Player chatPlayer = null;
    public Player lastMessagedBy = null;
    public long lastMessagedTime = 0;

    public Player player;

    public OnlinePlayer(Player p, List<Friend> friends) {
        player = p;
        this.friends = friends;
    }

    public OnlinePlayer(Player p) {
        Parties.ReplacePlayer(p);
        OnlinePlayers.ReplacePlayer(p);
        joinTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        player = p;
        party = Parties.GetPartyInit(p);

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
            playerInfo["colour"] = p.color;
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

    public bool MessagePlayer(OnlinePlayer target, string content) {
        if (target == null) {
            player.Message($"&cThat player isn't online currently!");
            return false;
        }
        if (target.player == player) {
            player.Message($"&cYou cannot message yourself!");
            return false;
        }
        target.lastMessagedBy = player;
        target.lastMessagedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        player.Message($"&dTo {target.player.ColoredName}&7: {content}");
        target.player.Message($"&dFrom {player.ColoredName}&7: {content}");
        return true;
    }

    public void Reply(string content) {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now - lastMessagedTime > Bancho.Config.MessageReplyTimeOut) {
            player.Message(
                $"&cYou have not been messaged by anyone in the past {Formatter.Duration(Bancho.Config.MessageReplyTimeOut, 2)}");
            return;
        }
        MessagePlayer(OnlinePlayers.GetPlayer(lastMessagedBy), content);
    }

    public void ReplacePlayer(Player p) {
        for (int i = 0; i < friends.Count(); i++) {
            friends[i].ReplacePlayer(p);
        }

        if (chatPlayer != null && chatPlayer.name == p.name) {
            chatPlayer = p;
        }

        if (lastMessagedBy != null && lastMessagedBy.name == p.name) {
            lastMessagedBy = p;
        }
    }

    /// -2: currently online
    /// -1: not known
    /// anything else: timestamp
    public static long LastSeen(Player p) {
        if (PlayerInfo.Online.Contains(p))
            return -2;

        BsonDocument playerInfo =
            Bancho.BanchoPlayers.Find(item => item["_id"] == p.name)
                .FirstOrDefault();

        if (playerInfo == null || !playerInfo.Contains("lastSeen"))
            return -1;

        return (long)playerInfo["lastSeen"];
    }

    public void Logout() {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (party != null) {
            party.RegisterLogout();
        }

        foreach (Player friendRequested in recievedRequests.Keys) {
            friendRequested.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&eThe friend request to {player.ColoredName} &ehas expired because the player is now offline.")
                    .Split('\n'));
        }
        recievedRequests.Clear();

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Eq("_id", player.name);
        UpdateDefinition<BsonDocument> update =
            Builders<BsonDocument>.Update.Set("lastSeen", now).Inc("playTime", now - joinTime).Set("friends", friends.Select(friend => friend.asBson())).Set("colour",  player.color);
        UpdateResult? res = Bancho.BanchoPlayers.UpdateOne(filter, update);
    }

    public Party? party;

    public Dictionary<Player, long> recievedRequests =
        new Dictionary<Player, long>();
    private List<Friend> friends = new List<Friend>();

    public List<Friend> Friends {
        get { return friends; }
    }

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

        if (recievedRequests.ContainsKey(p)) {
            recievedRequests.Remove(p);

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
    public long FriendRequestCooldownRemaining(Player fromPlayer) {
        if (fromPlayer == null)
            return -1;
        if (recievedRequests.ContainsKey(fromPlayer))
            return Bancho.Config.FriendCooldown -
                   (DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                    recievedRequests[fromPlayer]);
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
        return friends.Any(friend => friend.UnderlyingPlayer.name == p.name);
    }

    public void Unfriend(Player p) {
        for (int i = 0; i < friends.Count(); i++) {
            if (friends[i].player.name == p.name) {
                friends.RemoveAt(i);
                return;
            }
        }
    }

    public static void UnfriendSave(Player target, Player p) {
        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.Eq("_id", target.name);
        UpdateDefinition<BsonDocument> update =
            Builders<BsonDocument>.Update.PullFilter(
                "friends", Builders<BsonDocument>.Filter.Eq("name", p.name));
        UpdateResult? res = Bancho.BanchoPlayers.UpdateOne(filter, update);
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
            Party party = Parties.GetParty(this);

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
            Party party = Parties.GetParty(this);

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
        case ChatChannel.player:
            if (!PlayerInfo.Online.Contains(chatPlayer)) {
                player.MessageLines(
                    Formatter
                        .BarsWrap(
                            "&cThat player isn't online currently.")
                        .Split('\n'));
                return;
            }
            MessagePlayer(OnlinePlayers.GetPlayer(chatPlayer), message);
            break;
        }
    }
}
