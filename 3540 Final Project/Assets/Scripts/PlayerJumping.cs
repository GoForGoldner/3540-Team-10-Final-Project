using UnityEngine;

namespace Common.Player {
    [RequireComponent(typeof(Player))]
    public class PlayerJumping : MonoBehaviour {
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float jumpPressBufferTime = 0.05f;
        [SerializeField] private float jumpGroundGraceTime = 0.2f;

        private Player player;

        private bool tryingToJump;
        private float lastJumpTime;
        private float lastGroundTime;
        private void Awake() {
            player = GetComponent<Player>();
        }

        private void OnEnable() {
            player.OnBeforeMove += OnBeforeMove;
            player.OnGroundStateChange += OnGroundStateChange;
        }

        private void OnDisable() {
            player.OnBeforeMove -= OnBeforeMove;
            player.OnGroundStateChange -= OnGroundStateChange;
        }

        private void OnJump() {
            tryingToJump = true;
            lastJumpTime = Time.time;
        }

        private void OnBeforeMove() {
            var wasTryingToJump = Time.time - lastJumpTime < jumpPressBufferTime;
            var wasGrounded = Time.time - lastGroundTime < jumpGroundGraceTime;

            var isOrWasTryingToJump = tryingToJump && (wasTryingToJump && player.IsGrounded);
            var isOrWasGrounded = player.IsGrounded || wasGrounded;

            if (isOrWasTryingToJump && isOrWasGrounded) {
                player.Velocity.y += jumpSpeed;
            }
            tryingToJump = false;
        }

        void OnGroundStateChange(bool isGrounded) {
            if (!isGrounded) lastGroundTime = Time.time;
        }
    }
}
