using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {
    #region Variables
    [Header("Attachments")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float mass = 3f;
    [SerializeField] private float acceleration = 20f;

    //State
    private State state;

    public State CurrentState {
        get => state;
        set {
            state = value;
            Velocity = Vector3.zero;
        }
    }

    public enum State {
        Walking,
        Climbing,
    }

    //Actions
    public event Action OnBeforeMove;
    public event Action<bool> OnGroundStateChange;

    //internals
    internal float MovementSpeedMultiplier;
    internal Vector3 Velocity;

    //getter
    public bool IsGrounded => controller.isGrounded;

    //private values
    private CharacterController controller;
    private Vector2 look;
    private bool wasGrounded;

    //input
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;

    public float Acceleration => acceleration;

    #endregion

    void Awake() {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
        lookAction = playerInput.actions["look"];
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        MovementSpeedMultiplier = 1;
        switch (CurrentState) {
            case State.Walking:
                UpdateGround();
                UpdateGravity();
                UpdateMovement();
                UpdateLook();
                break;
            case State.Climbing:
                UpdateLook();
                break;
        }
    }

    private void UpdateGround() {
        if (wasGrounded == IsGrounded) return;
        OnGroundStateChange?.Invoke(IsGrounded);
        wasGrounded = IsGrounded;
    }

    #region Movement
    void UpdateLook() {
        var lookInput = lookAction.ReadValue<Vector2>();
        look.x += lookInput.x * mouseSensitivity;
        look.y += lookInput.y * mouseSensitivity;

        look.y = Mathf.Clamp(look.y, -89f, 89f);

        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0, 0);
        transform.localRotation = Quaternion.Euler(0, look.x, 0);
    }

    // Make this public so PlayerClimbing can use it
    public Vector3 GetMovementInput(float speed, bool horizontal = true) {
        var moveInput = moveAction.ReadValue<Vector2>();
        var input = new Vector3();
        var referenceTransform = horizontal ? transform : cameraTransform;
        input += referenceTransform.forward * moveInput.y;
        input += referenceTransform.right * moveInput.x;
        input = Vector3.ClampMagnitude(input, 1f);
        input *= speed * MovementSpeedMultiplier;

        return input;
    }

    void UpdateMovement() {
        OnBeforeMove?.Invoke();

        var input = GetMovementInput(walkingSpeed);

        var factor = acceleration * Time.deltaTime;
        Velocity.x = Mathf.Lerp(Velocity.x, input.x, factor);
        Velocity.z = Mathf.Lerp(Velocity.z, input.z, factor);

        controller.Move(Velocity * Time.deltaTime);
    }

    void UpdateGravity() {
        var gravity = Physics.gravity * (mass * Time.deltaTime);
        Velocity.y = controller.isGrounded ? -1f : Velocity.y + gravity.y;
    }

    // Make this public so PlayerClimbing can use it
    public void Move(Vector3 motion) {
        controller.Move(motion);
    }
    #endregion
}

