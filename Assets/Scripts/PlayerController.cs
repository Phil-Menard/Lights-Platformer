using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private InputActionAsset inputActions;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(1, 0.1f);
	[SerializeField] private List<ColorAbility> abilities;
	[SerializeField] private Light2D lightPlayer;
	[SerializeField] private List<PhysicsMaterial2D> pMaterials;
	[SerializeField] private GameObject[] platformsA;
	[SerializeField] private GameObject[] platformsB;

	private Rigidbody2D rb;
	private InputAction moveAction;
	private InputAction jumpAction;
	private InputAction bounceAction;
	private InputAction switchAction;
	private Vector2 direction;


	private float jumpForce = 5.0f;
	private float speed = 5.0f;
	private float jumpHoldDuration = .5f;
	private float jumpDuration;
	private bool isJumping = false;
	private bool isGrounded = true;
	private int jumpCount;
	private int maxJump;
	private bool isBouncing;
	private bool isPlatformsAEnabled;
	private enum physicsMaterials
	{
		noFriction,
		bounce
	}

	private enum ability
	{
		doubleJump,
		bounce,
		switchPlatform
	}


	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		moveAction = InputSystem.actions.FindAction("Move");
		jumpAction = InputSystem.actions.FindAction("Jump");
		bounceAction = InputSystem.actions.FindAction("Bounce");
		switchAction = InputSystem.actions.FindAction("Switch");

		jumpDuration = jumpHoldDuration;
		jumpCount = 0;
		maxJump = 2;
		isBouncing = false;
		isPlatformsAEnabled = true;
		SetLightPlayerColor((int)ability.doubleJump);

		platformsA = GameObject.FindGameObjectsWithTag("PlatformA");
		platformsB = GameObject.FindGameObjectsWithTag("PlatformB");
		foreach (GameObject platform in platformsB)
		{
			platform.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update()
	{
		isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
		if (isGrounded && !isJumping)
		{
			if (isBouncing && jumpCount > 0)
				jumpCount = 1;
			else
				jumpCount = 0;
		}

		direction = moveAction.ReadValue<Vector2>();
		JumpInput();
		BounceInput();
		SwitchInput();
	}

	void FixedUpdate()
	{
		rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);

		if (isJumping)
			rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
	}

	void JumpInput()
	{
		if (jumpAction.WasPressedThisFrame() && (isGrounded || jumpCount < maxJump))
		{
			isJumping = true;
			SetLightPlayerColor((int)ability.doubleJump);
			jumpCount++;
		}

		if (jumpAction.WasReleasedThisFrame() || jumpHoldDuration <= 0)
		{
			isJumping = false;
			jumpHoldDuration = jumpDuration;
		}

		if (isJumping)
			jumpHoldDuration -= Time.deltaTime;
		else
		{
			if ((jumpAction.IsPressed() || isBouncing) && !isGrounded)
				rb.gravityScale = 0.5f;
			else
				rb.gravityScale = 1;
		}
	}

	void BounceInput()
	{
		if (bounceAction.IsPressed())
		{
			SetLightPlayerColor((int)ability.bounce);
			rb.sharedMaterial = pMaterials[(int)physicsMaterials.bounce];
			isBouncing = true;
		}
		else
		{
			rb.sharedMaterial = pMaterials[(int)physicsMaterials.noFriction];
			isBouncing = false;
		}
	}

	void SwitchInput()
	{
		if (switchAction.WasPressedThisFrame())
		{
			SetLightPlayerColor((int)ability.switchPlatform);
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

	void OnDrawGizmos()
	{
		if (groundCheck == null)
			return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
	}

	void SetLightPlayerColor(int index)
	{
		lightPlayer.color = abilities[index].color;
	}
}