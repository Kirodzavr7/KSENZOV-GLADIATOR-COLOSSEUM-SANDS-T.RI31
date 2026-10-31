using System;
using UnityEngine;

[RequireComponent(typeof(SimpleFighter))]
public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private Transform movementCamera;
    [SerializeField, Min(0f)] private float rotationSpeed = 12f;

    private SimpleFighter _fighter;
    private GameInput _gameInput;
    private bool _subscribed;

    private void Awake()
    {
        _fighter = GetComponent<SimpleFighter>();
        if (movementCamera == null && Camera.main != null)
            movementCamera = Camera.main.transform;
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        CoreCombatEvents.Died += HandleFighterDied;
        TrySubscribe();
    }

    private void OnDisable()
    {
        CoreCombatEvents.Died -= HandleFighterDied;
        Unsubscribe();
    }

    private void Update()
    {
        // Поддерживает порядок запуска, при котором GameInput создаётся позже игрока.
        TrySubscribe();
    }

    private void FixedUpdate()
    {
        if (_fighter == null || _fighter.IsDead || GameInput.Instance == null)
            return;

        Vector2 input = GameInput.Instance.GetMovementVector();
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);
        if (inputDir == Vector3.zero)
        {
            _fighter.Move(Vector3.zero, rotationSpeed);
            return;
        }

        Transform cameraTransform = movementCamera != null ? movementCamera : Camera.main != null ? Camera.main.transform : null;
        Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 cameraRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;
        _fighter.Move(moveDirection, rotationSpeed);
    }

    private void TrySubscribe()
    {
        if (_subscribed || GameInput.Instance == null)
            return;

        _gameInput = GameInput.Instance;
        _gameInput.OnPlayerAttack += HandlePlayerAttack;
        _gameInput.OnBlockStarted += HandleBlockStarted;
        _gameInput.OnBlockCanceled += HandleBlockCanceled;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _gameInput == null)
            return;

        _gameInput.OnPlayerAttack -= HandlePlayerAttack;
        _gameInput.OnBlockStarted -= HandleBlockStarted;
        _gameInput.OnBlockCanceled -= HandleBlockCanceled;
        _subscribed = false;
        _gameInput = null;
    }

    private void HandlePlayerAttack(object sender, EventArgs e)
    {
        if (_fighter != null && !_fighter.IsDead)
            _fighter.TryAttack();
    }

    private void HandleBlockStarted(object sender, EventArgs e)
    {
        if (_fighter != null && !_fighter.IsDead)
            _fighter.SetBlocking(true);
    }

    private void HandleBlockCanceled(object sender, EventArgs e)
    {
        if (_fighter != null && !_fighter.IsDead)
            _fighter.SetBlocking(false);
    }

    private void HandleFighterDied(SimpleFighter deadFighter)
    {
        if (deadFighter != _fighter || deadFighter.Team != FighterTeam.Player)
            return;

        if (GameInput.Instance != null)
            GameInput.Instance.DisableGameplayInput();

        Unsubscribe();
    }
}
