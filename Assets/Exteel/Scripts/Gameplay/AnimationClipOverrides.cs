using System.Collections.Generic;
using UnityEngine;

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>> {
    public AnimationClipOverrides(int capacity) : base(capacity) {
    }

    public AnimationClip this[string name] {
        get {
            KeyValuePair<AnimationClip,AnimationClip> clipPair = this.Find(x => x.Key.name.Equals(name));
            if(clipPair.Value==null)return clipPair.Key;
            else return this.Find(x => x.Key.name.Equals(name)).Value;
        }
        set {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}