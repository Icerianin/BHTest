using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerDash : NetworkBehaviour
{
    [Header("Settings")]
    [Tooltip("Dash distance = DashPower * DashDuration")]
    [SerializeField] private float _dashPower = 15f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashColdownTime = 0.4f;

    [Header("Components")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private PlayerMovement _playerMovement;

    private float _curDashTime;
    private float _curDashColdownTime;

    private int _curHits = 0;

    private Vector3 _moveVector;
    private Vector3 _dashVector;

    private bool _isDashing => _curDashTime < _dashDuration;
    private bool _dashIsOnCooldown => _curDashColdownTime < _dashColdownTime;

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

    public Action<int> OnScoreChanged;

    public override void OnStartAuthority()
    {
        enabled = true;

        PlayerInput.Player.Ability.performed += ctx => StartDash();

        _curDashTime = _dashDuration;

        _curHits = 0;

        PlayersScoreContainer.OnGameEnded += OnGameEnded;
    }

    [ClientCallback]
    private void OnDestroy() => PlayersScoreContainer.OnGameEnded -= OnGameEnded;

    private void OnGameEnded()
    {
        PlayerInput.Disable();
    }

    #region Server

    [Command]
    public void TryToDamage(PlayerColorChange playerColor)
    {
        if (playerColor.IsInvulnerable)
            return;

        _curHits++;
        OnScoreChanged?.Invoke(_curHits);

        playerColor.RecieveDamage();
    }

    #endregion

    #region Client

    [ClientCallback]
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isLocalPlayer && _isDashing && hit.gameObject.TryGetComponent(out PlayerColorChange playerColor))
        {
            TryToDamage(playerColor);
        }
    }

    [ClientCallback]
    private void OnEnable()
    {
        PlayerInput.Enable();
        _playerMovement.OnPlayerMove += OnUpdateMoveVector;
    }

    [ClientCallback]
    private void OnDisable()
    {
        PlayerInput.Disable();
        _playerMovement.OnPlayerMove -= OnUpdateMoveVector;
    }

    [ClientCallback]
    private void OnUpdateMoveVector(Vector3 moveVector)
    {
        _moveVector = moveVector;
    }

    [ClientCallback]
    private void Update() => OnMove();

    [Client]
    private void StartDash()
    {
        if (_dashIsOnCooldown)
            return;

        _curDashTime = 0f;
        _curDashColdownTime = 0f;

        if (_moveVector == Vector3.zero)
            _dashVector = transform.forward;
        else
            _dashVector = _moveVector;
    }

    [Client]
    private void OnMove()
    {
        if (_isDashing)
        {
            DoDash();
        }
        else if (_dashIsOnCooldown)
        {
            _curDashColdownTime += Time.deltaTime;
        }
    }

    [Client]
    private void DoDash()
    {
        _curDashTime += Time.deltaTime;

        _controller.Move(_dashVector * Time.deltaTime * _dashPower);
    }

    #endregion
}