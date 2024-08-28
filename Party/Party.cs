using MCGalaxy;
using MCGalaxy.Tasks;
using System.Linq;

public class Party {
    private Dictionary<Player, long> invited = new Dictionary<Player, long>();
    private Dictionary<Player, long> requested = new Dictionary<Player, long>();
    private HashSet<Player> members = new HashSet<Player>();
    private HashSet<Player> moderators = new HashSet<Player>();
    private List<Player> joinOrder = new List<Player>();
    private Player leader;

    public void Message(Player p, string content) {
        content = $"&9Party &8> &f{p.name}: {content}";

        foreach(Player recepient in joinOrder) {
            recepient.Message(content);
        }
    }

    public static int InviteCooldown {
        get { return 60; }
    }

    public Player[] Members {
        get {
            Player[] list = members.ToArray();
            Array.Sort(list);
            return list;
        }
    }

    public Player[] Moderators {
        get {
            Player[] list = moderators.ToArray();
            Array.Sort(list);
            return list;
        }
    }

    public Player Leader {
        get { return leader; }
    }

    private Dictionary<string, bool> settings = new Dictionary<string, bool>();

    public bool IsEmpty() { return FlatList().Length == 1; }

    public Party(Player p) {
        leader = p;
        joinOrder.Add(p);
        settings.Add("allInvite", false);
        settings.Add("mute", false);
        settings.Add("allowRequest", true);
    }

    public bool Contains(Player p) {
        return members.Contains(p) || moderators.Contains(p) || leader == p;
    }

    /// less than 0: no cooldown
    /// anything else: seconds
    public long InvitedCooldownRemaining(Player p) {
        if (invited.ContainsKey(p))
            return InviteCooldown -
                   (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - invited[p]);

        return -1;
    }

    /// less than 0: no cooldown
    /// anything else: seconds
    public long RequestedCooldownRemaining(Player p) {
        if (requested.ContainsKey(p))
            return InviteCooldown -
                   (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - requested[p]);

        return -1;
    }

    /// 0: invite sent
    /// 1: player has requested to join, now in party
    /// 2: player offline
    /// 3: player already in said party
    public ushort Invite(Player? target, Player inviter) {
        if (!PlayerInfo.Online.Contains(target)) {
            return 2;
        }

        if (Contains(target))
            return 3;

        if (requested.Remove(target)) {
            members.Add(target);
            joinOrder.Add(target);
            return 1;
        } else {
            invited.Add(target, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Player[] parameters = { target, inviter };
            Server.MainScheduler.QueueOnce(RemoveInvite, parameters,
                                           new TimeSpan(0, 0, InviteCooldown));
            return 0;
        }
    }

    private void RemoveInvite(SchedulerTask task) {
        Player[] players = (Player[])task.State;

        if (!invited.ContainsKey(players[0]))
            return;

        invited.Remove((Player)players[0]);

        players[0].MessageLines(
            Formatter
                .BarsWrap(
                    $"&eParty invite from {players[1].ColoredName} &ehas expired.")
                .Split('\n'));
        players[1].MessageLines(
            Formatter
                .BarsWrap(
                    $"&eParty invite to {players[0].ColoredName} &ehas expired.")
                .Split('\n'));
    }

    /// 0: allowRequest is false
    /// 1: requested
    /// 2: already invited, now in party
    /// 3: player already in said party
    public ushort Request(Player target, Player requester) {
        if (Contains(requester))
            return 3;

        if (invited.Remove(requester)) {
            members.Add(requester);
            joinOrder.Add(requester);
            return 2;
        }

        if (!settings["allowRequest"])
            return 0;

        requested.Add(requester, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        Player[] parameters = { target, requester };
        Server.MainScheduler.QueueOnce(RemoveRequest, parameters,
                                       new TimeSpan(0, 0, InviteCooldown));
        return 1;
    }

    private void RemoveRequest(SchedulerTask task) {
        Player[] players = (Player[])task.State;

        if (!requested.ContainsKey(players[0]))
            return;

        requested.Remove((Player)players[0]);

        players[0].MessageLines(
            Formatter
                .BarsWrap(
                    $"&eParty request from {players[1].ColoredName} &ehas expired.")
                .Split('\n'));
        players[1].MessageLines(
            Formatter
                .BarsWrap(
                    $"&eParty request to {players[0].ColoredName} &ehas expired.")
                .Split('\n'));
    }

    /// 0: not in party
    /// 1: member
    /// 2: moderator
    /// 3: leader
    public ushort Permission(Player p) {
        if (members.Contains(p))
            return 1;
        if (moderators.Contains(p))
            return 2;
        if (leader == p)
            return 3;
        return 0;
    }

    public bool IsOwner(Player p) { return p == leader; }

    /// does user1 has higher permission than user2
    public bool IsHigher(Player one, Player two) {
        return Permission(one) > Permission(two);
    }

    public bool CanInvite(Player p) {
        return Permission(p) > 1 || settings["allInvite"];
    }

    /// 0: removed
    /// 1: removed, party is empty and disbanded
    /// 2: removed leader, auto promotion
    /// 3: not in party
    public ushort Remove(Player p) {
        if (!joinOrder.Remove(p))
            return 3;

        if (moderators.Remove(p) || members.Remove(p))
            return 0;

        if (joinOrder.Count() == 0) {
            Parties.Remove(this);
            return 1;
        }

        if (p == leader) {
            p.Message("got here");
            Promote(joinOrder[0]);
            Promote(joinOrder[0]);
            return 2;
        }

        throw new Exception("unreachable");
    }

    /// 0: not in party
    /// 1: is already leader
    /// 2: promoted (member to mod)
    /// 3: promoted (mod to leader, you are now mod)
    public int Promote(Player p) {
        if (!Contains(p))
            return 0;
        if (p == leader)
            return 1;

        if (members.Remove(p)) {
            moderators.Add(p);
            return 2;
        }

        if (moderators.Remove(p)) {
            moderators.Add(leader);
            leader = p;
            return 3;
        }

        throw new Exception("unreachable");
    }

    /// 0: not in party
    /// 1: is leader
    /// 2: is member
    /// 3: deomoted (mod to member)
    public int Demote(Player p) {
        if (!Contains(p))
            return 0;
        if (p == leader)
            return 1;

        if (members.Contains(p)) {
            return 2;
        }

        if (moderators.Remove(p)) {
            members.Add(leader);
            return 3;
        }

        throw new Exception("unreachable");
    }

    public bool Transfer(Player p) {
        if (Remove(p) != 3) {
            moderators.Add(leader);
            leader = p;
            return true;
        } else {
            return false;
        }
    }

    public bool IsAllInvite() { return settings["allInvite"]; }

    public bool IsAllowRequest() { return settings["allowRequest"]; }

    public bool IsMute() { return settings["mute"]; }

    public bool? GetSetting(string name) {
        if (!settings.ContainsKey(name)) {
            return null;
        }

        return settings[name];
    }

    public bool? ToggleSetting(string name) {
        if (!settings.ContainsKey(name)) {
            return null;
        }

        settings[name] = !settings[name];

        return settings[name];
    }

    public (Player[], Player[], Player) List() {
        return (members.ToArray(), moderators.ToArray(), leader);
    }

    public Player[] FlatList() {
        return members.Concat(moderators).Append(leader).ToArray();
    }
}
