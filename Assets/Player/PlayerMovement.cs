using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Chromecore
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class PlayerMovement : MonoBehaviour
	{
		[Title("Move"), Min(0.01f)]
		[SerializeField] private float moveSpeed;
		[Tooltip("@moveSpeed * runMultiplyer"), Min(1)]
		[SerializeField] private float runMultiplyer;
		[Min(0.1f), HorizontalGroup("Acceleration"), Tooltip("How long it takes to get up to full speed")]
		[SerializeField] private float accelerationTime;
		[HorizontalGroup("Acceleration"), HideLabel]
		[SerializeField] private AnimationCurve accelerationCurve;
		[Min(0.1f), HorizontalGroup("Decceleration"), Tooltip("How long it takes to fully slow down")]
		[SerializeField] private float deccelerationTime;
		[HorizontalGroup("Decceleration"), HideLabel]
		[SerializeField] private AnimationCurve deccelerationCurve;
		private float currentAccelerationTime;
		private float currentDeccelerationTime;
		private bool isRunning;
		private float move;

		[Title("Grapple")]
		[SerializeField, Min(0)] private float maxGrappleDistance;
		[SerializeField, Min(0)] private float minGrappleDistance;
		[SerializeField, Min(0)] private float grappleSwingSpeed;
		[SerializeField, Min(0)] private float grappleUpDownSpeed;
		[SerializeField, Min(0)] private float grappleReleaseX = 20;
		[SerializeField, Min(0)] private float grappleReleaseY = 1;
		[SerializeField, Min(0)] private float grappleGravity;
		[SerializeField, Range(0, 1)] private float grappleDrag;
		[SerializeField, Required] private LineRenderer grappleLine;
		[SerializeField, Required] private Transform grappleCircle;
		[SerializeField, Required] private DistanceJoint2D joint;
		[SerializeField] private LayerMask grappleLayer;

		[Title("Jump"), Min(0)]
		[SerializeField] private float jumpHeight;
		[Range(0, 1)]
		[SerializeField] private float jumpEndEarlyFallMultiplyer;
		[Min(0), Tooltip("Time between running off a ledge and pressing jump which it will jump")]
		[SerializeField] private float coyoteTime;
		[Min(0), Tooltip("Time between pressing the jump key and hitting the ground in which it will buffer the jump")]
		[SerializeField] private float jumpBufferTime;
		[Title("Head Check"), ChildGameObjectsOnly(IncludeSelf = false), Required("Head check needs to be assigned")]
		[SerializeField] private Transform headCheck;
		[SerializeField] private Vector2 headSize;
		[Min(0), Title("Jump Apex Modifiers")]
		[SerializeField] private float jumpApexThreashold;
		[Min(0), Tooltip("Extra speed added onto the movement at the apex of the jump")]
		[SerializeField] private float apexBonusSpeed;
		private bool hitHead;
		/// <summary>
		/// 1 at the apex of the jump, 0 at the bottom
		/// </summary>
		private float apexPoint;
		private float coyoteTimer;
		private float jumpBufferTimer;
		private bool startJumpBuffer;

		[Title("Fall"), Min(0)]
		[SerializeField] private float gravityAmount;
		[InfoBox("@gravityAmount * fallGravityScale"), Min(1)]
		[SerializeField] private float fallGravityScale;
		[Min(0)]
		[SerializeField] private float maxFallSpeed;

		[Title("Ground"), ChildGameObjectsOnly(IncludeSelf = false), Required("Ground check needs to be assigned")]
		[SerializeField] private Transform groundCheck;
		[SerializeField] private Vector2 groundSize;
		[SerializeField] private LayerMask groundMask;
		private bool onGround;

		[Title("Visual Effects")]
		[SerializeField, Min(0)] private float spriteTiltAngle;
		[SerializeField, Required] private Transform sprite;

		[Title("Other")]
		[SerializeField, Required] private Rigidbody2D body;
		[SerializeField, Required] private Camera mainCamera;

		private bool canControl = true;

		private void Reset()
		{
			body = GetComponent<Rigidbody2D>();
		}

		private void OnValidate()
		{
			grappleCircle.localScale = Vector3.one * maxGrappleDistance * 2;
		}

		private void Awake()
		{
			grappleCircle.localScale = Vector3.one * maxGrappleDistance * 2;
		}

		private void Start()
		{
			InputHandler.Instance.playerActions.Jump.performed += ProcessJumpInput;
			InputHandler.Instance.playerActions.Run.performed += Run;
			InputHandler.Instance.playerActions.Grapple.performed += GrapplePressed;
		}

		private void OnDestroy()
		{
			InputHandler.Instance.playerActions.Jump.performed -= ProcessJumpInput;
			InputHandler.Instance.playerActions.Run.performed -= Run;
			InputHandler.Instance.playerActions.Grapple.performed -= GrapplePressed;
		}

		private void Update()
		{
			HandleGravity();

			// apply movement
			HandleMove();

			// apex speed modifier
			if (!joint.enabled)
			{
				apexPoint = Mathf.InverseLerp(jumpApexThreashold, 0, 1 / Mathf.Abs(body.linearVelocityY));
				float apexBonus = Mathf.Sign(move) * apexBonusSpeed * apexPoint;
				body.linearVelocityX += apexBonus * Time.deltaTime;
			}

			// drag
			if (joint.enabled && move == 0)
			{
				body.linearVelocity -= body.linearVelocity * grappleDrag;
			}

			// grapple up/down
			if (joint.enabled)
			{
				float distance = joint.distance + InputHandler.Instance.playerActions.UpDown.ReadValue<float>() * grappleUpDownSpeed * Time.deltaTime;
				joint.distance = Mathf.Clamp(distance, minGrappleDistance, maxGrappleDistance);
			}

			// Max fall speed
			if (!joint.enabled) body.linearVelocityY = Mathf.Max(body.linearVelocityY, -maxFallSpeed);

			HandleJumpEffects();
			HandleVisualEffects();
		}

		private void HandleVisualEffects()
		{
			float tiltAmount = body.linearVelocityX < -0.01f ? spriteTiltAngle : body.linearVelocityX > 0.01f ? -spriteTiltAngle : 0;
			Vector3 rotation = sprite.localEulerAngles;
			rotation.z = tiltAmount;
			sprite.localEulerAngles = rotation;

			// grapple
			grappleLine.gameObject.SetActive(joint.enabled);
			if (grappleLine.gameObject.activeSelf)
			{
				grappleLine.SetPosition(0, transform.position);
				grappleLine.SetPosition(1, joint.connectedAnchor);
			}
		}

		private void HandleJumpEffects()
		{
			// coyote timing
			if (!onGround) coyoteTimer += Time.deltaTime;
			else coyoteTimer = 0;

			// jump buffer
			if (startJumpBuffer && onGround && jumpBufferTimer <= jumpBufferTime)
			{
				startJumpBuffer = false;
				jumpBufferTimer = 0;
				Jump();
			}
			if (!onGround && startJumpBuffer) jumpBufferTimer += Time.deltaTime;
		}

		private void HandleGravity()
		{
			// apply gravity : fall faster if going down
			body.linearVelocityY -= (joint.enabled ? grappleGravity : gravityAmount) * Time.deltaTime * (body.linearVelocityY < 0 ? fallGravityScale : 1);

			if (joint.enabled) return;

			// stop gravity from increasing
			if (onGround && body.linearVelocityY <= 0) body.linearVelocityY = 0;

			// hit head
			if (hitHead && !onGround && body.linearVelocityY > 0)
			{
				body.linearVelocityY = 0;
			}
		}

		private void FixedUpdate()
		{
			onGround = Physics2D.OverlapBox(groundCheck.position, groundSize, 0, groundMask);
			hitHead = Physics2D.OverlapBox(headCheck.position, headSize, 0, groundMask);
		}

		private void GrapplePressed(InputAction.CallbackContext ctx)
		{
			if (!canControl) return;
			if (!ctx.action.IsPressed())
			{
				EndGrapple();
				return;
			}
			Vector2 direction = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxGrappleDistance, grappleLayer);
			if (hit.collider != null)
			{
				StartGrapple(hit);
			}
		}

		private void EndGrapple()
		{
			if (joint.enabled == false) return;
			joint.enabled = false;
			float magnitude = body.linearVelocity.magnitude;
			body.linearVelocityX = move * magnitude * grappleReleaseX;
			if (body.linearVelocityY > 0) body.linearVelocityY += magnitude * grappleReleaseY;
			else body.linearVelocityY = magnitude * grappleReleaseY;
		}

		private void StartGrapple(RaycastHit2D hit)
		{
			joint.connectedAnchor = hit.point;
			joint.enabled = true;
		}

		private void ProcessJumpInput(InputAction.CallbackContext ctx)
		{
			if (joint.enabled || !canControl) return;
			// jumps based on how long you hold the jump button
			if (!ctx.action.IsPressed())
			{
				body.linearVelocityY *= jumpEndEarlyFallMultiplyer;
				return;
			}

			Jump();
		}

		private void Jump()
		{
			if (!onGround)
			{
				startJumpBuffer = true;
				jumpBufferTimer = 0;
			}

			bool canCoyoteTime = !onGround && coyoteTimer <= coyoteTime;
			if (!onGround && !canCoyoteTime) return;
			body.linearVelocityY = Mathf.Sqrt(jumpHeight * 2 * gravityAmount);
			onGround = false;
		}

		private void Run(InputAction.CallbackContext ctx)
		{
			isRunning = ctx.action.IsPressed() ? true : false;
		}

		private void HandleMove()
		{
			move = canControl ? InputHandler.Instance.playerActions.Move.ReadValue<float>() : 0;

			if (joint.enabled)
			{
				body.linearVelocity += -Vector2.Perpendicular(joint.connectedAnchor - joint.anchor).normalized * move * grappleSwingSpeed;
				return;
			}

			if (move == 0)
			{
				// deccelerate
				float decceleration = deccelerationCurve.Evaluate(currentDeccelerationTime / deccelerationTime);
				body.linearVelocityX *= decceleration;
				currentDeccelerationTime += Time.deltaTime;
				currentAccelerationTime = 0;
			}
			else
			{
				// accelerate
				if (currentAccelerationTime == 0)
				{
					// find the starting value of the acceleration
					currentAccelerationTime = Mathf.Abs(body.linearVelocity.x) / moveSpeed;
				}
				float speed = accelerationCurve.Evaluate(currentAccelerationTime / accelerationTime) * moveSpeed;
				speed *= isRunning ? runMultiplyer : 1;

				body.linearVelocityX = move * speed;
				currentAccelerationTime += Time.deltaTime;
				currentDeccelerationTime = 0;
			}
		}

		public void Die()
		{
			canControl = false;
			EndGrapple();
		}

		private void OnDrawGizmosSelected()
		{
			// Draw The ground check rectangle
			if (groundCheck != null)
			{
				Gizmos.color = Color.blue;
				Vector2 groundCheckPos = groundCheck.position;
				float topY = groundCheckPos.y + groundSize.y / 2;
				float bottomY = groundCheckPos.y - groundSize.y / 2;
				float leftX = groundCheckPos.x - groundSize.x / 2;
				float rightX = groundCheckPos.x + groundSize.x / 2;
				Vector2 topLeft = new Vector2(leftX, topY);
				Vector2 topRight = new Vector2(rightX, topY);
				Vector2 bottomLeft = new Vector2(leftX, bottomY);
				Vector2 bottomRight = new Vector2(rightX, bottomY);
				Gizmos.DrawLine(topLeft, topRight);
				Gizmos.DrawLine(topRight, bottomRight);
				Gizmos.DrawLine(bottomRight, bottomLeft);
				Gizmos.DrawLine(bottomLeft, topLeft);
			}

			// Draw The head check rectangle
			if (headCheck != null)
			{
				Gizmos.color = Color.red;
				Vector2 headCheckPos = headCheck.position;
				float topY = headCheckPos.y + headSize.y / 2;
				float bottomY = headCheckPos.y - headSize.y / 2;
				float leftX = headCheckPos.x - headSize.x / 2;
				float rightX = headCheckPos.x + headSize.x / 2;
				Vector2 topLeft = new Vector2(leftX, topY);
				Vector2 topRight = new Vector2(rightX, topY);
				Vector2 bottomLeft = new Vector2(leftX, bottomY);
				Vector2 bottomRight = new Vector2(rightX, bottomY);
				Gizmos.DrawLine(topLeft, topRight);
				Gizmos.DrawLine(topRight, bottomRight);
				Gizmos.DrawLine(bottomRight, bottomLeft);
				Gizmos.DrawLine(bottomLeft, topLeft);
			}
		}
	}
}