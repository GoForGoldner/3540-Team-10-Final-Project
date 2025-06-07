using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour {
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;

    [SerializeField] private MouseLook m_MouseLook;

    [Header("")]
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();

    [Header("Head Bob Settings")]
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();

    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();

    [SerializeField, Tooltip("The rate at which footstep and head bobs are played")] private float m_StepInterval;

    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.


    public float yRotOffset { get { return m_MouseLook.yRotOffset; } set { m_MouseLook.yRotOffset = value; } }
    public float xRotOffset { get { return m_MouseLook.xRotOffset; } set { m_MouseLook.xRotOffset = value; } }


    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    private void Awake() {
        // Only one instance of the character
        DontDestroyOnLoad(this.gameObject);

        // Anything that requires "GetComponent" or getting something from the hierarchy at the begging should be here!
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_AudioSource = GetComponent<AudioSource>();
    }



    private void Start() {
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_MouseLook.Init(transform, m_Camera.transform);
    }

    private void Update() {
        // Handle camera rotation with the mouse
        RotateView();

        // Set jump to if the jump "key" is placed down
        if (!m_Jump) {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    private void FixedUpdate() {
        float speed;
        GetInput(out speed);
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded) {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump) {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();
    }

    // Plays the sound for landing on the group
    private void PlayLandingSound() {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    // Plays the sound for jumping in the air
    private void PlayJumpSound() {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    // Manages calling the footstep audio for when the player is walking
    private void ProgressStepCycle(float speed) {
        // If the player is moving and they have an input down
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0)) {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        // Leave if the player hasn't moved enough for a step
        if (!(m_StepCycle > m_NextStep)) {
            return;
        }

        // Increase the next step to be at the next interval (frequency of running sounds)
        m_NextStep = m_StepCycle + m_StepInterval;

        // Play the footstep audio
        PlayFootStepAudio();
    }

    // Plays the audio for the foot steps
    private void PlayFootStepAudio() {
        // Leave if the player isn't on the ground
        if (!m_CharacterController.isGrounded) {
            return;
        }

        // Pick a random audio clip from the footsteps
        int r = Random.Range(0, m_FootstepSounds.Length);

        // Play the audio source
        m_AudioSource.PlayOneShot(m_FootstepSounds[r]);
    }


    private void UpdateCameraPosition(float speed) {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob) {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded) {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed) {
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);

        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        if (m_Input.sqrMagnitude > 1) {
            m_Input.Normalize();
        }
    }


    private void RotateView() {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (m_CollisionFlags == CollisionFlags.Below) {
            return;
        }

        if (body == null || body.isKinematic) {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
