using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Switch")]
public class SwitchAbility : ColorAbility
{
	public override void Activate(PlayerController player)
	{
		player.EnableSwitch();
	}

	public override void Desactivate(PlayerController player)
	{
		player.DisableSwitch();
	}
}
