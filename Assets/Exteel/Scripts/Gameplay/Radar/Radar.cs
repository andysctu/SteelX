using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Radar : MonoBehaviour {
    private GameObject RadarImage;
    private Camera radarCam;
    private PhotonView pv;
    private Transform radarCam_transform;
    private float radius;
    private List<RadarElement> radarElements = new List<RadarElement>();
    private List<RadarElement> elementsToRemove = new List<RadarElement>();

    private Vector3 radarElemnet_proj, transform_proj;
    private RenderTexture RadarTexture;
    private int RawImageSize = 310;

    private void Awake() {
        pv = transform.root.GetComponent<PhotonView>();
        if (!pv.isMine){
            gameObject.SetActive(false);
            return;
        }

        radarCam = GetComponentInChildren<Camera>();

        radarCam.enabled = pv.isMine;
        enabled = pv.isMine;

        radarCam_transform = radarCam.transform;
        radius = radarCam.orthographicSize;
    }

    private void Start() {
        if (!pv.isMine) return;

        AssignRadarTextureToCanvas();
    }

    private void AssignRadarTextureToCanvas() {
        RadarTexture = new RenderTexture(RawImageSize, RawImageSize, 0, RenderTextureFormat.ARGB32);
        RadarTexture.Create();
        radarCam.targetTexture = RadarTexture;
        RawImage RadarOnPanelCanvas = GameObject.Find("PanelCanvas/Radar").GetComponentInChildren<RawImage>();
        RadarOnPanelCanvas.texture = RadarTexture;
    }

    public void RegisterRadarElement(RadarElement radarElement) {
        radarElement.transform.localScale *= radius/100;
        radarElements.Add(radarElement);
    }

    private void LateUpdate() {
        //Update radar elements position
        for(int i = 0; i < radarElements.Count; i++) {
            if (radarElements[i] == null) {
                elementsToRemove.Add(radarElements[i]);
                continue;
            }

            radarElemnet_proj = radarElements[i].transform.parent.position - Vector3.up * radarElements[i].transform.parent.position.y;
            transform_proj = transform.position - Vector3.up * transform.position.y;

            if (Vector3.Distance(radarElemnet_proj, transform_proj) > radius) {
                //border element
                radarElements[i].transform.position = transform_proj + (radarElemnet_proj - transform_proj).normalized * radius;
            }
            //else do nothing
        }

        if(elementsToRemove.Count > 0) {
            foreach(RadarElement re in elementsToRemove) {
                radarElements.Remove(re);
            }
            elementsToRemove.Clear();
        }
    }
}