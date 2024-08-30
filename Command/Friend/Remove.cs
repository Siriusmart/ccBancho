using MCGalaxy;

public sealed class FriendRemove : Subcommand {
    public static string Name() { return "remove"; }

    public static string Description() { return "Unfriend a player"; }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        Player target = OnlinePlayers.Find(args[0]);
        OnlinePlayer remover = OnlinePlayers.GetPlayer(p);

        if (target == p) {
            p.MessageLines(
                Formatter.FriendBarsWrap("&cYou cannot unfriend yourself!")
                    .Split('\n'));
            return true;
        }

        if (!remover.HasFriend(target)) {
            p.MessageLines(
                Formatter
                    .FriendBarsWrap("&cYou are not friends with that player.")
                    .Split('\n'));
            return true;
        }

        OnlinePlayer targetPlayer = OnlinePlayers.GetPlayer(target);
        if (targetPlayer != null) {
            targetPlayer.Unfriend(p);
            target.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"{p.ColoredName} &e removed you as a friend.")
                    .Split('\n'));
        } else {
            OnlinePlayer.UnfriendSave(target, p);
        }

        remover.Unfriend(target);

        p.MessageLines(
            Formatter
                .FriendBarsWrap(
                    $"&eYou are no longer friends with {target.ColoredName}.")
                .Split('\n'));

        return true;
    }
}
