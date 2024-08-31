using MCGalaxy;

public sealed class PartyKick : Subcommand {
    public static string Name() { return "kick"; }

    public static string Description() { return "Kick a party member"; }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        Party party = Parties.GetParty(p);

        if (party == null) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou are not in a party.").Split('\n'));
            return true;
        }

        OnlinePlayer target = OnlinePlayers.Find(args[0]);

        if (target == null || !party.Contains(target.player)) {
            p.MessageLines(
                Formatter.BarsWrap("&cThat player is not in your party.")
                    .Split('\n'));
            return true;
        }

        if (target.player == p) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou cannot kick yourself!").Split('\n'));
            return true;
        }

        if (target.player != null && !party.IsHigher(p, target.player)) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou don't have permission to do that!")
                    .Split('\n'));
            return true;
        }

        switch (party.Remove(target.player)) {
        case 0:
            target.player.MessageLines(
                Formatter.BarsWrap($"&eYou have been kicked from the party.")
                    .Split('\n'));
            party.Tell(Formatter.BarsWrap(
                $"{target.player.ColoredName} &ehas been kicked from the party."));
            break;
        case 1:
        case 2:
            throw new Exception("unreachable");
        case 3:
            throw new Exception("unreachable");
        }

        return true;
    }
}
