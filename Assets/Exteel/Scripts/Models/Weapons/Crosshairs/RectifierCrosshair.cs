using UnityEngine;
using Weapons.Crosshairs.Data;

namespace Weapons.Crosshairs {
    class RectifierCrosshair : Crosshair {
        private GameObject _middleCross, _targetMark;
        private const float LerpMarkSpeed = 15;

        public override void Init(Transform parent, Camera Cam, int hand) {
            this.Cam = Cam;

            GetCrosshairData();

            RectifierCrosshairData data = (RectifierCrosshairData)CrosshairData;

            _middleCross = Object.Instantiate(data.MiddleCrossPrefab, parent);
            _targetMark = Object.Instantiate(data.TargetMarkPrefab, parent);
        }

        protected override void GetCrosshairData() {
            CrosshairData = Resources.Load<RectifierCrosshairData>("Data/Crosshairs/RectifierCrosshairData");
        }

        public override void EnableCrosshair(bool b) {
            _middleCross.transform.localPosition = Vector3.zero;
            _middleCross.SetActive(b);
            _targetMark.SetActive(false);
        }

        public override void OnTarget(bool onTarget) {
            _targetMark.SetActive(onTarget);
        }

        public override void MarkTarget(IDamageable target) {
            _targetMark.transform.position = Vector3.Lerp(_targetMark.transform.position, Cam.WorldToScreenPoint(target.GetPosition()), Time.deltaTime * LerpMarkSpeed);
            //_middleCross.transform.position = _targetMark.transform.position;
        }

        public override void Destroy() {
            base.Destroy();
            Object.Destroy(_middleCross);
            Object.Destroy(_targetMark);
        }
    }
}
