using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private CharacterController _controller;

    private Vector2 _prevInput;

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

    public Action<Vector3> OnPlayerMove;

    public override void OnStartAuthority()
    {
        enabled = true;

        PlayerInput.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        PlayerInput.Player.Move.canceled += ctx => ResetMovement();

        PlayersScoreContainer.OnGameEnded += OnGameEnded;
    }

    [ClientCallback]
    private void OnDestroy() => PlayersScoreContainer.OnGameEnded -= OnGameEnded;

    private void OnGameEnded()
    {
        PlayerInput.Disable();
    }

    [ClientCallback]
    private void OnEnable() => PlayerInput.Enable();

    [ClientCallback]
    private void OnDisable() => PlayerInput.Disable();

    [ClientCallback]
    private void Update() => OnMove();

    [Client]
    private void SetMovement(Vector2 move) => _prevInput = move;

    [Client]
    private void ResetMovement() => _prevInput = Vector2.zero;

    [Client]
    private void OnMove()
    {
        Vector3 right = _controller.transform.right;
        Vector3 forward = _controller.transform.forward;

        right.y = 0f;
        forward.y = 0f;

        Vector3 move = right.normalized * _prevInput.x + forward.normalized * _prevInput.y;

        _controller.Move(move * _speed * Time.deltaTime);

        OnPlayerMove?.Invoke(move);
    }
}