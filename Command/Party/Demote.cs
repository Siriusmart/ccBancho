using MCGalaxy;

public sealed class PartyDemote : Subcommand {
    public static string Name() { return "demote"; }

    public static string Description() {
        return "Demote party member to a lower role";
    }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        Party party = Parties.GetParty(p);

        if (party == null) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou are not in a party!").Split('\n'));
            return true;
        }

        Player target = party.GetPlayer(args[0]);

        if (target == null || !party.Contains(target)) {
            p.MessageLines(
                Formatter.BarsWrap("&cThat player is not in your party!")
                    .Split('\n'));
            return true;
        }

        if (target == p) {
            p.MessageLines(Formatter.BarsWrap("&cYou cannot demote yourself!")
                               .Split('\n'));
            return true;
        }

        if (!party.IsHigher(p, target)) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou don't have permission to do that!")
                    .Split('\n'));
            return true;
        }

        switch (party.Demote(target)) {
        case 0:
        case 1:
            throw new Exception("unreachable");
        case 2:
            p.MessageLines(
                Formatter
                    .BarsWrap($"{target.ColoredName} &cis already a member.")
                    .Split('\n'));
            break;
        case 3:
            target.MessageLines(
                Formatter.BarsWrap($"&eYou have been demoted to party member.")
                    .Split('\n'));
            party.TellExcept(
                target,
                Formatter.BarsWrap(
                    $"{target.ColoredName} &ehas been demoted to party member."));
            break;
        }

        return true;
    }
}
