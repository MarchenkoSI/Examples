using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Database/AbilityDatabase")]
public class AbilityDatabase : ScriptableObject
{
	#region singleton
	private static AbilityDatabase instance;
	public static AbilityDatabase Instance
	{
		get
		{
			if (instance == null)
				instance = Resources.Load("Databases/AbilityDatabase") as AbilityDatabase;

			return instance;
		}
	}
	#endregion

	public Ability[] abilities;

	public Ability Get(int index)
	{
		return (abilities[index]);
	}

	public Ability GetByUUID(string UUID)
	{
		for (int i = 0; i < this.abilities.Length; i++)
		{
			if (this.abilities[i].UUID == UUID)
				return this.abilities[i];
		}

		return null;
	}

	public void ResetPlayerAutoAttack()
    {
		abilities[0].Reset();

	}
}
