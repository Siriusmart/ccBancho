using MCGalaxy;

public sealed class FriendList : Subcommand {
    public static string Name() { return "list"; }

    public static string Description() { return "List all friends"; }

    public static string? Format() { return "<page>"; }

    public static bool Run(Player p, string[] args) {
        if (args.Length > 1) {
            return false;
        }

        ushort page = 1;

        if (args.Length == 1) {
            if (!ushort.TryParse(args[0], out page)) {
                return false;
            }
        }

        if (page == 0) {
            page++;
        }

        OnlinePlayer player = OnlinePlayers.GetPlayer(p);

        if (player.Friends.Count() == 0) {
            p.MessageLines(
                Formatter.FriendBarsWrap($"&cNeed a hug? You have no friends!")
                    .Split('\n'));
            return true;
        }

        List<OnlinePlayer> online = new List<OnlinePlayer>();
        List<OnlinePlayer> offline = new List<OnlinePlayer>();

        foreach (Friend friend in player.Friends) {
            OnlinePlayer friendPlayer =
                OnlinePlayers.FindExact(friend.player.name);
            if (PlayerInfo.Online.Contains(friendPlayer.player)) {
                online.Add(friendPlayer);
            } else {
                offline.Add(friendPlayer);
            }
        }

        ushort maxPossible =
            (ushort)Math.Ceiling((float)player.Friends.Count() /
                                 (float)Bancho.Config.FriendPageSize);
        if (page > maxPossible) {
            page = maxPossible;
        }

        int pageMin = Bancho.Config.FriendPageSize * (page - 1);
        int pageMax = Math.Min(pageMin + Bancho.Config.FriendPageSize,
                               online.Count() + offline.Count());

        bool isOnline = true;

        List<string> pList = new List<string>();

        for (int i = pageMin; i < pageMax; i++) {
            if (isOnline && i >= online.Count()) {
                isOnline = false;
            }

            if (isOnline) {
                pList.Add($"{online[i].player.ColoredName} &eis online.");
            } else {
                pList.Add(
                    $"{offline[i - online.Count()].player.ColoredName} &cis currently offline.");
            }
        }

        p.MessageLines(
            Formatter
                .FriendBarsWrap(
                    $"&6Friends (Page {page} of {maxPossible})\n{pList.Join("\n")}")
                .Split('\n'));

        return true;
    }
}
