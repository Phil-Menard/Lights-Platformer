using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Pulse")]
public class PulseAbility : ColorAbility
{
	public override void Activate(PlayerController player)
	{
		player.EnablePulse();
	}

	public override void Desactivate(PlayerController player)
	{
		player.DisablePulse();
	}
}
