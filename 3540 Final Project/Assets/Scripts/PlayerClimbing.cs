using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerClimbing : MonoBehaviour {
    [SerializeField] private float climbingSpeed = 2f;

    private Player player;

    private void Awake() {
        player = GetComponent<Player>();
    }

    public void StartClimbing() {
        player.CurrentState = Player.State.Climbing;
    }

    public void StopClimbing() {
        player.CurrentState = Player.State.Walking;
    }

    private void UpdateMovementClimbing() {
        var input = player.GetMovementInput(climbingSpeed, false);
        var forwardInputFactor = Vector3.Dot(transform.forward, input.normalized);

        if (forwardInputFactor > 0) {
            input.x = input.x * 0.5f;
            input.z = input.z * 0.5f;

            if (Mathf.Abs(input.y) > 0.2f) {
                input.y = Mathf.Sign(input.y) * climbingSpeed;
            }
        }
        else {
            input.y = 0;
            input.x *= 3f;
            input.z *= 3f;
        }

        var factor = player.Acceleration * Time.deltaTime;
        player.Velocity = Vector3.Lerp(player.Velocity, input, factor);

        player.Move(player.Velocity * Time.deltaTime);
    }

    private void Update() {
        if (player.CurrentState == Player.State.Climbing) {
            UpdateMovementClimbing();
        }
    }
}