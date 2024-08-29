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

    [ConfigString("mongodb-address", "General", "mongodb://localhost:27017")]
    public string MongoAddress = "mongodb://localhost:27017";
    [ConfigString("mongodb-name", "General", "ccBancho")]
    public string MongoName = "ccBancho";
    [ConfigInt("invite-cooldown", "Cooldown", 60)]
    public int InviteCooldown = 60;

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
