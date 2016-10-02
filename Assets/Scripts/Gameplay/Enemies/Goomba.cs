using UnityEngine;
using System.Collections;

public class Goomba : Enemy
{

	#region SETUP

	/*protected override void Start()
	{
		base.Start();
	}*/

	#endregion

	#region PHYSICS CALLBACKS

	protected override void OnGrounded()
	{
		base.OnGrounded();
		//Debug.Log("OnGrounded");

		if(alive)
		{
			StartCoroutine(Move());
			_anims.Play("Move");
		}
	}

	protected override void OnAirborne()
	{
		base.OnAirborne();
		//Debug.Log("OnAirborne");
		if(alive)
			_anims.Play("Idle");
	}

	protected override void OnSliding()
	{
		base.OnSliding();
		//Debug.Log("OnSliding");
		if(alive)
			_anims.Play("Idle");
	}

	#endregion

	#region STATES/UPDATES

	[SerializeField]
	protected float speed = 1;

	protected virtual IEnumerator Move()
	{
		while(_physics.IsGrounded && alive)
		{
			_physics.ForwardCast();
			if(_physics.castResult.touched)
			{
				Facing = -Facing;
			}

			tempVector = _physics.MovementVector;
			tempVector.x = Facing * speed;
			_physics.MovementVector = tempVector;

			yield return null;
		}
	}

	protected override void Death()
	{
		base.Death();
		_physics.IsGrounded = false;
		_anims.Play("Dead");
	}

	#endregion
}
