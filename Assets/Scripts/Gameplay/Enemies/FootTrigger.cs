using UnityEngine;
using System.Collections;

public class FootTrigger : MonoBehaviour {

	#region SETUP

	Enemy enemy;
	PlayerControllerFus player;

	void Awake () {
		enemy = transform.GetComponent<Enemy>();
		if(enemy == null)
			enemy = transform.GetComponentInParent<Enemy>();
		if(enemy == null)
		{
			Debug.Log("aucun script Enemy trouvé");
			Destroy(this);
		}
	}

	#endregion

	#region TRIGGER REACTIONS

	void OnTriggerEnter2D(Collider2D co)
	{
		player = co.transform.GetComponent<PlayerControllerFus>();
		if(player == null)
			player = co.transform.GetComponentInParent<PlayerControllerFus>();

		if(player != null)
		{
			player.AfterFootTouch();
			enemy.UpdateLife(-player.footDamage);
			if(!enemy.alive)
			{
				Destroy(this);
			}
		}
	}

	#endregion
}
