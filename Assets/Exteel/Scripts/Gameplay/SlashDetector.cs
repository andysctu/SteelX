using System.Collections.Generic;
using UnityEngine;

public class SlashDetector : MonoBehaviour {
    [SerializeField] private MechCamera cam;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private MechController mctrl;

    private readonly List<IDamageable> _targets = new List<IDamageable>();
    private float _clampedCamAngleX;
    private float _clampAngle = 75;
    private float _mechMidpoint = 5;
    private float _clampAngleCoeff = 0.3f;//how much the cam angle affecting the y pos of box collider
    private float inAirStartZ = 14f;
    private Vector3 _inAirCenter = new Vector3(0, 0, 3.6f), _inAirSize = new Vector3(10, 18, 36);
    private Vector3 _onGroundCenter = new Vector3(0, 0, 2.5f), _onGroundSize = new Vector3(10, 11, 15);
    private bool _onOriginalPlace = false;

    private IDamageable _triggeredObj;

    private void Update() {
        if (!mctrl.Grounded) {
            _onOriginalPlace = false;
            _clampedCamAngleX = Mathf.Clamp(cam.GetCamAngle(), -_clampAngle, _clampAngle);
            transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, _mechMidpoint, transform.parent.localPosition.z);

            //set collider size
            SetCenter(new Vector3(_inAirCenter.x, _inAirCenter.y, inAirStartZ));
            SetSize(_inAirSize);
            SetLocalRotation(new Vector3(-_clampedCamAngleX, transform.parent.localRotation.eulerAngles.y, transform.parent.localRotation.eulerAngles.z));
        } else {
            if (!_onOriginalPlace) {
                _onOriginalPlace = true;
                transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, _mechMidpoint, transform.parent.localPosition.z);

                SetCenter(_onGroundCenter);
                SetSize(_onGroundSize);
                SetLocalRotation(Vector3.zero);
            }
        }
    }

    private void OnTriggerEnter(Collider target) {
        if((_triggeredObj = target.GetComponent(typeof(IDamageable)) as IDamageable) == null)return;

        if (_triggeredObj.IsEnemy(_triggeredObj.GetOwner())) {
            _targets.Add(_triggeredObj);
        }
    }

    private void OnTriggerExit(Collider target) {
        if ((_triggeredObj = target.GetComponent(typeof(IDamageable)) as IDamageable) == null) return;

        if (_triggeredObj.IsEnemy(_triggeredObj.GetOwner())) {
            _targets.Remove(_triggeredObj);
        }
    }

    private void SetLocalRotation(Vector3 v) {
        transform.parent.localRotation = Quaternion.Euler(v);
    }

    private void SetCenter(Vector3 v) {
        boxCollider.center = v;
    }

    private void SetSize(Vector3 v) {
        boxCollider.size = v;
    }

    public IDamageable[] GetCurrentTargets() {
        return _targets.ToArray();
    }

    public void EnableDetector(bool b) {
        boxCollider.enabled = b;
        enabled = b;

        _targets.Clear();
    }
}