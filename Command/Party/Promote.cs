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

        OnlinePlayer target = OnlinePlayers.Find(args[0]);

        if (target == null || !party.Contains(target.player)) {
            p.MessageLines(
                Formatter.BarsWrap("&cThat player is not in your party!")
                    .Split('\n'));
            return true;
        }

        if (target.player == p) {
            p.MessageLines(Formatter.BarsWrap("&cYou cannot promote yourself!")
                               .Split('\n'));
            return true;
        }

        if (!party.IsHigher(p, target.player)) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou don't have permission to do that!")
                    .Split('\n'));
            return true;
        }

        switch (party.Promote(target.player)) {
        case 0:
        case 1:
            throw new Exception("unreachable");
        case 2:
            target.player.MessageLines(
                Formatter.BarsWrap("You have been promoted to party moderator.")
                    .Split('\n'));
            party.TellExcept(
                target.player,
                Formatter.BarsWrap(
                    $"{target.player.ColoredName} &ehas been promoted to party moderator."));
            break;
        case 3:
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"{target.player.ColoredName} &ehas been promoted to party leader.\nYou are now a party moderator.")
                    .Split('\n'));
            target.player.MessageLines(
                Formatter
                    .BarsWrap(
                        $"You have been promoted to party leader.\n{p.ColoredName} &eis now a party moderator.")
                    .Split('\n'));

            HashSet<Player> except =
                new HashSet<Player>(new Player[] { p, target.player });
            party.TellExcept(
                except,
                Formatter.BarsWrap(
                    $"{target.player.ColoredName} &ebeen promoted to party leader.\n{p.ColoredName} &eis now a party moderator."));
            break;
        }

        return true;
    }
}
