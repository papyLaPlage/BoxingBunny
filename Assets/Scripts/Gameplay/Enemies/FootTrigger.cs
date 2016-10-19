using UnityEngine;
using System.Collections;

public class FootTrigger : MonoBehaviour {

	#region SETUP

	Enemy enemy;
	DamageObject damageObject;

	void Awake ()
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
		damageObject = co.transform.GetComponent<DamageObject>();

		if(damageObject != null)
		{
			//damageObject.AfterFootTouch();
			enemy.UpdateLife(-damageObject.Damage);
			if(!enemy.alive)
			{
				Destroy(this);
			}
		}
	}

	#endregion
}
