using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour

    {

        private AudioSource m_audioSource;


        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
            public float CrouchSpeed = 3.0f; // Crouch walk speed
	        public KeyCode RunKey = KeyCode.LeftShift;
            public KeyCode CrouchKey = KeyCode.X;
            public float JumpForce = 30f;

            
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running, m_Crouching;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
                if (Input.GetKey(CrouchKey))
                {
                    CurrentTargetSpeed = CrouchSpeed;
                    m_Crouching = true;
                }
	            else if (Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
                }
	            else
	            {
		            m_Running = false;
                    m_Crouching = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.1f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public float crouchDistance = 0.2f; // default crouch length
            public bool airControl = true; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset = 0.1f; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        [Serializable]
        public class SoundSetting
        {
            public AudioClip m_Footsteps;
            public AudioClip m_Jumping;
            public AudioClip m_Landing;
            public AudioClip m_smallOrbAudio;
            public AudioClip m_mediumOrbAudio;
            public AudioClip m_bigOrbAudio;

        }

        private int maxScore;
        private int score;
        public Text scoreText;
        private float time;
        public Text timerText;

        public Camera cam;
        
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        public SoundSetting soundSettings = new SoundSetting();


        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded, m_Crouch;
        private bool m_Crouching, justStoppedCrouching;


        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Crouching
        {
            get { return m_Crouching; }
        }
        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }

 
        private void Start()
        {

            score = 0;
            maxScore = 27;
            time = 0.0f;
            m_audioSource = GetComponent<AudioSource>();
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init (transform, cam.transform);
            
            
            justStoppedCrouching = false;


        }


        private void Update()
        {
            RotateView();

            if (score < maxScore)
            {
                time += Time.deltaTime;
                UpdateTimeText();
            }

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
            {
                playSound(soundSettings.m_Jumping);
                m_Jump = true;
            }
            if(CrossPlatformInputManager.GetButtonDown("Crouch") && !m_Crouch)
            {
           //     Vector3 currPos = cam.transform.position;
                playSound(soundSettings.m_Footsteps);
          //      currPos.z -= 100;
               // cam.transform.position = new Vector3(currPos.x, currPos.y - 10, currPos.z);
                m_Crouch = true;


            }
            if (CrossPlatformInputManager.GetButtonUp("Crouch") && m_Crouch)
            {
                
                m_Crouch = false;
                justStoppedCrouching = true;
            }
        }


        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward*input.y + cam.transform.right*input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce,0f), ForceMode.Impulse);
                    m_Jumping = true;
                }
                if (m_Crouch && !m_Crouching)
                {


                    transform.localScale -= new Vector3(0,advancedSettings.crouchDistance,0);
                    m_Capsule.height -= 0.2f;
                   
                    //            cam.transform.localScale.Set(cam.transform.position.x,cam.transform.position.y-100,cam.transform.position.z);

                    // m_RigidBody.GetComponent<CapsuleCollider>().height -= 5;
                    // tr.localScale += new Vector3(0,0.8f,0);
                    //    m_RigidBody.GetComponent<CapsuleCollider>().center
                    //   cam.transform.localPosition.Set(12,2,2);
                    // m_RigidBody.position = new Vector3(cam.transform.position.x,cam.transform.localScale.y,cam.transform.position.z);
                    //  m_RigidBody.transform.position.y -= 100;
                    m_Crouching = true;
                   

                }

                if (justStoppedCrouching)
                {   
                //    tr.localScale += new Vector3(0, 0.8f, 0);
                    m_Capsule.height += 0.2f;
                    transform.localScale += new Vector3(0,advancedSettings.crouchDistance,0);
                    m_Crouching = false;
                    justStoppedCrouching = false;
                }
                if (!m_Crouching && !m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }

        private void playSound(AudioClip sound)
        {
            m_audioSource.PlayOneShot(sound);
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }
        

        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = CrossPlatformInputManager.GetAxis("Horizontal"),
                    y = CrossPlatformInputManager.GetAxis("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {

                playSound(soundSettings.m_Landing);
                m_Jumping = false;
            }
        }


        void OnTriggerEnter(Collider other)
        {

            AudioSource otherAudioSource = other.GetComponent<AudioSource>();
            if (other.gameObject.CompareTag("Small Orb"))
            {

                m_audioSource.PlayOneShot(otherAudioSource.clip);
                
                other.gameObject.SetActive(false);
                score += 1;
            }
            else if (other.gameObject.CompareTag("Medium Orb"))
            {
                
                m_audioSource.PlayOneShot(otherAudioSource.clip);
                other.gameObject.SetActive(false);
                score += 3;
            }
            else if (other.gameObject.CompareTag("Large Orb"))
            {
                
                m_audioSource.PlayOneShot(otherAudioSource.clip);
                other.gameObject.SetActive(false);
                score += 5;
            }

            UpdateScoreText(); // finally update score text
        }


        void UpdateScoreText()
        {
            scoreText.text = "Score: " + score.ToString();
        }

        void UpdateTimeText()
        {
            timerText.text = "Time: " + Math.Round(time, 2).ToString();
        }
    

    }
}
