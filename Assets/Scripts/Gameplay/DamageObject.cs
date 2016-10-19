using UnityEngine;
using System.Collections;

public class DamageObject : MonoBehaviour
{

	public Power powerType = Power.Normal;
	private int damage = 1;

	public int Damage
	{
		get
		{
			GetDamage();
			return damage;
		}

		set
		{
			damage = value;
		}
	}

	protected virtual void GetDamage()
	{

	}
}
