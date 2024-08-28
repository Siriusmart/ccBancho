using MCGalaxy;

public sealed class PartyLeave : Subcommand {
    public static string Name() { return "leave"; }

    public static string Description() { return "Leave your current party"; }

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

        switch (party.Remove(p)) {
        case 0:
            // TODO party broadcast
            p.MessageLines(
                Formatter.BarsWrap($"You left the party.").Split('\n'));
            break;
        case 1:
            // TODO party broadcast
            p.MessageLines(
                Formatter.BarsWrap($"You left an empty party and is disbanded.")
                    .Split('\n'));
            break;
        case 2:
            // TODO party broadcast
            p.MessageLines(
                Formatter.BarsWrap($"You left the party.").Split('\n'));
            break;
        case 3:
            throw new Exception("unreachable");
        }

        return true;
    }
}
