using UnityEngine;
using Weapons.Crosshairs.Data;

namespace Weapons.Crosshairs {
    class NCrosshair : Crosshair{
        private GameObject _middleCross, _targetMark, _LRmark, _LRRedMark;
        private const float LerpMarkSpeed = 15;

        public override void Init(Transform parent, Camera _cam, int hand) {
            base.Init(parent, _cam, hand);

            NCrosshairData data = (NCrosshairData)CrosshairData;

            _middleCross = Object.Instantiate(data.MiddleCrossPrefab, parent);
            _targetMark = Object.Instantiate(data.TargetMarkPrefab, parent);

            _LRmark = Object.Instantiate((hand == 0) ? data.LMarkPrefab : data.RMarkPrefab, (hand == 0) ? crosshair.transform.GetChild(2) : crosshair.transform.GetChild(3));
            _LRRedMark = Object.Instantiate((hand == 0) ? data.LRedmarkPrefab : data.RRedmarkPrefab, (hand == 0) ? redCrosshair.transform.GetChild(2) : redCrosshair.transform.GetChild(3));
        }

        protected override void GetCrosshairData(){
            CrosshairData = Resources.Load<NCrosshairData>("Data/Crosshairs/NCrosshairData");
        }

        public override void SetRadius(float _radius) {
            this.Radius = _radius * RadiusCoeff;
            OrgRadius = _radius * RadiusCoeff;
            SetOffset(_radius * RadiusCoeff);
        }

        protected override void SetOffset(float _radius) {
            crosshairRect.offsetMin = new Vector2(-_radius, -_radius);
            crosshairRect.offsetMax = new Vector2(_radius, _radius);

            crosshairRedRect.offsetMin = new Vector2(-_radius, -_radius);
            crosshairRedRect.offsetMax = new Vector2(_radius, _radius);
        }

        public override void Update() {
            if (IsShaking) {//TODO : implement this
                //Setoffset(_radius);
                //_radius = Mathf.Lerp(_radius, _orgRadius, 0.05f);
            }
        }

        public override void EnableCrosshair(bool b) {
            base.EnableCrosshair(b);

            //_LRmark.SetActive(b);
            //_LRRedMark.SetActive(false);

            _middleCross.transform.localPosition = Vector3.zero;
            _middleCross.SetActive(b);
            _targetMark.SetActive(false);
        }

        public override void OnTarget(bool onTarget) {
            base.OnTarget(onTarget);

            //_middleCross.SetActive(onTarget);
            if(!onTarget) _middleCross.transform.localPosition = Vector3.zero;
            _targetMark.SetActive(onTarget);
        }

        public override void MarkTarget(Transform target) {
            _targetMark.transform.position = Vector3.Lerp(_targetMark.transform.position, Cam.WorldToScreenPoint(target.transform.position + new Vector3(0, 5, 0)), Time.deltaTime * LerpMarkSpeed);
            _middleCross.transform.position = _targetMark.transform.position;
        }

        public override void Destroy() {
            base.Destroy();
            Object.Destroy(_middleCross);
            Object.Destroy(_targetMark);
            Object.Destroy(_LRmark);
            Object.Destroy(_LRRedMark);
        }
    }
}
