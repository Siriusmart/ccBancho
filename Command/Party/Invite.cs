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

        bool temporary = false;

        Party party = Parties.GetParty(p);

        if (party == null) {
            temporary = true;
            party = Parties.Create(p);
        }

        if (!party.CanInvite(p)) {
            p.MessageLines(
                Formatter
                    .BarsWrap("&cYou don't have permission to invite players!")
                    .Split('\n'));
            return true;
        }

        Player target = PlayerInfo.FindExact(args[0]);

        long cooldown = party.InvitedCooldownRemaining(target);
        if (cooldown > 0) {
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&ePlease wait &c{Formatter.Duration(cooldown, 2)} before doing this.")
                    .Split('\n'));
            return true;
        }

        switch (party.Invite(target, p)) {
        case 0:
            p.MessageLines(
                Formatter
                    .BarsWrap(
                        $"{p.ColoredName} &einvited {target.ColoredName} &eto the party! \nThey have &c{Formatter.Duration(Party.InviteCooldown, 1)} &eto join the party.")
                    .Split('\n'));
            target.MessageLines(
                Formatter
                    .BarsWrap(
                        $"&eYou have been invited to join {p.ColoredName}'s &eparty! \nYou have &c{Formatter.Duration(Party.InviteCooldown, 1)} &eaccept this invite.\nJoin the party with /party join {p.name}")
                    .Split('\n'));
            break;
        case 1:
            // TODO party broadcast
            p.MessageLines(
                Formatter
                    .BarsWrap($"{target.ColoredName} &ehas joined the party.")
                    .Split('\n'));

            string[] members = party.Members.Concat(party.Moderators)
                                   .Where(player => player != target)
                                   .Select(player => player.ColoredName)
                                   .ToArray();
            Array.Sort(members);

            target.MessageLines(
                Formatter
                    .BarsWrap($"&eYou have joined {p.ColoredName}'s &eparty{
                        (members.Length == 1 ?
                        $" with {Formatter.ListItems(members)}" :
                        string.Empty)
                        }.")
                    .Split('\n'));
            break;
        case 2:
            // TODO party broadcast
            p.MessageLines(
                Formatter
                    .BarsWrap($"&cThat player is not online at the moment.")
                    .Split('\n'));
            if (temporary)
                Parties.Remove(party);
            break;
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
