using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MongoDB.Bson;
using MongoDB.Driver;

public sealed class Bancho : Plugin {
    public static BanchoConfig Config = new BanchoConfig();
    public static MongoClient Mongo;
    public static IMongoDatabase BanchoDB;
    public static IMongoCollection<BsonDocument> BanchoPlayers;

    public override int build {
        get { return 1; }
    }

    public override string creator {
        get { return "siriusmart"; }
    }

    public override string name {
        get { return "Bancho"; }
    }

    public override string welcome {
        get { return "Bancho is now online!"; }
    }

    public override void Load(bool startup) {
        Config.Load();

        Mongo = new MongoClient(Config.MongoAddress);
        BanchoDB = Mongo.GetDatabase(Config.MongoName);
        BanchoPlayers = BanchoDB.GetCollection<BsonDocument>("players");

        OnlinePlayers.Init();

        Command.Register(new PartyEntry());
        Command.Register(new FriendEntry());
        Command.Register(new ChatEntry());
        Command.Register(new ChatGlobal());
        Command.Register(new ChatLocal());
        Command.Register(new ChatParty());
        Command.Register(new HelpEntry());
        OnPlayerConnectEvent.Register(OnlinePlayers.PlayerConnect,
                                      Priority.Critical);
        OnPlayerDisconnectEvent.Register(OnlinePlayers.PlayerDisconnect,
                                         Priority.Critical);
        OnPlayerChatEvent.Register(OnlinePlayers.Message, Priority.Low);
    }

    public override void Unload(bool shutdown) {
        Command.Unregister(Command.Find("party"));
        Command.Unregister(Command.Find("friend"));
        Command.Unregister(Command.Find("chat"));
        Command.Unregister(Command.Find("ac"));
        Command.Unregister(Command.Find("lc"));
        Command.Unregister(Command.Find("help"));
        OnPlayerConnectEvent.Unregister(OnlinePlayers.PlayerConnect);
        OnPlayerDisconnectEvent.Unregister(OnlinePlayers.PlayerDisconnect);
        OnPlayerChatEvent.Unregister(OnlinePlayers.Message);

        OnlinePlayers.Exit();
    }
}
