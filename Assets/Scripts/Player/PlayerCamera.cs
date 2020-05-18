using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour, IPunObservable
{
    [SerializeField] private Transform pivot;
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _verticalSpeed = 0.05f;
    [SerializeField] private Transform _rotationTarget;
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private Transform _camera;
    [SerializeField] private Vector2 _cameraYOffsetBounds = new Vector2(-1.5f, 2.5f);
    public float shakeDuration;
    public float shakeAmount = 10f;
    public float decreaseFactor = 0.2f;
    public Vector3 originalPos;
    public bool bShake;
    private Quaternion camRot;

    private PhotonView _photonView;

    internal Transform EyesCamera
        => _camera;

    internal Transform RotationTarget
        => _rotationTarget;

    private float _yOffset = 0;
    private float _startY = 0;

    private Quaternion _networkedTargetRotation = Quaternion.identity;
    private float _angleBetweenRotations;

    void Awake()
    {

        _startY = transform.localPosition.y;
        _photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        bool _isDead = gameObject.GetComponent<PlayerController>().IsDead;
        if (Input.GetKeyDown(KeyCode.M))
        {
            shakeDuration = 10.0f;
            bShake = true;
        }
        if (!_photonView.IsMine)
        {
            // If it's not ours, then just interpolate towards where we're supposed to be facing
            _rotationTarget.localRotation = Quaternion.RotateTowards(_rotationTarget.localRotation,
                                                                    _networkedTargetRotation,
                                                                    _angleBetweenRotations * (1.0f / PhotonNetwork.SerializationRate));
            return;
        }
        var yChange = Input.GetAxis("Mouse Y") * -_verticalSpeed;
        Cursor.lockState = CursorLockMode.Locked;   // keep confined to center of screen
        _yOffset += yChange;
        _yOffset = Mathf.Clamp(_yOffset, _cameraYOffsetBounds.x, _cameraYOffsetBounds.y);

        _rotationTarget.localRotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _horizontalSpeed, 0);
        pivot.localRotation *= Quaternion.Euler(yChange, 0, 0);

        // stop the camera from flipping
        var xEuler = pivot.localRotation.eulerAngles.x;
        while (xEuler < 0)
        {
            xEuler += 360;
        }

        if (xEuler > 52 && xEuler < 70)
        {
            pivot.localRotation = Quaternion.Euler(52, 0, 0);
        }
        else if (xEuler < 320 && xEuler > 270)
        {
            pivot.localRotation = Quaternion.Euler(320, 0, 0);
        }

        camRot = _camera.rotation;
        if (shakeDuration > 0 && bShake && !_isDead)
        {
            StartCoroutine(shake());
            shakeDuration = -decreaseFactor * Time.deltaTime;
        }
        else if (bShake && !_isDead)
        {
            bShake = false;
            originalPos = _camera.transform.position;
            _camera.LookAt(_lookAtTarget);
        }
        else if (_isDead)
        {
            _camera.LookAt(transform.position);
        }
        else
        {
            _camera.LookAt(_lookAtTarget);
        }

        //Debug.Log(_camera.rotation);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_rotationTarget.localRotation);
        }
        else
        {
            _networkedTargetRotation = (Quaternion)stream.ReceiveNext();
            _angleBetweenRotations = Quaternion.Angle(_rotationTarget.localRotation, _networkedTargetRotation);
        }
    }

    public IEnumerator shake()
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-10f, 10f) * shakeAmount;
            float y = Random.Range(-10f, 10f) * shakeAmount;
            float z = Random.Range(-10f, 10f) * shakeAmount;

            _camera.transform.Rotate(x, y, z);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = orignalPosition;
    }

}
