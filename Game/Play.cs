using MCGalaxy;

public class GamePlay : Command {
    public override string name {
        get { return "play"; }
    }
    public override string type {
        get { return "games"; }
    }

    public override void Use(Player p, string message) {
        string[] args = message.SplitSpaces();

        if (!Games.HasGameMode(args[0])) {
            p.Message("&cThat is not a valid gamemode!");
            return;
        }

        Game game = Games.FindGameToJoin(args[0]);

        if (game == null) {
            p.Message("&cWe could not find a game to join.");
            return;
        }

        game.Join(p);
    }

    public override void Help(Player p) {
        p.Message("&e/msg [player] [message] &7- &bSend a private message to " +
                  "a player.");
    }
}
