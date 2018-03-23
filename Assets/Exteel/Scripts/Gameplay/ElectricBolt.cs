using UnityEngine;
using System.Collections;

public class ElectricBolt : MonoBehaviour {

    private LineRenderer lRend;
    private Vector3[] points = new Vector3[5];

    private readonly int point_Begin = 0;
    private readonly int point_Middle_Left = 1;
    private readonly int point_Center = 2;
    private readonly int point_Middle_Right = 3;
    private readonly int point_End = 4;

	public Camera cam;
	public Transform Target = null;
    public Transform lineEnd = null;
	public Vector3 dir;

    private readonly float randomPosOffset = 1f;
    private readonly float randomWithOffsetMax = 8.5f;
    private readonly float randomWithOffsetMin = 7.5f;

    private readonly WaitForSeconds customFrame = new WaitForSeconds(0.05f);

    void Start () {
        lRend = GetComponent<LineRenderer>();
        StartCoroutine(Beam());
		Destroy (gameObject, 1.675f);
	}

    private IEnumerator Beam()
    {
		yield return customFrame;
		points[point_Begin] = transform.position + transform.forward*2.5f;
		if (Target != null) {
			lineEnd.position = Target.position + new Vector3(0,5f,0);
			points [point_End] = lineEnd.position;
		}else{
			points [point_End] = transform.position+cam.transform.forward*50f;
			lineEnd.position = points [point_End];
		}
		CalculateMiddle ();
        lRend.SetPositions(points);
        lRend.SetWidth(RandomWidthOffset(), RandomWidthOffset());
        StartCoroutine(Beam());
    }

    private float RandomWidthOffset()
    {
        return Random.Range(randomWithOffsetMin, randomWithOffsetMax);
    }

    private void CalculateMiddle()
    {
		Vector3 center = GetMiddleWithRandomness(transform.position, lineEnd.position);
        points[point_Center] = center;
        points[point_Middle_Left] = GetMiddleWithRandomness(transform.position, center);
		points[point_Middle_Right] = GetMiddleWithRandomness(center, lineEnd.position);
    }

    private Vector3 GetMiddleWithRandomness (Vector3 point1, Vector3 point2)
    {
        float x = (point1.x + point2.x) / point_Center;
        float finalX = Random.Range(x - randomPosOffset, x + randomPosOffset);
        float y = (point1.y + point2.y) / point_Center;
        float finalY = Random.Range(y - randomPosOffset, y + randomPosOffset); 
		float z = (point1.z + point2.z) / point_Center;
		float finalZ = Random.Range (z - randomPosOffset, z + randomPosOffset);
		return new Vector3(finalX, finalY, finalZ);
    }
}
