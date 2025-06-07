using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerSprinting : MonoBehaviour {
    [SerializeField] float speedMultiplier = 3f;

    private Player player;
    private PlayerInput playerInput;
    private InputAction sprintAction;

    private void Awake() {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["sprint"];
    }

    private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
    private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

    private void OnBeforeMove() {
        var sprintInput = sprintAction.ReadValue<float>();
        player.MovementSpeedMultiplier *= sprintInput > 0 ? speedMultiplier : 1f;
    }
}

