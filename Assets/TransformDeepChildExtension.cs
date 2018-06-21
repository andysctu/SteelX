using UnityEngine;
using System.Collections;

public static class TransformDeepChildExtension {
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName , string ParentMustMatch = "") {
        var result = aParent.Find(aName);

        if (result != null && (ParentMustMatch == "" || ParentMustMatch == result.parent.name))
            return result;

        foreach (Transform child in aParent) {
            result = child.FindDeepChild(aName, ParentMustMatch);
            if (result != null) {
                return result;
            }
        }
        return null;
    }
}