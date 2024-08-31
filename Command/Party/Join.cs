using MCGalaxy;

public sealed class PartyJoin : Subcommand {
    public static string Name() { return "join"; }

    public static string Description() { return "Request to join a party"; }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        OnlinePlayer target = OnlinePlayers.Find(args[0]);

        Party party = target == null ? null : Parties.GetParty(target.player);

        if (party == null) {
            p.MessageLines(
                Formatter.BarsWrap($"&cThat player is not in a party.")
                    .Split('\n'));
            return true;
        }

        if (target.player == p) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou cannot join yourself!").Split('\n'));
            return true;
        }

        if (Parties.GetParty(p) != null) {
            if (party.Contains(p)) {
                p.MessageLines(
                    Formatter
                        .BarsWrap(
                            $"&cYou are already in a party with that player!")
                        .Split('\n'));
                return true;
            }

            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&cYou are already in a party.\nYou can only be in one party at a time.")
                    .Split('\n'));
            return true;
        }

        long cooldown = party.RequestedCooldownRemaining(p);
        if (cooldown > 0) {
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&ePlease wait &c{Formatter.Duration(cooldown, 2)} before doing this.")
                    .Split('\n'));
            return true;
        }

        switch (party.Request(target.player, p)) {
        case 0:
            p.MessageLines(
                Formatter.BarsWrap($"&cThe party has join requests disabled.")
                    .Split('\n'));
            break;
        case 1:
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eJoin request sent. They have &c{Formatter.Duration(Bancho.Config.InviteCooldown, 1)} &eto accept your request.")
                    .Split('\n'));
            party.Tell(Formatter.BarsWrap(
                $"{p.ColoredName} &ehas requested to join your party.\nYou have &c{Formatter.Duration(Bancho.Config.InviteCooldown, 1)} &eto accept this request.\nAccept the request with /party invite {p.name}"));
            break;
        case 2:
            string[] members =
                party.FlatList()
                    .Where(player => player != p && player != party.Leader)
                    .Select(player => player.ColoredName)
                    .ToArray();

            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eYou have joined {party.Leader.ColoredName}'s &eparty{
                        (members.Length == 1 ?
                        $" with {Formatter.ListItems(members)}" :
                        string.Empty)
                        }.")
                    .Split('\n'));

            party.TellExcept(
                p, Formatter.BarsWrap($"{p.ColoredName} &ejoined the party."));
            break;
        case 3:
            p.MessageLines(
                Formatter.BarsWrap($"&cYou are already in the party!")
                    .Split('\n'));
            break;
        }

        return true;
    }
}
