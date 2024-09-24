public class TestGame : Game {
    public override string ModeName() { return "testgame"; }
    public override string ShortName() { return "tg"; }
    public static string StaticModeName() { return "testgame"; }
    public static string StaticShortName() { return "tg"; }

    public override int BroadcastCountdown() { return -1; }

    public override int GameStartCountdown() { return 20; }

    public override int MaxPlayers {
        get { return 2; }
    }

    public override bool CanSpectate() { return true; }
}
