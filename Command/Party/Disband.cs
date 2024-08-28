using MCGalaxy;

public sealed class PartyDisband : Subcommand {
    public static string Name() { return "disband"; }

    public static string Description() { return "Disband the current party"; }

    public static string? Format() { return null; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 0) {
            return false;
        }

        Party party = Parties.GetParty(p);

        if (party == null) {
            p.MessageLines(Formatter.BarsWrap("&cYou are not in a party!")
                               .Split('\n'));
            return true;
        }

        if(party.Leader != p) {
            p.MessageLines(Formatter.BarsWrap("&cOnly the party leader can do this!")
                               .Split('\n'));
            return true;
        }

        // TODO party broadcast
        Parties.Remove(party);
        return true;
    }
}
