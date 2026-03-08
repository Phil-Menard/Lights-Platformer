using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Bounce")]
public class BounceAbility : ColorAbility
{
	public override void Activate(PlayerController player)
	{
		player.EnableBounce();
	}

	public override void Desactivate(PlayerController player)
	{
		player.DisableBounce();
	}
}
