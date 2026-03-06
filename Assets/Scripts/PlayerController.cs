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


	private Rigidbody2D rb;
	private InputAction moveAction;
	private InputAction jumpAction;
	private InputAction abilityAction;
	private InputAction pulseAction;
	private Vector2 direction;


	private float jumpForce = 5.0f;
	private float speed = 5.0f;
	private float jumpHoldDuration = .5f;
	private float jumpDuration;
	private bool isJumping = false;
	private bool isGrounded = true;
	private int indexAbilities;
	private int jumpCount;
	private int maxJump;
	private bool canPulse;
	private float gravity = -9.81f;


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
		pulseAction = InputSystem.actions.FindAction("Pulse");

		jumpDuration = jumpHoldDuration;
		indexAbilities = 0;
		jumpCount = 0;
		maxJump = 1;
		canPulse = false;
		SetLightPlayerColor();
		abilities[indexAbilities].Activate(this);
	}

	// Update is called once per frame
	void Update()
	{
		isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
		if (isGrounded && !isJumping)
			jumpCount = 0;

		direction = moveAction.ReadValue<Vector2>();
		ChangeAbility();
		JumpInput();
		CheckPulse();
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
			if (jumpAction.IsPressed() && !isGrounded)
				Physics2D.gravity = new Vector2(0, gravity / 2.5f);
			else
				Physics2D.gravity = new Vector2(0, gravity);
			Debug.Log(Physics2D.gravity);
		}
	}

	void CheckPulse()
	{
		if (pulseAction.WasPressedThisFrame() && canPulse)
		{
			int currentIndexAbility = indexAbilities;
			Pulse(currentIndexAbility);
		}
	}

	void ChangeAbility()
	{
		if (abilityAction.WasPressedThisFrame())
		{
			abilities[indexAbilities].Desactivate(this);
			indexAbilities = (indexAbilities + 1) % abilities.Count;
			SetLightPlayerColor();
			abilities[indexAbilities].Activate(this);
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

	public void EnableDoubleJump()
	{
		maxJump = 2;
	}

	public void DisableDoubleJump()
	{
		maxJump = 1;
	}

	public void EnablePulse()
	{
		canPulse = true;
	}

	public void DisablePulse()
	{
		canPulse = false;
	}

	public void Pulse(int currentIndexAbility)
	{
		canPulse = false;
		StartCoroutine(PulseLight(currentIndexAbility));
	}

	IEnumerator PulseLight(int currentIndexAbility)
	{
		float startRadius = lightPlayer.pointLightOuterRadius;
		float targetRadius = 18f;

		//Increase
		float durationUp = 0.5f;
		float time = 0f;
		while (time < durationUp)
		{
			lightPlayer.pointLightOuterRadius = Mathf.SmoothStep(startRadius, targetRadius, time / durationUp);
			time += Time.deltaTime;
			yield return null;
		}
		lightPlayer.pointLightOuterRadius = targetRadius;

		//Hold
		yield return new WaitForSeconds(1.5f);

		//Decrease
		float durationDown = 0.5f;
		time = 0f;
		while (time < durationDown)
		{
			lightPlayer.pointLightOuterRadius = Mathf.SmoothStep(targetRadius, startRadius, time / durationDown);
			time += Time.deltaTime;
			yield return null;
		}
		lightPlayer.pointLightOuterRadius = startRadius;

		yield return new WaitForSeconds(1f);
		if (indexAbilities == currentIndexAbility)
			canPulse = true;
	}
}
