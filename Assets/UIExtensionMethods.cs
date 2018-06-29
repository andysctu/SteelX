using UnityEngine;

public static class UIExtensionMethods{
    public static string BarValueToString(int curvalue, int maxvalue) {        
        string curvalueStr = (curvalue<0)? "0" : curvalue.ToString();
        string maxvalueStr = maxvalue.ToString();

        string finalStr = string.Empty;
        for (int i = 0; i < 4 - curvalueStr.Length; i++) {
            finalStr += "0 ";
        }

        for (int i = 0; i < curvalueStr.Length; i++) {
            finalStr += (curvalueStr[i] + " ");

        }
        finalStr += "/ ";

        for (int i = 0; i < 4 - maxvalueStr.Length; i++) {
            finalStr += "0 ";
        }

        for (int i = 0; i < maxvalueStr.Length - 1; i++) {
            finalStr += (maxvalueStr[i] + " ");
        }
        finalStr += maxvalueStr[maxvalueStr.Length - 1];

        return finalStr;
    }
}