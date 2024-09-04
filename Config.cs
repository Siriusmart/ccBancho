using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using MCGalaxy;
using MCGalaxy.Config;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Modules.Relay;
using MCGalaxy.Network;

public sealed class BanchoConfig {
    private const string PATH = "properties/bancho.properties";
    private static ConfigElement[] cfg;

    [ConfigString("mongodb-address", "General", "mongodb://localhost:27017",
                  false)]
    public string MongoAddress = "mongodb://localhost:27017";
    [ConfigString("mongodb-name", "General", "ccBancho", false)]
    public string MongoName = "ccBancho";
    // cooldown between party invites/requests, also how long the invites expire
    [ConfigInt("invite-cooldown", "Cooldown", 60, 0)]
    public int InviteCooldown = 60;
    [ConfigInt("friend-cooldown", "Cooldown", 300, 0)]
    public int FriendCooldown = 300;
    // how long a party is allowed to exist without any member onlne
    [ConfigInt("party-life", "Social", 300, 0)]
    public int PartyLife = 300;
    [ConfigInt("friend-page-size", "Social", 10, 0)]
    public int FriendPageSize = 10;
    [ConfigInt("message-reply-timeout", "Social", 300, 0)]
    public int MessageReplyTimeOut = 300;

    [ConfigString("main-level", "Game", "main", false)]
    public string MainLevel = "main";
    // how long between checking for new maps for games
    [ConfigInt("map-update-interval", "Game", 300, 0)]
    public int MapUpdateInterval = 300;

    public void Load() {
        // create default config file
        if (!File.Exists(PATH))
            Save();

        if (cfg == null)
            cfg = ConfigElement.GetAll(typeof(BanchoConfig));
        ConfigElement.ParseFile(cfg, PATH, this);
    }

    public void Save() {
        if (cfg == null)
            cfg = ConfigElement.GetAll(typeof(BanchoConfig));

        using (StreamWriter w = new StreamWriter(PATH)) {
            ConfigElement.Serialise(cfg, w, this);
        }
    }
}
