using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInputActions playerInputs;

    public event EventHandler OnSpikeAbility;

    [SerializeField] private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    [SerializeField] private bool ultimatePressed;
    public bool UltimatePressed => ultimatePressed;

    [SerializeField] private bool spikeAbilityPressed;
    public bool SpikeAbilityPressed => spikeAbilityPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInputs = new PlayerInputActions();
        playerInputs.Gameplay.Enable();

        playerInputs.Gameplay.UseUltimate.performed += OnUltimateButtonPressed;
        playerInputs.Gameplay.SpikeAbility.performed += OnSpikeAbilityPressed;
    }

    private void OnEnable()
    {
        if (playerInputs != null)
            playerInputs.Gameplay.Enable();
    }

    private void OnDisable()
    {
        if (playerInputs != null)
            playerInputs.Gameplay.Disable();
    }

    private void OnUltimateButtonPressed(InputAction.CallbackContext callback)
    {
        ultimatePressed = true;
    }

    private void OnSpikeAbilityPressed(InputAction.CallbackContext callback)
    {
        spikeAbilityPressed = true;
        OnSpikeAbility?.Invoke(this, EventArgs.Empty);
    }

    public void ConsumeUltimate()
    {
        ultimatePressed = false;
    }

    public void ConsumeSpikeAbility()
    {
        spikeAbilityPressed = false;
    }

    public Vector2 GetMoveInput()
    {
        moveInput = playerInputs.Gameplay.Move.ReadValue<Vector2>();
        moveInput.Normalize();
        return moveInput;
    }
}