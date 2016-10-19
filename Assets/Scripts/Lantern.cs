﻿using UnityEngine;
using System.Collections;

public class Lantern : MonoBehaviour, ITrigger
{
	private Power power = Power.Fire;
	private bool active = true;
	private bool hasPlayer = false;
	private PlayerController _player;
	[SerializeField]
	private float refullSpeed = 100;

	#region ITrigger

	public void OnPlayerEnter(PlayerController player)
	{
		if(active)
		{
			_player = player;
			if(_player.powerActual != power)
			{
				_player.PowerQuantity = 0;
				_player.powerActual = power;
			}

			StartCoroutine(RefullPower());
		}
	}

	IEnumerator RefullPower()
	{
		if(hasPlayer)
			yield break;

		hasPlayer = true;

		while(hasPlayer)
		{
			if(_player.powerActual != power)
				yield break;

			_player.powerUpdateActivate = false;

			_player.PowerQuantity += Time.deltaTime * refullSpeed;
			yield return null;
		}
	}

	public void OnPlayerExit(PlayerController player)
	{
		_player.ActivePowerUpdate();
		hasPlayer = false;
	}

	#endregion
}
