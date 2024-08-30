using MCGalaxy;

public sealed class PartyList : Subcommand {
    public static string Name() { return "list"; }

    public static string Description() { return "List party members"; }

    public static string? Format() { return null; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 0) {
            return false;
        }

        Party party = Parties.GetParty(p);

        if (party == null) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou are not in a party.").Split('\n'));
            return true;
        }

        int partySize = party.FlatList().Length;
        List<string> people = new List<string>();

        Player leader = party.Leader;
        Player[] moderators = party.Moderators;
        Player[] members = party.Members;

        people.Add(
            $"&eParty leader: {leader.ColoredName} {OnlineIcon(leader)}");

        if (moderators.Length != 0)
            people.Add(
                $"&eParty moderators: {moderators.Select(player => $"{player.ColoredName} {OnlineIcon(player)}").ToArray().Join("  ")}");
        if (members.Length != 0)
            people.Add(
                $"&eParty members: {members.Select(player => $"{player.ColoredName} {OnlineIcon(player)}").ToArray().Join("  ")}");

        p.MessageLines(
            Formatter
                .BarsWrap($"&6Party Members ({partySize})\n{people.Join("\n")}")
                .Split("\n"));

        return true;
    }

    private static string OnlineIcon(Player p) {
        return PlayerInfo.Online.Contains(p) ? "&a*" : "&c*";
    }
}
