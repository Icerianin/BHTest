using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private Vector2 _maxFollowOffset = new Vector2(-1f, 6f);
    [SerializeField] private Vector2 _cameraVelocity = new Vector2(6f, 1f);

    [SerializeField] private Transform _player;
    [SerializeField] private CinemachineVirtualCamera _camera;

    private PlayerInput _playerInput;
    private PlayerInput PlayerInput
    {
        get
        {
            if (_playerInput != null)
                return _playerInput;
            return _playerInput = new PlayerInput();
        }
    }

    private CinemachineTransposer _transposer;

    public override void OnStartAuthority()
    {
        _transposer = _camera.GetCinemachineComponent<CinemachineTransposer>();

        _camera.gameObject.SetActive(true);

        enabled = true;

        PlayerInput.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
    }

    [ClientCallback]
    private void OnEnable() => PlayerInput.Enable();

    [ClientCallback]
    private void OnDisable() => PlayerInput.Disable();

    [Client]
    private void Look(Vector2 lookAxis)
    {
        _transposer.m_FollowOffset.y = Mathf.Clamp(
            _transposer.m_FollowOffset.y - (lookAxis.y * _cameraVelocity.y * Time.deltaTime),
            _maxFollowOffset.x,
            _maxFollowOffset.y);

        if (_player)
            _player.Rotate(0f, lookAxis.x * _cameraVelocity.x * Time.deltaTime, 0f);
    }
}