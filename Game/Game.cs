using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

public abstract class Game {
    private Level map;
    private static volatile uint lastMapUpdate = 0;
    private static volatile string[] maps;
    public abstract string ModeName();
    public abstract string ShortName();
    public static string StaticModeName() { return null; }
    public static string StaticShortName() { return null; }
    public bool HasStarted = false;

    public string[] UpdateMaps() {
        return Directory.GetFiles($"levels/games/{ModeName()}/", "*.lvl")
            .Select((path) => path.Substring(0, path.LastIndexOf('.'))
                                  .Substring(path.LastIndexOf('/') + 1))
            .ToArray();
    }

    public long startTime;
    public string Name {
        get { return map.name; }
    }

    /// -1: stop countdown
    /// 0: now
    /// anything else: seconds
    public abstract int BroadcastCountdown();

    /// -1: stop countdown
    /// 0: now
    /// anything else: seconds
    public abstract int GameStartCountdown();

    private volatile int broadcastCountdownFrom = -1;
    private volatile int gamestartCountdownFrom = -1;
    private volatile int countdownTryNumber = 0;

    public void CountdownUpdateRequest() {
        int gamestartNew = GameStartCountdown();

        if (gamestartNew == 0) {
            countdownTryNumber++;
            OnStartEvent();
            return;
        }

        if (gamestartNew == -1 && gamestartCountdownFrom != -1) {
            gamestartCountdownFrom = -1;
            countdownTryNumber++;
        }

        if (gamestartNew != gamestartCountdownFrom) {
            gamestartCountdownFrom = gamestartNew;
            countdownTryNumber++;

            // { countdown type, countdown from, try number}
            // countdown type: 1 - gamestart, 2 - broadcast
            int[] state = { 1, gamestartNew, countdownTryNumber };
            CountdownTick(
                new SchedulerTask(CountdownTick, state, TimeSpan.Zero, false));
            return;
        }

        int broadcastNew = BroadcastCountdown();

        if (broadcastNew == -1 && broadcastCountdownFrom != -1) {
            broadcastCountdownFrom = -1;
            countdownTryNumber++;
            return;
        }

        if (broadcastNew != broadcastCountdownFrom) {
            broadcastCountdownFrom = broadcastNew;
            countdownTryNumber++;

            // { countdown type, countdown from, try number}
            // countdown type: 1 - gamestart, 2 - broadcast
            int[] state = { 2, broadcastNew, countdownTryNumber };
            CountdownTick(
                new SchedulerTask(CountdownTick, state, TimeSpan.Zero, false));
        }
    }

    public void MessagePlayers(string content) {
        foreach (Player p in players) {
            p.MessageLines(content.Split('\n'));
        }
    }

    public void MessageSpectators(string content) {
        foreach (Player p in spectators) {
            p.MessageLines(content.Split('\n'));
        }
    }

    public void MessageAll(string content) {
        MessagePlayers(content);
        MessageSpectators(content);
    }

    private void CountdownTick(SchedulerTask task) {
        int[] state = (int[])task.State;

        if (countdownTryNumber != state[2])
            return;

        if (state[1] == 0)
            return;

        switch (state[0]) {
        case 1:
            MessageAll(
                $"Game starting in {Formatter.Duration(state[1], 3)}");
            break;
        case 2:
            MessageAll(
                $"Announcing to lobby in {Formatter.Duration(state[1], 3)}");
            break;
        }

        state[1]--;
        Server.MainScheduler.QueueOnce(CountdownTick, state,
                                       new TimeSpan(0, 0, 1));
    }

    public volatile List<Player> players = new List<Player>();
    public volatile List<Player> spectators = new List<Player>();
    public Level Map {
        get { return map; }
    }
    public abstract int MaxPlayers { get; }

    public abstract bool CanSpectate();
    public bool CanJoin() {
        return !HasStarted && players.Count() != MaxPlayers;
    }

    public Game() {
        uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now - Bancho.Config.MapUpdateInterval > lastMapUpdate) {
            maps = UpdateMaps();
            lastMapUpdate = now;
        }

        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        Random random = new Random();

        string mapName = maps[random.Next(maps.Length)];
        string name;

        do {
            string discriminator = string.Empty;

            for (int i = 0; i < 6; i++) {
                discriminator += chars[random.Next(chars.Length)];
            }

            name = $"{ModeName()}-{discriminator}";
        } while (Games.HasGame(name));

        string mapPath = $"games/{ModeName()}/{mapName}";

        Level lvl = Level.LoadWithPath(mapName, mapPath);
        lvl.SaveChanges = false;
        lvl.name = name;
        map = lvl;
        LevelInfo.Add(lvl);

        startTime = now;
        Games.Add(this);
        OnLoadEvent();
    }

    public void Unload() {
        countdownTryNumber++;
        OnUnloadEvent();
        Games.Remove(map.name);
        LevelInfo.Remove(map);
        map.Unload();
        map = null;

        foreach (Player p in players) {
            PlayerActions.ChangeMap(p, Bancho.Config.MainLevel);
        }

        foreach (Player p in spectators) {
            PlayerActions.ChangeMap(p, Bancho.Config.MainLevel);
        }
    }

    /// 0: already in game
    /// 1: cannot join game
    /// 2: joined the game
    public int PlayerJoin(Player p) {
        if (players.Contains(p))
            return 0;

        if (MaxPlayers == players.Count()) {
            return 1;
        }

        players.Add(p);

        if (spectators.Remove(p)) {
            OnSpectatorToPlayerEvent(p);
        } else {
            OnPlayerJoinEvent(p);
        }

        return 2;
    }

    public bool PlayerLeave(Player p) {
        if (players.Remove(p)) {
            PlayerLeave(p);
            if (players.Count() == 0) {
                Unload();
            }

            return true;
        }

        return false;
    }

    public bool SpectatorLeave(Player p) {
        if (spectators.Remove(p)) {
            SpectatorLeave(p);
            return true;
        }

        return false;
    }

    public void OnPlayerJoinEvent(Player p) {}
    public void OnSpectatorToPlayerEvent(Player p) { OnPlayerJoinEvent(p); }
    public void OnSpectatorJoinEvent(Player p) {}

    public void OnPlayerLeaveEvent(Player p) {}
    public void OnSpectatorLeaveEvent(Player p) {}

    public void OnPlayerMidgameRejoin(Player p) {}
    public void OnPlayerMidgameLeave(Player p) {}

    public void OnLoadEvent() {}
    public void OnUnloadEvent() {}

    public void OnPlayerChatEvent(Player p, string msg) {}
    public void OnPlayerMoveEvent(Player p, Position next, byte yaw, byte pitch,
                                  ref bool cancel) {}
    public void OnBlockChangingEvent(Player p, ushort x, ushort y, ushort z,
                                     BlockID block, bool placing,
                                     ref bool cancel) {}
    public void OnPlayerClickEvent(Player p, MouseButton button,
                                   MouseAction action, ushort yaw, ushort pitch,
                                   byte entity, ushort x, ushort y, ushort z,
                                   TargetBlockFace face) {}
    public void OnStartEvent() {}

    /// 0: already in game
    /// 1: cannot join game
    /// 2: joined the game
    public int Join(Player p) {
        int res = PlayerJoin(p);

        if (res != 2)
            return res;

        PlayerActions.ChangeMap(p, map);
        OnPlayerJoinEvent(p);
        CountdownUpdateRequest();
        return res;
    }
}
