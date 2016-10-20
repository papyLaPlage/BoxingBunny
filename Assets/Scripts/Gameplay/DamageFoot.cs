using UnityEngine;
using System.Collections;

public class DamageFoot : DamageObject
{
	PlayerController player;

	// Use this for initialization
	void Start () {
		player = GetComponentInParent<PlayerController>();
	}

	protected override void GetDamage()
	{
		player.AfterFootTouch();
	}
}
