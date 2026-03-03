using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Double Jump")]
public class DoubleJumpAbility : ColorAbility
{
	public override void Activate(PlayerController player)
	{
		player.EnableDoubleJump();
	}

	public override void Desactivate(PlayerController player)
	{
		player.DisableDoubleJump();
	}
}
