using MCGalaxy;

public sealed class PartyPromote : Subcommand {
    public static string Name() { return "promote"; }

    public static string Description() {
        return "Promote party member to a higher role";
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

        Player target = PlayerInfo.FindExact(args[0]);

        if (!party.Contains(target)) {
            p.MessageLines(
                Formatter.BarsWrap("&cThat player is not in your party!")
                    .Split('\n'));
            return true;
        }

        if (!party.IsHigher(p, target)) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou don't have permission to do that!")
                    .Split('\n'));
            return true;
        }

        switch (party.Promote(target)) {
        case 0:
        case 1:
            throw new Exception("unreachable");
        case 2:
            // TODO party broadcast
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"{target.ColoredName} &ehas been promoted to party moderator.")
                    .Split('\n'));
            target.MessageLines(
                Formatter.BarsWrap("You have been promoted to party moderator.")
                    .Split('\n'));
            break;
        case 3:
            // TODO party broadcast
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"{target.ColoredName} &ehas been promoted to party leader.\nYou are now a party moderator.")
                    .Split('\n'));
            target.MessageLines(
                Formatter
                    .BarsWrap(
                        $"You have been promoted to party leader.\n{p.ColoredName} &eis now a party moderator.")
                    .Split('\n'));
            break;
        }

        return true;
    }
}
