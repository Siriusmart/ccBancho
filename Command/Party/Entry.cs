using MCGalaxy;

public sealed class PartyEntry : Command {
    public override string name {
        get { return "party"; }
    }
    public override string type {
        get { return "social"; }
    }

    public static Type? GetSubcommand(string command) {
        switch (command) {
        case "create":
            return typeof(PartyCreate);
        case "invite":
            return typeof(PartyInvite);
        case "join":
            return typeof(PartyJoin);
        case "list":
            return typeof(PartyList);
        case "promote":
            return typeof(PartyPromote);
        case "demote":
            return typeof(PartyDemote);
        case "leave":
            return typeof(PartyLeave);
        case "kick":
            return typeof(PartyKick);
        case "disband":
            return typeof(PartyDisband);
        default:
            return null;
        }
    }

    public override void Use(Player p, string message) {
        if (message == string.Empty) {
            p.MessageLines(Formatter
                               .BarsWrap(@$"&aParty Commands:
&e{Subcommand.FormatText("party",PartyCreate.Name(), PartyCreate.Format())} &7- &b{PartyCreate.Description()}
&e{Subcommand.FormatText("party",PartyInvite.Name(), PartyInvite.Format())} &7- &b{PartyInvite.Description()}
&e{Subcommand.FormatText("party",PartyJoin.Name(), PartyJoin.Format())} &7- &b{PartyJoin.Description()}
&e{Subcommand.FormatText("party",PartyList.Name(), PartyList.Format())} &7- &b{PartyList.Description()}
&e{Subcommand.FormatText("party",PartyPromote.Name(), PartyPromote.Format())} &7- &b{PartyPromote.Description()}
&e{Subcommand.FormatText("party",PartyDemote.Name(), PartyDemote.Format())} &7- &b{PartyDemote.Description()}
&e{Subcommand.FormatText("party",PartyLeave.Name(), PartyLeave.Format())} &7- &b{PartyLeave.Description()}
&e{Subcommand.FormatText("party",PartyKick.Name(), PartyKick.Format())} &7- &b{PartyKick.Description()}
&e{Subcommand.FormatText("party",PartyDisband.Name(), PartyDisband.Format())} &7- &b{PartyDisband.Description()}")
                               .Split("\n"));
            return;
        }

        string[] args = message.SplitSpaces();
        Type? command = GetSubcommand(args[0]);
        object[] parameters = { p, args[1..] };

        if (command == null) {
            parameters[1] = args;
            command = typeof(PartyInvite);
        }

        bool res = (bool)command.GetMethod("Run").Invoke(null, parameters);

        if (!res) {
            p.Message(
                $"&cIncorrect usage: {Subcommand.FormatText("party",(string)command.GetMethod("Name").Invoke(null, null),(string?)command.GetMethod("Format").Invoke(null, null))}");
        }
    }

    public override void Help(Player p) {
        p.Message("&e/party <args..> &7- &bManage game parties.");
    }
}
