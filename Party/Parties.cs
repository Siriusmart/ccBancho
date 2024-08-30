using MCGalaxy;

public class Parties {
    private static HashSet<Party> all = new HashSet<Party>();

    public static int Count() { return all.Count(); }

    public static Party Create(Player leader) {
        Party party = new Party(leader);
        all.Add(party);

        return party;
    }

    public static Party? GetParty(Player p) {
        OnlinePlayer? player = OnlinePlayers.GetPlayer(p);
        if (player != null)
            return player.party;

        return null;
    }

    public static Party? GetPartyInit(Player p) {
        foreach (Party party in all) {
            if (party.ContainsInit(p))
                return party;
        }
        return null;
    }

    public static void ReplacePlayer(Player p) {
        foreach (Party party in all) {
            party.ReplacePlayer(p);
        }
    }

    public static Party? GetParty(OnlinePlayer p) { return p.party; }

    public static bool Remove(Party party) { return all.Remove(party); }
}
