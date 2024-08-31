using MCGalaxy;

public sealed class PartyInvite : Subcommand {
    public static string Name() { return "invite"; }

    public static string Description() {
        return "Invite a player to your party";
    }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        OnlinePlayer target = OnlinePlayers.Find(args[0]);

        if (target == null || !PlayerInfo.Online.Contains(target.player)) {
            p.MessageLines(
                Formatter
                    .BarsWrap($"&cThat player is not online at the moment.")
                    .Split('\n'));
            return true;
        }

        if (target.player == p) {
            p.MessageLines(Formatter.BarsWrap("&cYou cannot invite yourself!")
                               .Split('\n'));
            return true;
        }

        bool temporary = false;

        Party party = Parties.GetParty(p);

        if (party == null) {
            temporary = true;
            party = Parties.Create(p);
        } else if (party.Contains(target.player)) {
            p.MessageLines(
                Formatter
                    .BarsWrap($"&cYou are already in a party with that player!")
                    .Split('\n'));
            return true;
        }

        if (!party.CanInvite(p)) {
            p.MessageLines(
                Formatter
                    .BarsWrap("&cYou don't have permission to invite players!")
                    .Split('\n'));
            return true;
        }

        long cooldown = party.InvitedCooldownRemaining(target.player);
        if (cooldown > 0) {
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&ePlease wait &c{Formatter.Duration(cooldown, 2)} before doing this.")
                    .Split('\n'));
            return true;
        }

        switch (party.Invite(target.player, p)) {
        case 0:
            target.player.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eYou have been invited to join {p.ColoredName}'s &eparty! \nYou have &c{Formatter.Duration(Bancho.Config.InviteCooldown, 1)} &eaccept this invite.\nJoin the party with /party join {p.name}")
                    .Split('\n'));
            party.Tell(Formatter.BarsWrap(
                $"{p.ColoredName} &einvited {target.player.ColoredName} &eto the party! \nThey have &c{Formatter.Duration(Bancho.Config.InviteCooldown, 1)} &eaccept the invite."));
            break;
        case 1:
            string[] members = party.FlatList()
                                   .Where(player => player != target.player &&
                                                    player != party.Leader)
                                   .Select(player => player.ColoredName)
                                   .ToArray();

            target.player.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eYou have joined {party.Leader.ColoredName}'s &eparty{
                        (members.Length == 1 ?
                        $" with {Formatter.ListItems(members)}" :
                        string.Empty)
                        }.")
                    .Split('\n'));

            party.TellExcept(
                target.player,
                Formatter.BarsWrap(
                    $"{target.player.ColoredName} &ejoined the party."));
            break;
        case 2:
            throw new Exception("unreachable");
        case 3:
            p.MessageLines(
                Formatter.BarsWrap($"&cThis player is already in your party!")
                    .Split('\n'));
            if (temporary)
                Parties.Remove(party);
            break;
        }

        return true;
    }
}
