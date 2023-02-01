using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerColorChange : NetworkBehaviour
{
    [SerializeField] private float _damagedTimeDuration = 3f;

    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Color _defColor;
    [SerializeField] private Color _damagedColor;

    [SyncVar(hook = nameof(HandleColorUpdated))]
    private Color _curColor = new Color();

    public bool IsInvulnerable => _isInvulnerable;
    private bool _isInvulnerable = false;

    #region Server

    [Server]
    public void RecieveDamage()
    {
        if (_isInvulnerable)
            return;

        _curColor = _damagedColor;
        _isInvulnerable = true;
        StartCoroutine(DamagedTimeCooldown());
    }

    private IEnumerator DamagedTimeCooldown()
    {
        yield return new WaitForSeconds(_damagedTimeDuration);
        _curColor = _defColor;
        _isInvulnerable = false;
    }

    #endregion

    #region Client

    private void HandleColorUpdated(Color oldColor, Color newColor)
    {
        _renderer.material.color = newColor;
    }

    #endregion
}