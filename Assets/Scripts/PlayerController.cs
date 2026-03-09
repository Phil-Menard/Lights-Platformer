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
	private InputAction abilityAction;
	private InputAction pulseAction;
	private InputAction bounceAction;
	private InputAction switchAction;
	private Vector2 direction;


	private float jumpForce = 5.0f;
	private float speed = 5.0f;
	private float jumpHoldDuration = .5f;
	private float jumpDuration;
	private bool isJumping = false;
	private bool isGrounded = true;
	private int indexAbilities;
	[SerializeField] private int jumpCount;
	private int maxJump;
	private bool canPulse;
	private bool canBounce;
	private bool isBouncing;
	private bool canSwitch;
	private bool isPlatformsAEnabled;
	private enum physicsMaterials
	{
		noFriction,
		bounce
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
		abilityAction = InputSystem.actions.FindAction("Ability");
		pulseAction = InputSystem.actions.FindAction("Pulse");
		bounceAction = InputSystem.actions.FindAction("Bounce");
		switchAction = InputSystem.actions.FindAction("Switch");

		jumpDuration = jumpHoldDuration;
		indexAbilities = 0;
		jumpCount = 0;
		maxJump = 1;
		canPulse = false;
		canBounce = false;
		isBouncing = false;
		canSwitch = false;
		isPlatformsAEnabled = true;
		SetLightPlayerColor();
		abilities[indexAbilities].Activate(this);

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
		ChangeAbility();
		JumpInput();
		PulseInput();
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

	void PulseInput()
	{
		if (pulseAction.WasPressedThisFrame() && canPulse)
		{
			int currentIndexAbility = indexAbilities;
			Pulse(currentIndexAbility);
		}
	}

	void BounceInput()
	{
		if (bounceAction.IsPressed() && canBounce)
		{
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
		if (switchAction.WasPressedThisFrame() && canSwitch)
		{
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

	public void EnableBounce()
	{
		canBounce = true;
	}

	public void DisableBounce()
	{
		canBounce = false;
	}

	public void EnablePulse()
	{
		canPulse = true;
	}

	public void DisablePulse()
	{
		canPulse = false;
	}

	public void EnableSwitch()
	{
		canSwitch = true;
	}

	public void DisableSwitch()
	{
		canSwitch = false;
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
