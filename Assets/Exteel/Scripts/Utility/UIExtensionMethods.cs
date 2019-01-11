using UnityEngine;

public static class UIExtensionMethods{
    public static string BarValueToString(int curvalue, int maxvalue) {        
        int maxvalue_length = (int)Mathf.Log10(maxvalue) + 1;

        string curvalueStr = (curvalue<0)? "0" : curvalue.ToString();
        string maxvalueStr = maxvalue.ToString();

        for (int i = 0; i < maxvalue_length - curvalueStr.Length; i++) {
            curvalueStr.Insert(0, "0");
        }

        return FillStringWithSpaces(curvalueStr + "/" + maxvalueStr);
    }

    public static string FillStringWithSpaces(string str) {
        string finalStr= "";
        for(int i = 0; i < str.Length-1; i++) {
            finalStr += str[i]+ " ";
        }
        finalStr += str[str.Length-1];

        return finalStr;
    }
}