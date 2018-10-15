using UnityEngine;

public class Sync : Photon.MonoBehaviour {
    [Tooltip("RootPv is owned by the player who instantiated this mech")]
    [SerializeField] private PhotonView _rootPv;
    [SerializeField] private Transform _mainCam;

    private Vector3 _trueLoc;
    private Quaternion _trueRot = Quaternion.identity, _camTrueRot;

    private void Update() {
        if (!PhotonNetwork.isMasterClient && !_rootPv.isMine) {
            transform.position = Vector3.Lerp(transform.position, _trueLoc, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, _trueRot, Time.deltaTime * 10);
            _mainCam.rotation = _camTrueRot;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isReading) {
            if (!PhotonNetwork.isMasterClient){
                this._trueLoc = (Vector3)stream.ReceiveNext();

                if (!_rootPv.isMine){
                    this._trueRot = (Quaternion)stream.ReceiveNext();
                    this._camTrueRot = (Quaternion)stream.ReceiveNext();
                } else{
                    stream.ReceiveNext();
                    stream.ReceiveNext();
                }
            }
        } else {
            if (PhotonNetwork.isMasterClient) {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(_mainCam.rotation);
            }
        }
    }
}