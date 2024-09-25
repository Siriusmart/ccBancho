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
        get { return 2; }
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
        Games.OnLoad();

        Command.Register(new PartyEntry());
        Command.Register(new FriendEntry());
        Command.Register(new ChatEntry());
        Command.Register(new ChatGlobal());
        Command.Register(new ChatLocal());
        Command.Register(new ChatParty());
        Command.Register(new ChatMsg());
        Command.Register(new ChatReply());
        Command.Register(new HelpEntry());

        OnPlayerConnectEvent.Register(OnlinePlayers.PlayerConnect,
                                      Priority.Critical);
        OnPlayerDisconnectEvent.Register(OnlinePlayers.PlayerDisconnect,
                                         Priority.Critical);
        OnPlayerChatEvent.Register(OnlinePlayers.Message, Priority.Normal);
        OnPlayerSpawningEvent.Register(OnlinePlayers.PlayerSpawn,
                                       Priority.Normal);
    }

    public override void Unload(bool shutdown) {
        Games.OnUnload();

        Command.Unregister(Command.Find("party"));
        Command.Unregister(Command.Find("friend"));
        Command.Unregister(Command.Find("chat"));
        Command.Unregister(Command.Find("msg"));
        Command.Unregister(Command.Find("reply"));
        Command.Unregister(Command.Find("ac"));
        Command.Unregister(Command.Find("lc"));
        Command.Unregister(Command.Find("help"));
        OnPlayerConnectEvent.Unregister(OnlinePlayers.PlayerConnect);
        OnPlayerDisconnectEvent.Unregister(OnlinePlayers.PlayerDisconnect);
        OnPlayerChatEvent.Unregister(OnlinePlayers.Message);
        OnPlayerSpawningEvent.Unregister(OnlinePlayers.PlayerSpawn);

        OnlinePlayers.Exit();
    }
}
