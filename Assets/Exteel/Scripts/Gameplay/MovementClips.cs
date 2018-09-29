using UnityEngine;

[CreateAssetMenu(fileName = "MovementClips", menuName = "MovementClips")]
public class MovementClips : ScriptableObject {

    public string[] clipnames = {"Idle",
        "BackWalk",
        "BackWalk_Left",
        "BackWalk_Right",
        "Run_Left",
        "Run_Front",
        "Run_Right",

        "Hover_Back_01",
        "Hover_Back_02",
        "Hover_Back_03",
        "Hover_Back_01_Left",
        "Hover_Back_02_Left",
        "Hover_Back_03_Left",
        "Hover_Back_01_Right",
        "Hover_Back_02_Right",
        "Hover_Back_03_Right",

        "Hover_Left_01",
        "Hover_Left_02",
        "Hover_Left_03",
        "Hover_Right_01",
        "Hover_Right_02",
        "Hover_Right_03",
        "Hover_Front_01",
        "Hover_Front_02",
        "Hover_Front_03",

        "Jump01",
        "Jump01_Left",
        "Jump01_Right",
        "Jump01_Back_Left",
        "Jump01_Back_Right",

        "Jump01_b",
        "Jump01_Left_b",
        "Jump01_Right_b",
        "Jump01_Back_Left_b",
        "Jump01_Back_Right_b",

        "Jump02",
        "Jump02_Left",
        "Jump02_Right",
        "Jump02_Back_Left",
        "Jump02_Back_Right",

        "Jump03",
        "Jump03_Left",
        "Jump03_Right",
        "Jump03_Back_Left",
        "Jump03_Back_Right",

        "Jump06",
        "Jump06_Left",
        "Jump06_Right",
        "Jump06_Back_Left",
        "Jump06_Back_Right",

        "Jump07",
        "Jump07_Left",
        "Jump07_Right",
        "Jump07_Back_Left",
        "Jump07_Back_Right",

        "Jump08",
        "Jump08_Left",
        "Jump08_Right",
        "Jump08_Back_Left",
        "Jump08_Back_Right"
    };

#if UNITY_EDITOR
    [NamedArrayAttribute(new string[] {"Idle",
        "BackWalk",
        "BackWalk_Left",
        "BackWalk_Right",
        "Run_Left",
        "Run_Front",
        "Run_Right",

        "Hover_Back_01",
        "Hover_Back_02",
        "Hover_Back_03",
        "Hover_Back_01_Left",
        "Hover_Back_02_Left",
        "Hover_Back_03_Left",
        "Hover_Back_01_Right",
        "Hover_Back_02_Right",
        "Hover_Back_03_Right",

        "Hover_Left_01",
        "Hover_Left_02",
        "Hover_Left_03",
        "Hover_Right_01",
        "Hover_Right_02",
        "Hover_Right_03",
        "Hover_Front_01",
        "Hover_Front_02",
        "Hover_Front_03",

        "Jump01",
        "Jump01_Left",
        "Jump01_Right",
        "Jump01_Back_Left",
        "Jump01_Back_Right",

        "Jump01_b",
        "Jump01_Left_b",
        "Jump01_Right_b",
        "Jump01_Back_Left_b",
        "Jump01_Back_Right_b",

        "Jump02",
        "Jump02_Left",
        "Jump02_Right",
        "Jump02_Back_Left",
        "Jump02_Back_Right",

        "Jump03",
        "Jump03_Left",
        "Jump03_Right",
        "Jump03_Back_Left",
        "Jump03_Back_Right",

        "Jump06",
        "Jump06_Left",
        "Jump06_Right",
        "Jump06_Back_Left",
        "Jump06_Back_Right",

        "Jump07",
        "Jump07_Left",
        "Jump07_Right",
        "Jump07_Back_Left",
        "Jump07_Back_Right",

        "Jump08",
        "Jump08_Left",
        "Jump08_Right",
        "Jump08_Back_Left",
        "Jump08_Back_Right"
    })]
#endif
    public AnimationClip[] clips = new AnimationClip[60];
}