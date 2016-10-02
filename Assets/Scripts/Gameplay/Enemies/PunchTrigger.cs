using UnityEngine;
using System.Collections;

public class PunchTrigger : MonoBehaviour {

	#region SETUP

	Enemy enemy;
	PlayerControllerFus player;

	void Awake()
	{
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
			enemy.UpdateLife(-player.punchDamage);
		}
	}

	#endregion
}
