using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(1, 0.1f);
	[SerializeField] private List<Color32> abilitiesColors;
	[SerializeField] private Light2D lightPlayer;
	[SerializeField] private GameObject[] platformsA;
	[SerializeField] private GameObject[] platformsB;
	[SerializeField] private float bouncingSpeed = 500.0f;

	private Rigidbody2D rb;
	private Vector2 direction;
	private PlayerInputs inputs;


	[SerializeField] private float speed = 5.0f;
	private float jumpForce = 5.0f;
	private float jumpHoldDuration = .5f;
	private float jumpDuration;
	private bool isJumping = false;
	private bool isGrounded = true;
	private bool isOnWall = false;
	private int jumpCount;
	private int maxJump;
	private bool isPlatformsAEnabled;
	private bool isGravityFlipped;
	private bool isBouncing;
	private bool propulse;

	private enum colors
	{
		doubleJump,
		bounce,
		switchPlatform,
		gravity
	}


	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		inputs = new PlayerInputs();
	}

	void OnEnable()
	{
		inputs.Enable();
	}

	void OnDisable()
	{
		inputs.Disable();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		jumpDuration = jumpHoldDuration;
		jumpCount = 0;
		maxJump = 1;
		isPlatformsAEnabled = true;
		isGravityFlipped = false;
		isBouncing = false;
		propulse = false;

		SetLightPlayerColor((int)colors.doubleJump);
		platformsA = GameObject.FindGameObjectsWithTag("PlatformA");
		platformsB = GameObject.FindGameObjectsWithTag("PlatformB");

		foreach (GameObject platform in platformsA)
		{
			platform.SetActive(true);
		}
		foreach (GameObject platform in platformsB)
		{
			platform.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!isGravityFlipped)
			isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

		if (isGrounded && !isJumping)
		{
			jumpCount = 0;
			if (isBouncing && !propulse)
				propulse = true;
		}

		direction = inputs.Player.Move.ReadValue<Vector2>();

		if (rb.linearVelocityY < 0 && !isGrounded)
			rb.gravityScale = 2;
		else
			rb.gravityScale = 1;

		JumpInput();
		BounceInput();
		SwitchInput();
		GravityInput();
	}

	void FixedUpdate()
	{
		if (!isGravityFlipped)
		{
			rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);

			if (isJumping)
				rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
		}
		else
		{
			rb.linearVelocity = new Vector2(rb.linearVelocityX, direction.x * speed);

			if (isJumping)
				rb.linearVelocity = new Vector2(-jumpForce, rb.linearVelocityY);
		}

		if (propulse)
		{
			if (!isGravityFlipped)
				rb.linearVelocity = new Vector2(rb.linearVelocityX, bouncingSpeed);
			else
				rb.linearVelocity = new Vector2(-bouncingSpeed, rb.linearVelocityY);
			propulse = false;
		}
	}

	void JumpInput()
	{
		if (inputs.Player.Jump.WasPressedThisFrame() && (isGrounded || jumpCount < maxJump))
		{
			isJumping = true;
			SetLightPlayerColor((int)colors.doubleJump);
			jumpCount++;
		}

		if (inputs.Player.Jump.WasReleasedThisFrame() || jumpHoldDuration <= 0)
		{
			isJumping = false;
			jumpHoldDuration = jumpDuration;
		}

		if (isJumping)
			jumpHoldDuration -= Time.deltaTime;
	}

	void BounceInput()
	{
		if (inputs.Player.Bounce.WasPressedThisFrame() && !isGrounded)
		{
			SetLightPlayerColor((int)colors.bounce);
			isBouncing = true;
		}
		
		if (inputs.Player.Bounce.WasReleasedThisFrame())
		{
			SetLightPlayerColor((int)colors.doubleJump);
			isBouncing = false;
		}
	}

	void SwitchInput()
	{
		if (inputs.Player.Switch.WasPressedThisFrame())
		{
			SetLightPlayerColor((int)colors.switchPlatform);
			isPlatformsAEnabled = !isPlatformsAEnabled;

			foreach (GameObject platform in platformsA)
			{
				platform.SetActive(isPlatformsAEnabled);
			}
			foreach (GameObject platform in platformsB)
			{
				platform.SetActive(!isPlatformsAEnabled);
			}
		}
	}

	void GravityInput()
	{
		if (inputs.Player.Gravity.WasPressedThisFrame())
		{
			SetLightPlayerColor((int)colors.gravity);
			isGravityFlipped = !isGravityFlipped;
			if (isGravityFlipped)
			{
				Physics2D.gravity = new Vector2(9.81f, 0);
				gameObject.transform.Rotate(new Vector3(0, 0, 90));
			}
			else
			{
				Physics2D.gravity = new Vector2(0, -9.81f);
				gameObject.transform.Rotate(new Vector3(0, 0, -90));
			}
		}
	}

	void OnDrawGizmos()
	{
		if (groundCheck == null)
			return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
	}

	void SetLightPlayerColor(int index)
	{
		lightPlayer.color = abilitiesColors[index];
	}
}