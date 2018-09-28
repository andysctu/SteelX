using UnityEngine;

namespace Weapons.Crosshairs.Data {
    [CreateAssetMenu(fileName = "RocketCrosshairData", menuName = "CrosshairData/RocketCrosshair", order = 0)]
    class RocketCrosshairData : CrosshairData {
        public GameObject MiddleCrossPrefab, TargetMarkPrefab;
    }
}