using UnityEngine;
using System.Collections;

public class Projectile : DamageObject
{
	[SerializeField]
	public Vector2 speed = new Vector2(1,0);
	[SerializeField]
	private float lifeTime = 3;
	[SerializeField]
	private int lifeHit = 1;
	private bool alive = true;

	[SerializeField]
	private Animator _anims;

	public void GiveDirection(int facing)
	{
		speed.x *= facing;
		Vector3 direct = transform.localScale;
		direct.x *= facing;
		transform.localScale = direct;
	}

	protected override void GetDamage()
	{
		lifeHit--;
		if(lifeHit == 0)
		{
			Death();
		}
	}

	void Update () {
		if(alive)
		{
			transform.position += Time.deltaTime * (Vector3)speed;

			lifeTime -= Time.deltaTime;
			if(lifeTime <= 0)
			{
				Death();
			}
		}
	}

	void Death()
	{
		alive = false;
		Destroy(gameObject);
	}
}
