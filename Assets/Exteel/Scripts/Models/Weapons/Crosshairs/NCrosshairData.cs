using UnityEngine;

namespace Weapons.Crosshairs.Data {
    [CreateAssetMenu(fileName = "NCrosshair", menuName = "CrosshairData/NCrosshair", order = 0)]
    class NCrosshairData : CrosshairData{
        public GameObject MiddleCrossPrefab, TargetMarkPrefab;
        public GameObject LMarkPrefab, RMarkPrefab, LRedmarkPrefab, RRedmarkPrefab;
    }
}
