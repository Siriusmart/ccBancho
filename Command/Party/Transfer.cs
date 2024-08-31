using MCGalaxy;

public sealed class PartyTransfer : Subcommand {
    public static string Name() { return "transfer"; }

    public static string Description() { return "Transfer party ownership"; }

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
            p.MessageLines(
                Formatter.BarsWrap("&cYou transfer to yourself!").Split('\n'));
            return true;
        }

        if (!party.IsOwner(p)) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou are not the owner!").Split('\n'));
            return true;
        }

        if (party.Transfer(target)) {
            target.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eYou are given party ownership.\n{p.ColoredName} &eis now a party moderator.")
                    .Split('\n'));
            party.TellExcept(
                target,
                Formatter.BarsWrap(
                    $"&eThe party is transferred to {target.ColoredName}&e.\n{p.ColoredName} &eis now a party moderator."));
        }

        return true;
    }
}
