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
        private Transform tr;
        private AudioSource m_AudioSource;

        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting, we probably wont need this
            public float CrouchSpeed = 3.0f; // Crouch walk speed
	        public KeyCode RunKey = KeyCode.LeftShift;
            public KeyCode CrouchKey = KeyCode.X;
            public float JumpForce = 30f;
            public float DoubleJumpForce = 10f;
            public bool m_Moving = false;
            
            
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;
#if !MOBILE_INPUT
            private bool m_Crouching;

#endif      

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if ((Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"))  && !m_Moving)
                {
                    m_Moving = true;

                }
                else if ((Input.GetButtonUp("Horizontal") || Input.GetButtonUp("Vertical")) && (!Input.GetButtonDown("Horizontal") && !Input.GetButtonDown("Vertical")))
                {
                    m_Moving = false;
                }

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

                    m_Moving = true;
                    m_Crouching = true;
                }
	            else
	            {
                    m_Crouching = false;
                }
#endif
            }

#if !MOBILE_INPUT

#endif
        }


        


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.1f; // stops the character
            public float slowDownRate = 80f; // rate at which the controller comes to a stop when there is no input
            public float crouchDistance = 0.2f; // default crouch depth/how low the character crouches
            public bool airControl = true; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset = 0.1f; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }

         [Serializable]
         public class SoundSettings
         {
             public AudioClip m_Footsteps;
             public AudioClip m_Jumping;
             public AudioClip m_Landing;
             public AudioClip m_DoubleJump;


         }

        [Serializable]
        public class Buffs
        {
            public bool m_PDoubleJump;
            public bool m_BSpeedBoost;
            public bool m_BCloudJump;
            public bool m_BFeatherFalling;
            public float speedBoostTimer = 30f;
            public float cloudJumpTimer = 30f;
            public float featherFallingTimer = 30f;

            public float speedBoostSpeedIncrease = 1.5f;
            // IMPLEMENT FEATHER FALLING STUUFJOWA

        }

        void resetSpeedBoost()
        {
            buffs.speedBoostTimer = 30f;

        }

        void resetCloudJUmp()
        {
            buffs.cloudJumpTimer = 30f;
        }

        void resetFeatherFalling()
        {
            buffs.featherFallingTimer = 30f;
        }


        private void Start()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);

			tips = new string[] {
				"COLLECT ORBS FOR POINTS AND BUFFS!",
				"AVOID THE CAT!",
				"AFTER COLLECTING AS MANY ORBS AS POSSIBLE, BRING THEM TO THE TREE'S STAR",
				"'MAGIC' BUTTON TO CROUCH"
			};

			PlayerPrefs.SetInt ("orbTip", 1);
			PlayerPrefs.SetInt ("catTip", 1);
			PlayerPrefs.SetInt ("starTip", 1);
			PlayerPrefs.SetInt ("crouchTip", 1);
			tipText.enabled = false;

            tr = transform;

            m_doubleJumpDone = false;
            justStoppedCrouching = false;

            maxScore = 27;
            score = 0;
            time = 0.0f;
        }
        public Camera cam;
        
		public int getScore(){

			return score;
		}

		public float getTime(){
			return time;
		}

        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        public SoundSettings soundSettings = new SoundSettings();
        public Buffs buffs = new Buffs();


        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded, m_Crouch, m_wannaCrouch;
        private bool m_Crouching, justStoppedCrouching, m_doubleJumpDone;
        private bool doubleJumpNow;
        // doublejumpdone checks if the player has already doublejumped, or does he still have a second jump left
        // doubleJumpNow indicates the player wants to doublejump and it will be checked on the next update.

		private int maxScore;
		private int score;
		public Text scoreText;
		private float time;
		public Text timerText;

		public Text tipText;
		private float tipTimer;

		private string[] tips;

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

		public bool WannaCrouch
		{
			get { return m_wannaCrouch; }
		}


        private void PlaySound(AudioClip sound)
        {
            m_AudioSource.PlayOneShot(sound);
        }



        bool notStarted = true;



        private void Update()
        {
			if (tipTimer > 0) {
				tipTimer -= Time.deltaTime;
				if (tipTimer <= 0) {
					tipTimer = 0;
					tipText.enabled = false;
				}	
			}

			RotateView();
            if (movementSettings.m_Moving && notStarted)
            {
                m_AudioSource.clip = soundSettings.m_Footsteps;
                m_AudioSource.Play();
                // m_AudioSource.loop = true;
                //    movementSettings.m_Moving = false;
                //  PlaySound(soundSettings.m_Footsteps);
                if (m_AudioSource.isPlaying)
                {
                    notStarted = false;
                }
                
            }
            else if (!movementSettings.m_Moving)
            {
                notStarted = true;
            }
            if (score < maxScore) {
				time += Time.deltaTime;
				UpdateTimeText ();
			}

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
            {
                m_Jump = true;
                if (!m_Jumping)
                {
          
                    m_AudioSource.PlayOneShot(soundSettings.m_Jumping);
                }
                else if (m_Jumping && !Grounded && buffs.m_PDoubleJump && !m_doubleJumpDone)
                {
                    doubleJumpNow = true;
                }
            }
            if(CrossPlatformInputManager.GetButtonDown("Crouch") && !m_Crouch)
            {
                
           //     Vector3 currPos = cam.transform.position;

          //      currPos.z -= 100;
               // cam.transform.position = new Vector3(currPos.x, currPos.y - 10, currPos.z);
				if (!m_Jumping)
					m_Crouch = true;
				else
					m_wannaCrouch = true;
            }
            if (CrossPlatformInputManager.GetButtonUp("Crouch") && m_Crouch)
            {
                
                m_Crouch = false;
                justStoppedCrouching = true;
            }
			if (CrossPlatformInputManager.GetButtonUp("Crouch") && m_wannaCrouch)
			{
				m_wannaCrouch = false;
			}
            if (buffs.m_BCloudJump)
            {
                if (buffs.cloudJumpTimer > 0)
                {
                    
                    buffs.cloudJumpTimer -= Time.deltaTime;
                }
                else
                {
                    buffs.m_BCloudJump = false;
                    resetCloudJUmp();
                    
                }
            }

            if (buffs.m_BFeatherFalling)
            {
                if (buffs.featherFallingTimer > 0)
                {
                    buffs.featherFallingTimer -= Time.deltaTime;
                }
                else
                {
                    buffs.m_BFeatherFalling = false;
                    resetFeatherFalling();
                }
                
            }

            if (buffs.m_BSpeedBoost)
            {

                if (buffs.speedBoostTimer > 0)
                {
                    
                    buffs.speedBoostTimer -= Time.deltaTime;
                }
                else
                {
                    buffs.m_BSpeedBoost = false;
                    resetSpeedBoost();
                }
            }
        }

            

        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if (!m_IsGrounded)
            {
                Vector3 vel = m_RigidBody.velocity;
                vel.y -= 13f * Time.deltaTime;
            
                m_RigidBody.velocity = vel;
            }
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (m_IsGrounded))
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

            else if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) &&
                     (advancedSettings.airControl))
            {
                //               Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                //              desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                //              desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                //              desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                //              desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
             //   Vector3 vel = m_RigidBody.velocity;
            //    vel.y -= 13f*Time.deltaTime;

            //    m_RigidBody.velocity = vel;
      //          if (m_RigidBody.velocity.sqrMagnitude <
      //              (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
       //         {
                   // m_RigidBody.AddForce(desiredMove * SlopeMultiplier() * 0.5f, ForceMode.Impulse);
      //          }

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            }
            if (doubleJumpNow && !m_doubleJumpDone && buffs.m_PDoubleJump && !m_IsGrounded)
            {
                
                m_RigidBody.drag = 0f;
                // m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
         //       m_RigidBody.velocity.Set(m_RigidBody.velocity.x,0f,m_RigidBody.velocity.z );

                
                
                
           //     m_RigidBody.AddForce(Vector3.up * movementSettings.DoubleJumpForce);
           // TODO : implement doublejump direction change
                m_RigidBody.AddForce(new Vector3(0f, movementSettings.DoubleJumpForce, 0f), ForceMode.VelocityChange);
                PlaySound(soundSettings.m_DoubleJump);
                m_Jumping = true;
                m_doubleJumpDone = true;
                doubleJumpNow = false;
            }
            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (m_Crouch && !m_Crouching)
                {


                    transform.localScale -= new Vector3(0,advancedSettings.crouchDistance,0);
                    m_Capsule.height -= advancedSettings.crouchDistance;
                   
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
                    m_Capsule.height += advancedSettings.crouchDistance;
                    transform.localScale += new Vector3(0,advancedSettings.crouchDistance,0);
                    m_Crouching = false;
                    justStoppedCrouching = false;
                }
                if (!m_Crouching && !m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    movementSettings.m_Moving = false;
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

                PlaySound(soundSettings.m_Landing);
                m_Jumping = false;
                if (m_wannaCrouch)
                {
                    m_Crouch = true;
                }
                m_doubleJumpDone = false;

            }
        }

		void OnTriggerEnter(Collider other)
		{

		    AudioSource otherSource = other.GetComponent<AudioSource>();
			if (other.gameObject.CompareTag ("Small Orb"))
			{
                m_AudioSource.PlayOneShot(otherSource.clip);
				other.gameObject.SetActive (false);
				score += 1;
				UpdateScoreText ();
				if (PlayerPrefs.GetInt ("orbTip") == 1) {
					PlayerPrefs.SetInt ("orbTip", 0);
					tipText.enabled = true;
					Debug.Log (tips [0]);
					tipText.text = tips[0];
					tipTimer = 5;
				}
			}
		else if (other.gameObject.CompareTag ("Medium Orb"))
			{

                m_AudioSource.PlayOneShot(otherSource.clip);
                other.gameObject.SetActive (false);
				score += 3;
				UpdateScoreText ();
				if (PlayerPrefs.GetInt ("orbTip") == 1) {
					PlayerPrefs.SetInt ("orbTip", 0);
					tipText.enabled = true;
					tipText.text = tips[0];
					tipTimer = 5;
				}
			}
		else if (other.gameObject.CompareTag ("Large Orb")) // POWERUP + BUFF
			{

				if (PlayerPrefs.GetInt ("orbTip") == 1) {
					PlayerPrefs.SetInt ("orbTip", 0);
					tipText.enabled = true;
					tipText.text = tips[0];
					tipTimer = 5;
				}


			    if (other.gameObject.name == "DoubleJump")
			    {
                    
			        buffs.m_PDoubleJump = true;
                   
			    }
                else if(other.gameObject.name == "FeatherFalling")
			    {
			        buffs.m_BFeatherFalling = true;
			    }
                else if (other.gameObject.name == "SpeedBoost")
                {
                    buffs.m_BSpeedBoost = true;
                    movementSettings.ForwardSpeed = movementSettings.ForwardSpeed*buffs.speedBoostSpeedIncrease;
                }
                else if (other.gameObject.name == "CloudJump")
                {
                    buffs.m_BCloudJump = true;
                }
                m_AudioSource.PlayOneShot(otherSource.clip);
                other.gameObject.SetActive (false);
			}else if (other.gameObject.CompareTag ("Tip2"))
			{
				other.gameObject.SetActive (false);
				if (PlayerPrefs.GetInt ("catTip") == 1) {
					PlayerPrefs.SetInt ("catTip", 0);
					tipText.enabled = true;
					tipText.text = tips[1];
					tipTimer = 3;
				}
			}else if (other.gameObject.CompareTag ("Tip3"))
			{
				other.gameObject.SetActive (false);
				if (PlayerPrefs.GetInt ("starTip") == 1) {
					PlayerPrefs.SetInt ("starTip", 0);
					tipText.enabled = true;
					tipText.text = tips[2];
					tipTimer = 5;
				}
			}else if (other.gameObject.CompareTag ("Tip4"))
			{
				other.gameObject.SetActive (false);
				if (PlayerPrefs.GetInt ("crouchTip") == 1) {
					PlayerPrefs.SetInt ("crouchTip", 0);
					tipText.enabled = true;
					tipText.text = tips[3];
					tipTimer = 4;
				}
			}
		}

		void UpdateScoreText()
		{
			scoreText.text = "Score: " + score.ToString ();
		}

		void UpdateTimeText()
		{
			timerText.text = "Time: " + Math.Round(time, 2).ToString ();
		}

    }
}
