using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] private float _playerSpeed;
    [SerializeField] private Transform _rotationTarget;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _sprintSpeed;

    private bool _isGrounded;
    private bool isSprinting;
    private PlayerAnimationController _playerAnimationController;
    private PlayerController _playerController;
    private PhotonView _photonView;
    private Camera _camera;
    private float currentSpeed;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _photonView = GetComponent<PhotonView>();
        _playerAnimationController = GetComponent<PlayerAnimationController>();
        _playerController = GetComponent<PlayerController>();

        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }
    }

    void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        if (_playerController.IsDead)
        {
            return;
        }

        var cameraTransform = _camera.transform;

        var verticalAxis = Input.GetAxisRaw("Vertical");
        var horizontalAxis = Input.GetAxisRaw("Horizontal");

        var fwdMove = cameraTransform.forward * verticalAxis;
        var rightMove = cameraTransform.right * horizontalAxis;

        if (Input.GetButton("Jump") && _isGrounded)
        {
            Debug.Log("Jump");
            _rigidbody.AddForce(cameraTransform.up * _jumpForce);
        }

        if ((Input.GetButton("Sprint")) && (Input.GetAxisRaw("Vertical") > 0.6))
        {
            currentSpeed = _sprintSpeed;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
            currentSpeed = 0.0f;
        }

        var moveVector = fwdMove + rightMove;
        moveVector.y = 0;
        moveVector.Normalize();
        moveVector *= _playerSpeed + currentSpeed;
        moveVector.y = _rigidbody.velocity.y;

        _rigidbody.velocity = moveVector;

        _playerAnimationController.IsSprinting = isSprinting;
        _playerAnimationController.IsMovingBackwardsOrForwards = verticalAxis != 0 || horizontalAxis != 0;
        _playerAnimationController.ForwardsSpeed = verticalAxis * _playerSpeed;
        _playerAnimationController.RightSpeed = horizontalAxis * _playerSpeed;
        _playerAnimationController.IsStrafing = horizontalAxis != 0 && verticalAxis == 0;
        _isGrounded = false;
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        var product = Vector3.Dot(collisionInfo.contacts[0].normal, Vector3.up);
        if (product > 0.5)
        {
            _isGrounded = true;
        }


    }
}
