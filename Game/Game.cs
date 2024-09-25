using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

public abstract class Game {
    public Level map;
    private static volatile uint lastMapUpdate = 0;
    private static volatile string[] maps;
    public abstract string ModeName();
    public abstract string ShortName();
    public static string StaticModeName() { return null; }
    public static string StaticShortName() { return null; }
    public bool HasStarted = false;
    public bool HasEnded = false;
    public string currentMap;

    public string[] UpdateMaps() {
        return Directory.GetFiles($"levels/games/{ModeName()}/", "*.lvl")
            .Select((path) => path.Substring(0, path.LastIndexOf('.'))
                                  .Substring(path.LastIndexOf('/') + 1))
            .ToArray();
    }

    public string MapConfigPath {
        get { return $"levels/games/{ModeName()}/{currentMap}.properties"; }
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
            HasStarted = true;
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

    static HashSet<int> CountdownTicks = new HashSet<int>(
        new int[] { 1, 2, 3, 4, 5, 10, 20, 60, 120, 180, 240, 300 });

    private void CountdownStartTick(int duration) {
        if (CountdownTicks.Contains(duration)) {
            MessageAll(
                $"&eGame starting in {Formatter.Duration(duration, 3)}");

            if (duration <= 3) {
                SendCPEMessagePlayers(CpeMessageType.BigAnnouncement,
                                      $"&c{duration.ToString()}");
            } else if (duration <= 60) {
                SendCPEMessagePlayers(CpeMessageType.BigAnnouncement,
                                      $"&e{duration.ToString()}");
            }
        } else if (CountdownTicks.Contains(duration + 1)) {
            SendCPEMessagePlayers(CpeMessageType.BigAnnouncement, string.Empty);
        }
    }

    private void CountdownTick(SchedulerTask task) {
        int[] state = (int[])task.State;

        if (countdownTryNumber != state[2])
            return;

        if (state[1] == 0) {
            if (state[0] == 1)
                CountdownStartTick(0);
            HasStarted = true;
            OnStartEvent();
            return;
        }

        switch (state[0]) {
        case 1:
            CountdownStartTick(state[1]);
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

    public void DelayedUnload(int delay) {
        Server.MainScheduler.QueueOnce(DelayedUnloadTask, new Object(),
                                       new TimeSpan(0, 0, delay));
    }

    private void DelayedUnloadTask(SchedulerTask _) { Unload(); }

    public void SendCPEMessagePlayers(CpeMessageType type, string message) {
        foreach (Player p in players) {
            p.SendCpeMessage(type, message);
        }
    }

    public void SendCPEMessageSpectators(CpeMessageType type, string message) {
        foreach (Player p in spectators) {
            p.SendCpeMessage(type, message);
        }
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
        currentMap = mapName;
        LevelInfo.Add(lvl);

        startTime = now;
        Games.Add(this);
        OnLoadEvent();
    }

    public void Unload() {
        foreach (Player p in players) {
            PlayerActions.ChangeMap(p, Bancho.Config.MainLevel);
        }

        foreach (Player p in spectators) {
            PlayerActions.ChangeMap(p, Bancho.Config.MainLevel);
        }

        countdownTryNumber++;
        OnUnloadEvent();
        Games.Remove(map.name);
        LevelInfo.Remove(map);
        map.Unload();
        map = null;
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
            OnPlayerLeaveEvent(p);
            if (players.Count() == 0) {
                Unload();
            }

            return true;
        }

        return false;
    }

    public bool SpectatorLeave(Player p) {
        if (spectators.Remove(p)) {
            OnSpectatorLeaveEvent(p);
            return true;
        }

        return false;
    }

    public void Disconnect(Player p) {
        if (SpectatorLeave(p))
            return;

        if (players.Contains(p)) {
            if (HasStarted) {
                OnPlayerMidgameLeaveEvent(p);
            } else {
                PlayerLeave(p);
                OnPlayerLeaveEvent(p);
            }

            if (!HasStarted)
                CountdownUpdateRequest();
        }
    }

    public virtual void OnPlayerJoinEvent(Player p) {}
    public virtual void OnSpectatorToPlayerEvent(Player p) {
        OnPlayerJoinEvent(p);
    }
    public virtual void OnSpectatorJoinEvent(Player p) {}

    public virtual void OnPlayerLeaveEvent(Player p) {}
    public virtual void OnSpectatorLeaveEvent(Player p) {}

    public virtual void OnPlayerMidgameRejoinEvent(Player p) {}
    public virtual void OnPlayerMidgameLeaveEvent(Player p) {}

    public virtual void OnLoadEvent() {}
    public virtual void OnUnloadEvent() {}

    public virtual void OnPlayerChatEvent(Player p, string msg) {}
    public virtual void OnPlayerMoveEvent(Player p, Position next, byte yaw,
                                          byte pitch, ref bool cancel) {}
    public virtual void OnBlockChangingEvent(Player p, ushort x, ushort y,
                                             ushort z, BlockID block,
                                             bool placing, ref bool cancel) {}
    public virtual void OnPlayerClickEvent(Player p, MouseButton button,
                                           MouseAction action, ushort yaw,
                                           ushort pitch, byte entity, ushort x,
                                           ushort y, ushort z,
                                           TargetBlockFace face) {}
    public virtual void OnStartEvent() {}

    /// 0: already in game
    /// 1: cannot join game
    /// 2: joined the game
    public int Join(Player p) {
        OnlinePlayer player = OnlinePlayers.GetPlayer(p);
        if (player == null)
            return 1;

        int res = PlayerJoin(p);

        if (res == 0 && !HasStarted)
            p.SendPosition(p.level.SpawnPos, new Orientation(0, 0));

        if (res != 2)
            return res;

        player.game = this;

        PlayerActions.ChangeMap(p, map);
        CountdownUpdateRequest();
        return res;
    }
}
