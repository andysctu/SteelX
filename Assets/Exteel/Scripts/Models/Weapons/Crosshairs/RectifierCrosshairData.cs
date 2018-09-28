using UnityEngine;

namespace Weapons.Crosshairs.Data {
    [CreateAssetMenu(fileName = "RectifierCrosshairData", menuName = "CrosshairData/RectifierCrosshair", order = 0)]
    class RectifierCrosshairData : CrosshairData {
        public GameObject MiddleCrossPrefab, TargetMarkPrefab;
    }
}
