using MCGalaxy;

public sealed class FriendAdd : Subcommand {
    public static string Name() { return "add"; }

    public static string Description() { return "Add a player as friend"; }

    public static string? Format() { return "[player]"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length != 1) {
            return false;
        }

        Player target = PlayerInfo.FindExact(args[0]);

        if (!PlayerInfo.Online.Contains(target)) {
            p.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&cThat player is not online at the moment.")
                    .Split('\n'));
            return true;
        }

        OnlinePlayer targetPlayer = OnlinePlayers.GetPlayer(target);

        if (target == p) {
            p.MessageLines(
                Formatter.FriendBarsWrap("&cYou cannot friend yourself!")
                    .Split('\n'));
            return true;
        }

        long cooldown = targetPlayer.FriendRequestCooldownRemaining(p);
        if (cooldown > 0) {
            p.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&ePlease wait &c{Formatter.Duration(cooldown, 2)} before doing this.")
                    .Split('\n'));
            return true;
        }

        switch (OnlinePlayers.GetPlayer(p).FriendAdd(target)) {
        case 0:
            throw new Exception("unreachable");
        case 1:
            target.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&eYou got a friend request from {p.ColoredName}!\nYou have &c{Formatter.Duration(Bancho.Config.FriendCooldown, 1)} &eaccept this request.\nAdd {p.ColoredName} &eas a friend with /friend add {p.name}")
                    .Split('\n'));
            p.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"Friend request sent to {target.ColoredName}\nThey have &c{Formatter.Duration(Bancho.Config.FriendCooldown, 1)} &eaccept the request.")
                    .Split('\n'));
            break;
            break;
        case 2:
            p.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&aYou are now friends with {target.ColoredName}&a!")
                    .Split('\n'));
            target.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&aYou are now friends with {p.ColoredName}&a!")
                    .Split('\n'));
            break;
        case 3:
            p.MessageLines(
                Formatter
                    .FriendBarsWrap(
                        $"&cYou are already friends with this player!")
                    .Split('\n'));
            break;
        }

        return true;
    }
}
