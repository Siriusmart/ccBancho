public class Formatter {
    private static string bar =
        "&9-----------------------------------------------------";
    private static string darkGreenBar =
        "&2-----------------------------------------------------";
    private static string purpleBar =
        "&5-----------------------------------------------------";

    public static string BarsWrap(string content) {
        return $"{bar}\n{content}\n{bar}";
    }

    public static string FriendBarsWrap(string content) {
        return $"{darkGreenBar}\n{content}\n{darkGreenBar}";
    }

    public static string GameBarsWrap(string content) {
        return $"{purpleBar}\n{content}\n{purpleBar}";
    }

    private static (string, long)[] units = { ("week", 604800), ("day", 86400),
                                              ("hour", 3600), ("minute", 60),
                                              ("second", 1) };

    public static string Duration(long seconds, int precesion) {

        List<string> output = new List<string>();

        for (int i = 0; i < units.Length; i++) {
            var entry = units[i];
            long amount = seconds / entry.Item2;
            seconds %= entry.Item2;

            switch (amount) {
            case 0:
                continue;
            case 1:
                if (seconds == 0 && output.Count == 0 && amount == 1 &&
                    i != units.Length - 1) {
                    return $"{entry.Item2 / units[i+1].Item2} {units[i+1].Item1}s";
                }

                output.Add($"{amount} {entry.Item1}");
                break;
            default:
                output.Add($"{amount} {entry.Item1}s");
                break;
            }
        }

        output = output.Slice(0, Math.Min(precesion, output.Count));

        string outputString = string.Empty;

        for (int i = 0; i < output.Count; i++) {
            if (i == 0) {
                outputString += output[i];
                continue;
            }

            if (i == output.Count - 1) {
                outputString += $" and {output[i]}";
                continue;
            }

            outputString += $", {output[i]}";
        }

        return outputString;
    }

    public static string ListItems(object[] list) {
        switch (list.Length) {
        case 0:
            return string.Empty;
        case 1:
            return $"{list[0]}";
        default:
            return $"{String.Join(", ", list[..(list.Length - 1)])} and {list.Last()}";
        }
    }
}
