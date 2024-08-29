using MCGalaxy;

public class Parties {
    private static HashSet<Party> all = new HashSet<Party>();

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

    public static bool Remove(Party party) { return all.Remove(party); }
}
