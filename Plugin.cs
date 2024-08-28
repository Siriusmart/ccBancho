using MCGalaxy;

public sealed class Bancho : Plugin {
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
        Command.Register(new PartyEntry());
    }

    public override void Unload(bool shutdown) {
        Command.Unregister(Command.Find("party"));
    }
}
