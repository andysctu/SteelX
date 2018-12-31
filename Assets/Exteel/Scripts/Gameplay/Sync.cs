using UnityEngine;

public class Sync : Photon.MonoBehaviour {
    [SerializeField] private Transform _mainCam;

    private Vector3 _trueLoc;
    private Quaternion _trueRot = Quaternion.identity, _camTrueRot;

    private PhotonPlayer _owner;

    public void Init(PhotonPlayer owner){
        _owner = owner;
    }

    private void Update() {
        if (!PhotonNetwork.isMasterClient && _owner != null && !_owner.IsLocal) {
            transform.position = Vector3.Lerp(transform.position, _trueLoc, Time.deltaTime * 10); //TODO : pass curPos in HandleInputs
            transform.rotation = Quaternion.Lerp(transform.rotation, _trueRot, Time.deltaTime * 10);
            _mainCam.rotation = _camTrueRot;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isReading) {
            if (!PhotonNetwork.isMasterClient){
                if(_owner==null)return;

                this._trueLoc = (Vector3)stream.ReceiveNext();

                if (!_owner.IsLocal){
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