using MCGalaxy;

public sealed class PartyCreate : Subcommand {
    public static string Name() { return "create"; }

    public static string Description() { return "Create a new party"; }

    public static string? Format() { return null; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 0) {
            return false;
        }

        if (Parties.GetParty(p) != null) {
            p.MessageLines(Formatter.BarsWrap("&cYou are already in a party!")
                               .Split('\n'));
            return true;
        }

        Parties.Create(p);

        p.MessageLines(Formatter.BarsWrap("&eParty created!").Split('\n'));
        return true;
    }
}
