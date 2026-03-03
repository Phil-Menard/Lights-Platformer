using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Enlighten")]
public class EnlightenAbility : ColorAbility
{
	public override void Activate(PlayerController player)
	{
		// player.EnableDoubleJump();
		Debug.Log("Activate enlighten");
	}

	public override void Desactivate(PlayerController player)
	{
		// player.DisableDoubleJump();
		Debug.Log("Activate enlighten");
	}
}
