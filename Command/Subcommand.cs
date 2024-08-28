using MCGalaxy;

public interface Subcommand {
    public static abstract string Name();
    public static abstract string Description();
    public static abstract string? Format();

    public static string FormatText(string category, string name, string? format) {
        if (format == null) {
            return $"/{category} {name}";
        }

        return $"/{category} {name} {format}";
    }

    public static abstract bool Run(Player p, string[] args);
}
