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


	private Rigidbody2D rb;
	private InputAction moveAction;
	private InputAction jumpAction;
	private InputAction abilityAction;
	private Vector2 direction;


	private float jumpForce = 5.0f;
	private float speed = 5.0f;
	private float jumpHoldDuration = .5f;
	private float jumpDuration;
	private bool isJumping = false;
	private bool isGrounded = true;
	private int indexAbilities;


	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		moveAction = InputSystem.actions.FindAction("Move");
		jumpAction = InputSystem.actions.FindAction("Jump");
		abilityAction = InputSystem.actions.FindAction("Ability");
		jumpDuration = jumpHoldDuration;
		indexAbilities = 0;
		SetLightPlayerColor();
	}

	// Update is called once per frame
	void Update()
	{
		isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
		direction = moveAction.ReadValue<Vector2>();
		JumpInput();
		ChangeAbility();
	}

	void FixedUpdate()
	{
		rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocityY);

		if (isJumping)
		{
			rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
		}
	}

	void JumpInput()
	{
		if (jumpAction.WasPressedThisFrame() && isGrounded)
			isJumping = true;

		if (jumpAction.WasReleasedThisFrame() || jumpHoldDuration <= 0)
		{
			isJumping = false;
			jumpHoldDuration = jumpDuration;
		}

		if (isJumping)
			jumpHoldDuration -= Time.deltaTime;
	}

	void ChangeAbility()
	{
		if (abilityAction.WasPressedThisFrame())
		{
			indexAbilities = (indexAbilities + 1) % abilities.Count;
			SetLightPlayerColor();
		}
	}

	void OnDrawGizmos()
	{
		if (groundCheck == null)
			return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
	}

	void SetLightPlayerColor()
	{
		lightPlayer.color = abilities[indexAbilities].color;
	}
}
