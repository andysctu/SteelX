using System.Text;

public static class UIExtensionMethods {
    private static StringBuilder builder = new StringBuilder();

    public static string BarValueToString(int curvalue, int maxvalue) {// 900,2000 -> 0 9 0 0 / 2 0 0 0 , 13000,13000 - > 1 3 0 0 0 / 1 3 0 0 0
        return FillStringWithSpaces(((curvalue < 0) ? "0" : curvalue.ToString()).PadLeft(4, '0') + "/" + maxvalue.ToString().PadLeft(4, '0'));
    }

    public static string FillStringWithSpaces(string str) {
        //Clear string builder
        builder.Length = 0;
        builder.Capacity = 0;

        for (int i = 0; i < str.Length - 1; i++) {
            builder.Append(str[i] + " ");
        }
        builder.Append(str[str.Length - 1]);

        return builder.ToString();
    }
}