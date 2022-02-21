using UnityEngine;

//namespace UnityStandardAssets.Characters.ThirdPerson
//{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{

		Vector3 INPUT_forward_vector;


		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		// [SerializeField] float m_JumpPower = 12f;
		// [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		// [SerializeField] float m_CrouchSpeedMultiplier = 0.25f;
		// [SerializeField] float m_GroundCheckDistance = 0.1f;
		[SerializeField] LayerMask m_RaycastLayers = Physics.AllLayers;
		[SerializeField] float m_SlopeAngleLimit = 45f;

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded = true;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		float m_SlopeAngle;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching = false;

		bool m_IsStrafing = true;
		// bool m_IsRagdoll
		// {
		// 	get
		// 	{
		// 		return m_Ragdoll.Enabled;
		// 	}
		// 	set
		// 	{
		// 		m_Ragdoll.Enabled = value;
		// 	}
		// }
		Vector3 m_SmoothGroundNormal = Vector3.up;
		// float m_AirborneTurnAmount;
		// CameraController m_Cam;
		// bool m_DoubleJump;
		// CharacterVFX m_VFX;
		CharacterFootIK m_FootIK;
		// CharacterRagdollControl m_Ragdoll;
		float m_SprintMultiplier = 1f;

		// public enum CharacterState
		// {
		// 	GroundedFreeRun,
		// 	GroundedStrafe,
		// 	Airborne,
		// 	Crouching,
		// 	Ragdoll
		// }
		// public CharacterState CharState
		// {
		// 	get
		// 	{
		// 		if (m_IsRagdoll)
		// 			return CharacterState.Ragdoll;
		// 		else
		// 			if (m_IsGrounded)
		// 				if (!m_Crouching)
		// 					if (m_IsStrafing)
		// 						return CharacterState.GroundedStrafe;
		// 					else
		// 						return CharacterState.GroundedFreeRun;
		// 				else
		// 					return CharacterState.Crouching;
		// 			else
		// 				return CharacterState.Airborne;
		// 	}
		// }


		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;
			// m_Cam = Camera.main.GetComponent<CameraController>();
			// m_VFX = GetComponent<CharacterVFX>();
			m_FootIK = GetComponent<CharacterFootIK>();
			// m_Ragdoll = GetComponent<CharacterRagdollControl>();

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			// m_OrigGroundCheckDistance = m_GroundCheckDistance;
		}


		public void Move(Vector3 move, bool interact)//, bool crouch, bool jump, bool strafe, bool interact)
		{

			// if (m_IsRagdoll) move = Vector3.zero;

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			// CheckGroundStatus();
			// ModifyMoveOnSlope(ref move);

			bool strafe = true;
			SetStateVariables(move, strafe);

#if UNITY_EDITOR
			// draws the input vector's direction in world space
			Debug.DrawRay(m_Rigidbody.position + Vector3.up, transform.TransformDirection(move), Color.red);
			// Debug.DrawRay(transform.position, m_SmoothGroundNormal, (move.normalized.y > Mathf.Sin(m_SlopeAngleLimit * Mathf.Deg2Rad) ?  Color.red : Color.cyan), 1f);
#endif

			ApplyTurnRotation();

			// control and velocity handling is different when grounded and airborne:
//			if (m_IsGrounded)
//			{
				var crouch = false; var jump = false;
				HandleGroundedMovement(crouch, jump);
//			}
//			else
//			{
//				HandleAirborneMovement(jump, move);
//			}

//			ScaleCapsuleForCrouching(crouch);
//			PreventStandingInLowHeadroom();

			// send input and other state parameters to the animator
			UpdateAnimator(move);

//			if (strafe && !m_Crouching)
//				m_Cam.FollowTargetStrafing();
//			else
//				m_Cam.FollowTarget(m_Crouching);

//			if (strafe) m_VFX.StartLightningFX();
//			else m_VFX.StopLightningFX();

			m_FootIK.enableFeetIk = m_IsGrounded;
			m_FootIK.useFootRotation = (m_IsGrounded && m_ForwardAmount == 0f && m_TurnAmount == 0f);
			m_FootIK.slopeAngle = 1.0f;// + 4.0f * Mathf.Clamp01(m_SlopeAngle / m_SlopeAngleLimit);

			// m_Ragdoll.Enabled = interact;
		}


		void SetStateVariables(Vector3 move, bool strafe)
		{
//			if (!strafe || !m_IsGrounded)
//			{
//				m_TurnAmount = Mathf.Atan2(move.x, move.z);
//				m_ForwardAmount = move.z;
//				m_IsStrafing = false;
//			}
//			else
//			{
				// if strafing and grounded
				// strafing is disabled while crouching
				m_TurnAmount = move.x;
				m_ForwardAmount = move.z;
//				m_IsStrafing = !m_Crouching;
//			}
		}

		// void ModifyMoveOnSlope(ref Vector3 move)
		// {
		// 	move = Vector3.ProjectOnPlane(move, m_SmoothGroundNormal);
		// 	move = transform.InverseTransformDirection(move);
		//
		// 	// if (move.normalized.y > Mathf.Sin(m_SlopeAngleLimit * Mathf.Deg2Rad))
		// 	// {
		// 	// 	move = Vector3.zero;
		// 	// }
		// 	// move *= Mathf.Lerp(1f, 0f, move.normalized.y + (1f - Mathf.Sin(Mathf.Deg2Rad * m_SlopeAngleLimit)));
		//
		// 	m_SlopeAngle = Mathf.Asin(move.normalized.y) * Mathf.Rad2Deg;
		// 	// if (m_SlopeAngle > m_SlopeAngleLimit - 10f)
		// 	// 	move = Vector3.Lerp(move, Vector3.zero, (m_SlopeAngle - m_SlopeAngleLimit + 10f) / 10f);
		// 	if (m_SlopeAngle > m_SlopeAngleLimit)
		// 		move = Vector3.ClampMagnitude(move, 0.5f);
		//
		// 	//Debug.Log("Slope " + slopeAngle + " lerp " + (slopeAngle - m_SlopeAngleLimit + 5f) / 5f);
		// 	// if (slopeAngle > m_SlopeAngleLimit)
		// 	// 	move = Vector3.zero;
		// }

		// void ScaleCapsuleForCrouching(bool crouch)
		// {
		// 	if (m_IsGrounded)
		// 	{
		// 		if (crouch)
		// 		{
		// 			if (m_Crouching) return;
		// 			m_Capsule.height = m_Capsule.height / 2f;
		// 			m_Capsule.center = m_Capsule.center / 2f;
		// 			m_Crouching = true;
		// 		}
		// 		else
		// 		{
		// 			Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
		// 			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
		// 			if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, m_RaycastLayers, QueryTriggerInteraction.Ignore))
		// 			{
		// 				m_Crouching = true;
		// 				return;
		// 			}
		// 			m_Capsule.height = m_CapsuleHeight;
		// 			m_Capsule.center = m_CapsuleCenter;
		// 			m_Crouching = false;
		// 		}
		// 	}
		// }

		// void PreventStandingInLowHeadroom()
		// {
		// 	// prevent standing up in crouch-only zones
		// 	if (!m_Crouching && m_IsGrounded)
		// 	{
		// 		Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
		// 		float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
		// 		if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, m_RaycastLayers, QueryTriggerInteraction.Ignore))
		// 		{
		// 			m_Crouching = true;
		// 		}
		// 	}
		// }


		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount * m_SprintMultiplier, 0.02f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.02f, Time.deltaTime);
			m_Animator.SetBool("Crouch", m_Crouching);
			m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}

			if (m_IsGrounded) {
				m_Animator.SetFloat("SlopeAngle", m_SlopeAngle / m_SlopeAngleLimit);
			}

			m_Animator.SetBool("Strafe", m_IsStrafing);

			if (m_IsStrafing)
			{
				// adds a turn-in-place animation parameter when turning to face the camera.
				float turnAnim = Vector3.SignedAngle(transform.forward, Vector3.Scale(INPUT_forward_vector,
					new Vector3(1,0,1)).normalized, Vector3.up);
				turnAnim = Mathf.Sign(turnAnim) * Mathf.Sqrt(Mathf.Abs(turnAnim / 60f));

				m_Animator.SetFloat("CameraTurn", turnAnim, 0.1f, Time.deltaTime);
			}

            // sets weight for upper body for aiming
			//m_Animator.SetLayerWeight(1, m_IsStrafing ? 1f : 0f);

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded)
			{
				if (jumpLeg > 0) Debug.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position,
					m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position, Color.yellow);
				else if (jumpLeg < 0) Debug.DrawLine(m_Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position,
					m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position, Color.yellow);
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}


		// void HandleAirborneMovement(bool jump, Vector3 move)
		// {
		// 	if (jump && m_DoubleJump && m_Rigidbody.velocity.y < m_JumpPower * 0.5f)
		// 	{
		// 		// double jump!
		// 		m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z) +
		// 			transform.TransformDirection(move) * m_JumpPower * 0.25f;
		// 		m_IsGrounded = false;
		// 		m_Animator.applyRootMotion = false;
		// 		m_GroundCheckDistance = 0.1f;
		//
		// 		m_DoubleJump = false;
		// 		m_VFX.EmitSmokeFX(0.5f);
		// 	}
		// 	else if (m_Rigidbody.useGravity)
		// 	{
		// 		// applies extra gravity multiplier
		// 		Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
		// 		m_Rigidbody.AddForce(extraGravityForce);
		// 	}
		//
		// 	m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		// }

		void HandleGroundedMovement(bool crouch, bool jump)
		{
			Vector3 v;
			if (m_IsStrafing)
				// overrides root motion for grounded strafing animations
				v = transform.TransformDirection(new Vector3(m_TurnAmount, 0, m_ForwardAmount)) *
					2f * m_MoveSpeedMultiplier * m_AnimSpeedMultiplier;
			else
				// overrides root motion for grounded freerun and crouch animations
				v = (m_Rigidbody.rotation * Vector3.forward) * m_ForwardAmount *
					5f * m_MoveSpeedMultiplier * m_AnimSpeedMultiplier * m_SprintMultiplier;


			if (v.magnitude < 0.1f)
				v.y = Mathf.Lerp(m_Rigidbody.velocity.y, 0f, Time.deltaTime / 0.15f);
			else
				v.y = m_Rigidbody.velocity.y;
			m_Rigidbody.velocity = v;


			// checks whether conditions are right to allow a jump
			// player used to be able to bunny hop if they hit the jump button,
			// in the single frame where they touch the ground but the animator transition has not yet been invoked
			// if (jump && !crouch && !m_Animator.IsInTransition(0) && m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("OnGround"))
			// {
			// 	// jump!
			// 	m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z) +
			// 		Vector3.Scale(m_Rigidbody.velocity, new Vector3(1,0,1)).normalized * m_JumpPower * 0.2f;
			// 	m_IsGrounded = false;
			// 	m_Animator.applyRootMotion = false;
			// 	m_GroundCheckDistance = 0.1f;
			//
			// 	m_DoubleJump = true;
			// }
			// else if (m_Rigidbody.useGravity)
			// {
			// 	// applies extra gravity multiplier
			// 	Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			// 	m_Rigidbody.AddForce(extraGravityForce);
			// }
		}

		void ApplyTurnRotation()
		{
//			if (m_IsGrounded)
//			{
//				if (m_IsStrafing)
//				{
					// turns the character to face the camera's direction
					Vector3 newLookAt = Vector3.Scale(INPUT_forward_vector, new Vector3(1,0,1)).normalized;
					transform.forward = Vector3.Lerp(transform.forward, newLookAt, Time.deltaTime / 0.1f);
					// m_AirborneTurnAmount = 0f;
//				}
//				else
//				{
					// help the character turn faster (this is in addition to root rotation in the animation)
//					float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
//					transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
					// m_AirborneTurnAmount = m_TurnAmount * turnSpeed * 0.5f;
//				}
//			}
			// else
			// {
			// 	// if not grounded and not strafing lock turn speed mid-jump
			// 	transform.Rotate(0, m_AirborneTurnAmount * Time.deltaTime, 0);
			// }
		}


		// root motion is currently being overridden but not applied here
		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			/*if (m_Animator.applyRootMotion && Time.deltaTime > 0)
			{
				Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
			}*/
		}


// 		void CheckGroundStatus()
// 		{
// 			RaycastHit hitInfo;
// #if UNITY_EDITOR
// 			// helper to visualise the ground check ray in the scene view
// 			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
// #endif
// 			// 0.1f is a small offset to start the ray from inside the character
// 			// it is also good to note that the transform position in the sample assets is at the base of the character
// 			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance,
// 				m_RaycastLayers, QueryTriggerInteraction.Ignore))
// 			{
// 				if (!m_IsGrounded && m_Rigidbody.velocity.y < -3f) m_VFX.EmitSmokeFX(0.1f * -m_Rigidbody.velocity.y);
// 				m_GroundNormal = hitInfo.normal;
// 				m_IsGrounded = true;
// 				m_Animator.applyRootMotion = true;
// 			}
// 			else
// 			{
// 				m_GroundNormal = Vector3.up;
// 				m_IsGrounded = false;
// 				m_Animator.applyRootMotion = false;
// 			}
// 			m_SmoothGroundNormal = Vector3.Lerp(m_SmoothGroundNormal, m_GroundNormal, Time.deltaTime / 0.25f);
// 		}
	}
//}
