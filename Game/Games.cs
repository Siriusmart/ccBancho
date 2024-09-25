using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;

public class Games {
    private static volatile Dictionary<string, Game> games =
        new Dictionary<string, Game>();
    private static volatile Dictionary<string, Type> gameModes =
        new Dictionary<string, Type>();

    public static void RegisterGame(Type g) {
        string name = (string)g.GetMethod("StaticModeName").Invoke(null, null);
        string shortName =
            (string)g.GetMethod("StaticShortName").Invoke(null, null);

        if (HasGameMode(name)) {
            gameModes[name] = g;
        } else {
            gameModes.Add(name, g);
        }

        if (shortName != null) {
            if (HasGameMode(shortName)) {
                gameModes[shortName] = g;
            } else {
                gameModes.Add(shortName, g);
            }
        }
    }

    public static void UnregisterGame(string name) { gameModes.Remove(name); }

    public static bool HasGame(string id) { return games.ContainsKey(id); }
    public static bool HasGameMode(string id) {
        return gameModes.ContainsKey(id);
    }
    public static Game? GetGame(string id) {
        return HasGame(id) ? games[id] : null;
    }

    public static void Add(Game game) { games.Add(game.Map.name, game); }
    public static bool Remove(string id) { return games.Remove(id); }

    public static Game? FindGameToJoin(string gamemode) {
        if (!gameModes.ContainsKey(gamemode))
            return null;

        foreach (Game g in games.Values) {
            if ((g.ModeName() == gamemode || g.ShortName() == gamemode) &&
                g.CanJoin())
                return g;
        }

        return (Game)Activator.CreateInstance(gameModes[gamemode]);
    }

    public static void OnLoad() {
        OnPlayerChatEvent.Register(OnPlayerChat, Priority.Low);
        OnPlayerMoveEvent.Register(OnPlayerMove, Priority.Low);
        OnBlockChangingEvent.Register(OnBlockChanging, Priority.Low);
        OnPlayerClickEvent.Register(OnPlayerClick, Priority.Low);

        Command.Register(new GamePlay());
    }

    public static void OnUnload() {
        foreach (Game g in games.Values) {
            g.Unload();
        }

        Command.Unregister(Command.Find("play"));

        OnPlayerChatEvent.Unregister(OnPlayerChat);
        OnPlayerMoveEvent.Unregister(OnPlayerMove);
        OnBlockChangingEvent.Unregister(OnBlockChanging);
        OnPlayerClickEvent.Unregister(OnPlayerClick);
    }

    public static void OnPlayerChat(Player p, string msg) {
        if (games.ContainsKey(p.level.name))
            games[p.level.name].OnPlayerChatEvent(p, msg);
    }

    public static void OnPlayerMove(Player p, Position next, byte yaw,
                                    byte pitch, ref bool cancel) {
        if (games.ContainsKey(p.level.name))
            games[p.level.name].OnPlayerMoveEvent(p, next, yaw, pitch,
                                                  ref cancel);
    }

    public static void OnPlayerLeave(Player p, Position next, byte yaw,
                                     byte pitch, ref bool cancel) {
        if (games.ContainsKey(p.level.name))
            games[p.level.name].OnPlayerMoveEvent(p, next, yaw, pitch,
                                                  ref cancel);
    }

    public static void OnBlockChanging(Player p, ushort x, ushort y, ushort z,
                                       BlockID block, bool placing,
                                       ref bool cancel) {
        if (games.ContainsKey(p.level.name)) {
            games[p.level.name].OnBlockChangingEvent(p, x, y, z, block, placing,
                                                     ref cancel);
            if (cancel)
                p.RevertBlock(x, y, z);
        }
    }

    public static void OnPlayerClick(Player p, MouseButton button,
                                     MouseAction action, ushort yaw,
                                     ushort pitch, byte entity, ushort x,
                                     ushort y, ushort z, TargetBlockFace face) {
        if (games.ContainsKey(p.level.name))
            games[p.level.name].OnPlayerClickEvent(
                p, button, action, yaw, pitch, entity, x, y, z, face);
    }
}
