using UnityEngine;
using Weapons.Bullets;

public class ElectricBolt : Bullet {
    [SerializeField] private Transform _lineEnd = null;
    private LineRenderer _lRend;
    private readonly Vector3[] _points = new Vector3[5];

    private readonly int point_Begin = 0;
    private readonly int point_Middle_Left = 1;
    private readonly int point_Center = 2;
    private readonly int point_Middle_Right = 3;
    private readonly int point_End = 4;

    private readonly float randomPosOffset = 1f;
    private readonly float randomWithOffsetMax = 4.5f;
    private readonly float randomWithOffsetMin = 3.5f;

    private bool _calledPlay = false;

    protected override void Awake(){
        base.Awake();

        InitComponents();
    }

    private void InitComponents(){
        _lRend = GetComponent<LineRenderer>();
    }

    protected override void LateUpdate() {
        if(!_calledPlay)return;
        _points[point_Begin] = transform.position;
        if (Target != null) {
            _lineEnd.position = Target.GetTransform().position + new Vector3(0, 5f, 0);
            _points[point_End] = _lineEnd.position;
        } else {
            _points[point_End] = transform.position + cam.transform.forward * 50f;
            _lineEnd.position = _points[point_End];
        }
        CalculateMiddle();
        _lRend.SetPositions(_points);
        _lRend.startWidth = RandomWidthOffset();
        _lRend.endWidth = RandomWidthOffset();
        //lRend.SetWidth(RandomWidthOffset(), RandomWidthOffset());
    }

    public override void Play() {
        _lRend.enabled = true;
        _calledPlay = true;
    }

    public override void StopBulletEffect() {
        _lRend.enabled = false;
        _calledPlay = false;
    }

    private float RandomWidthOffset() {
        return Random.Range(randomWithOffsetMin, randomWithOffsetMax);
    }

    private void CalculateMiddle() {
        Vector3 center = GetMiddleWithRandomness(transform.position, _lineEnd.position);
        _points[point_Center] = center;
        _points[point_Middle_Left] = GetMiddleWithRandomness(transform.position, center);
        _points[point_Middle_Right] = GetMiddleWithRandomness(center, _lineEnd.position);
    }

    private Vector3 GetMiddleWithRandomness(Vector3 point1, Vector3 point2) {
        float x = (point1.x + point2.x) / point_Center;
        float finalX = Random.Range(x - randomPosOffset, x + randomPosOffset);
        float y = (point1.y + point2.y) / point_Center;
        float finalY = Random.Range(y - randomPosOffset, y + randomPosOffset);
        float z = (point1.z + point2.z) / point_Center;
        float finalZ = Random.Range(z - randomPosOffset, z + randomPosOffset);
        return new Vector3(finalX, finalY, finalZ);
    }
}