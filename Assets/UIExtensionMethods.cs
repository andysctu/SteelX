public static class UIExtensionMethods{
    public static string BarValueToString(int curvalue, int maxvalue) {
        string curvalueStr = curvalue.ToString();
        string maxvalueStr = maxvalue.ToString();

        string finalStr = string.Empty;
        for (int i = 0; i < 4 - curvalueStr.Length; i++) {
            finalStr += "0 ";
        }

        for (int i = 0; i < curvalueStr.Length; i++) {
            finalStr += (curvalueStr[i] + " ");

        }
        finalStr += "/ ";
        for (int i = 0; i < 3; i++) {
            finalStr += (maxvalueStr[i] + " ");
        }
        finalStr += maxvalueStr[3];

        return finalStr;
    }
}