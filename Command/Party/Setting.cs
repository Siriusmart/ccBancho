using MCGalaxy;

public sealed class PartySetting : Subcommand {
    public static string Name() { return "setting"; }

    public static string Description() { return "Change party settings"; }

    public static string? Format() { return "<option>"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length == 0) {
            p.MessageLines(Formatter
                               .BarsWrap(@$"&aParty settings:
&eUsage: /party setting [option]
&f- &eallInvite &7- &bToggle all invites
&f- &eallowRequest &7- &bToggle allow request
&f- &emute &7- &bToggle party chat mute")
                               .Split("\n"));
            return true;
        }

        if (args.Length != 1) {
            return false;
        }

        Party party = Parties.GetParty(p);

        if (party == null) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou are not in a party.").Split('\n'));
            return true;
        }

        if (party.Permission(p) < 2) {
            p.MessageLines(
                Formatter.BarsWrap("&cYou don't have permission to do that!")
                    .Split('\n'));
            return true;
        }

        bool? newValue = party.ToggleSetting(args[0]);

        if (newValue == null) {
            p.MessageLines(Formatter.BarsWrap("&cThat is not a valid option.")
                               .Split('\n'));
            return true;
        }

        if ((bool)newValue) {
            party.Tell(
                Formatter.BarsWrap($"&a{args[0]} has been set to TRUE."));
        } else {
            party.Tell(
                Formatter.BarsWrap($"&c{args[0]} has been set to FALSE."));
        }

        return true;
    }
}
