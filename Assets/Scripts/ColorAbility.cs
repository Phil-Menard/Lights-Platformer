using UnityEngine;

public abstract class ColorAbility : ScriptableObject
{
	public Color color;

	public abstract void Activate(PlayerController player);
	public abstract void Desactivate(PlayerController player);
}
