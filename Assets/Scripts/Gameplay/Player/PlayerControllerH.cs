using UnityEngine;
using System.Collections;

public class PlayerControllerH : MonoBehaviour
{

	#region SETUP

	private Transform _transform;
	private BoxCollider2D _collider;
	private Rigidbody2D _rigidbody2D;

	// Use this for initialization
	void Awake()
	{
		_transform = GetComponent<Transform>();
		_collider = GetComponent<BoxCollider2D>();
		_rigidbody2D = GetComponent<Rigidbody2D>();

		gravityScaleOrigine = _rigidbody2D.gravityScale;
	}

	#endregion


	#region UPDATE + PHYSICS

	private Vector2 positionBeforeJump;
	private Vector2 positionTargetJump;

	private float jumpTime = 1;
	private float jumpTimer = 0;

	private enum States
	{
		Grounded,
		Jumping,
		Falling
	}

	private States state = States.Falling;

	[SerializeField]
	private LayerMask groundCastLayer;

	private Collider2D[] colliders;

	private float gravityScaleOrigine = 2;

	private Vector2 transitionPosition = Vector2.zero;

	public float JumpMaxY = 10;
	public float JumpMinY = 2;
	public float JumpYDistanceFactor = 0.5f;
	public float MovePower = 30;


	void FixedUpdate()
	{
		switch (state)
		{
			case States.Falling:
			case States.Grounded:

			_rigidbody2D.gravityScale = gravityScaleOrigine;
			state = States.Falling;

			colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.down * 0.75f, 0.5f, groundCastLayer);
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].gameObject != gameObject)
					state = States.Grounded;
			}

			if (_rigidbody2D.velocity.y > 0)
			{
				_rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0);
			}

			break;

			case States.Jumping:
			
			if (jumpTimer >= jumpTime)
			{
				state = States.Falling;
				_rigidbody2D.velocity = Vector2.down * 20f;
			}
			else
			{
				_rigidbody2D.gravityScale = 0;

				float pourcent = jumpTimer / jumpTime;

				Vector2 transitionPositionBefore = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
				transitionPositionBefore.y +=
					Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) * JumpYDistanceFactor, JumpMinY, JumpMaxY)
					* Mathf.Sin(Mathf.PI * pourcent);

				jumpTimer += Time.deltaTime;

				pourcent = jumpTimer / jumpTime;

				Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
				transitionPosition.y += 
					Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) * JumpYDistanceFactor, JumpMinY, JumpMaxY) 
					* Mathf.Sin(Mathf.PI * pourcent);

				//Appli mouvement
				//_rigidbody2D.MovePosition(transitionPosition);
				_rigidbody2D.velocity = (transitionPosition - transitionPositionBefore) * MovePower;

				//collition Avant
				colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.right * 0.75f * _transform.localScale.x, 0.2f, groundCastLayer);
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].gameObject != gameObject)
					{
						state = States.Falling;
						_rigidbody2D.velocity = Vector2.down * 20f;
						break;
					}
				}

				//collistion Au-dessus
				colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.up * 0.75f, 0.2f, groundCastLayer);
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].gameObject != gameObject)
					{
						//state = States.Falling;
						//_rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x/2, -20);

						if (pourcent < 0.5f)
						{
							jumpTimer = jumpTime * (1 - pourcent);
						}
						break;
					}	
				}
			}

			break;

		}
	}

	#endregion


	#region PUNCHING!

	public void OnPunching(bool rightPunch)
	{
		Flip(rightPunch);
		Punch();
	}

	public void Punch()
	{

	}

	#endregion


	#region MOVING

	private float LimiteYTestedForJump;
	private RaycastHit2D hitRightDown, hitLeftDown, hitStartUp, hitEndUp;

	public float TimeJumpMax = 0.8f;
	public float TimeJumpMin = 0.2f;
	public float TimeFactorJumpDistance = 0.2f;

	public void OnTouchingStart(Vector2 target)
	{
		if (state == States.Grounded)
		{
			state = States.Jumping;
			jumpTimer = 0;

			transitionPosition = _transform.position;

			Vector2 targetDown = target + Vector2.down * 6.5f;
			Vector2 Up = Vector2.up * 6.5f;
			Vector2 VectorRight = Vector2.right * 0.5f;
			Vector2 VectorLeft = Vector2.left * 0.5f;

			hitRightDown = Physics2D.Linecast(target + VectorRight, targetDown + VectorRight, groundCastLayer);
			hitLeftDown = Physics2D.Linecast(target + VectorLeft, targetDown + VectorLeft, groundCastLayer);

			float positionTargetJumpRight;

			if (hitRightDown.collider != null && hitRightDown.transform.tag == "ground")
			{
				positionTargetJumpRight = hitRightDown.point.y;
			}
			else
			{
				positionTargetJumpRight = _transform.position.y;
			}

			float positionTargetJumpLeft;

			if (hitLeftDown.collider != null && hitLeftDown.transform.tag == "ground")
			{
				positionTargetJumpLeft = hitLeftDown.point.y;
			}
			else
			{
				positionTargetJumpLeft = _transform.position.y;
			}

			positionBeforeJump = _transform.position;

			positionTargetJump.x = target.x;
			positionTargetJump.y = positionTargetJumpRight > positionTargetJumpLeft ? positionTargetJumpRight : positionTargetJumpLeft;
			positionTargetJump.y += 1.4f;

			Flip(positionBeforeJump.x < positionTargetJump.x);

			jumpTime = Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) * TimeFactorJumpDistance, TimeJumpMin, TimeJumpMax);

			/*
			//test hauteur au début du saut
			float limiteYStart = 10, limiteYEnd = 10;
			hitStartUp = Physics2D.Linecast(_transform.position, Up + (Vector2)_transform.position, groundCastLayer);

			if (hitStartUp.collider != null && hitStartUp.transform.tag == "ground")
			{
				limiteYStart = hitStartUp.point.y - _transform.position.y;
			}

			//test hauteur à la fin du saut
			hitEndUp = Physics2D.Linecast(target, Up + target, groundCastLayer);
			if (hitEndUp.collider != null && hitEndUp.transform.tag == "ground")
			{
				limiteYEnd = hitEndUp.point.y - target.y;
			}

			LimiteYTestedForJump = Mathf.Min(limiteYStart, limiteYEnd) - 1.25f;*/

#if UNITY_EDITOR
			gizLineA = target + VectorRight;
			//gizLineB.x = gizLineA.x;
			//gizLineB.y = hitRightDown.point.y;
			gizLineB = targetDown + VectorRight;
			gizLineB.y = hitRightDown.point.y;

			gizLineC = target + VectorLeft;
			//gizLineD.x = gizLineC.x;
			//gizLineD.y = hitLeftDown.point.y;
			gizLineD = targetDown + VectorLeft;
			gizLineD.y = hitLeftDown.point.y;
#endif
		}
	}

	private void Flip(bool right)
	{
		int direction = right ? 1 : -1;
		_transform.localScale = new Vector3(direction, _transform.localScale.y, _transform.localScale.z);
	}

	public void OnTouchingStay(Vector2 target)
	{

	}

	#endregion


	#region TRIGGER REACTIONS

	/*void OnTriggerStay2D(Collider2D co)
	{
		if (co.tag == "ground")
		{
			state = States.Grounded;
		}
	}*/

	/*void OnTriggerExit2D(Collider2D co)
	{
		Debug.Log("Exit: " + co.tag);
		if (co.tag == "ground" && state != States.Jumping)
		{
			state = States.Falling;
		}
	}*/

	#endregion

#if UNITY_EDITOR

	Vector2 gizLineA;
	Vector2 gizLineB;
	Vector2 gizLineC;
	Vector2 gizLineD;
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(gizLineA, gizLineB);
		Gizmos.DrawLine(gizLineC, gizLineD);

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(positionTargetJump + Vector2.right, positionTargetJump + Vector2.left);
		Gizmos.DrawLine(positionTargetJump + Vector2.up, positionTargetJump + Vector2.down);
	}
#endif
}
