using System.Text;

public static class UIExtensionMethods {

    public static string BarValueToString(int curValue, int maxvalue) {// 900,2000 -> 0 9 0 0 / 2 0 0 0 , 13000,13000 - > 1 3 0 0 0 / 1 3 0 0 0
        return FillStringWithSpaces(((curValue < 0) ? "0" : curValue.ToString()).PadLeft(4, '0') + "/" + maxvalue.ToString().PadLeft(4, '0'));
    }

    public static string FillStringWithSpaces(string str) {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < str.Length - 1; i++) {
            builder.Append(str[i]);
            builder.Append(" ");
        }
        builder.Append(str[str.Length - 1]);

        return builder.ToString();
    }
}