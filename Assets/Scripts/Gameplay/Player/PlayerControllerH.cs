using UnityEngine;
using System.Collections;

public class PlayerControllerH : MonoBehaviour
{
	//TO DO
	//platforme traversable

	public EasingType ease = EasingType.Linear;


	#region SETUP

	private Transform _transform;
	//private BoxCollider2D _collider;
	private Rigidbody2D _rigidbody2D;

	// Use this for initialization
	void Awake()
	{
		_transform = GetComponent<Transform>();
		//_collider = GetComponent<BoxCollider2D>();
		_rigidbody2D = GetComponent<Rigidbody2D>();

		gravityScaleOrigine = _rigidbody2D.gravityScale;
	}

	#endregion


	#region UPDATE + PHYSICS

	private Vector2 positionBeforeJump;
	private Vector2 positionTargetJump;
	//private Vector2 transitionPosition = Vector2.zero;

	private float jumpTime = 1;
	private float jumpTimer = 0;
	private float gravityScaleOrigine = 2;

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

	public AnimationCurve jumpCurve;

	void FixedUpdate()
	{
		switch(state)
		{
			case States.Falling:
				_rigidbody2D.gravityScale = gravityScaleOrigine;
				colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.down * 0.75f, 0.5f, groundCastLayer);
				for(int i = 0; i < colliders.Length; i++)
				{
					if(colliders[i].gameObject != gameObject)
					{
						state = States.Grounded;
						_rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
						break;
					}
				}

				if(_rigidbody2D.velocity.y > 0)
				{
					_rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0);
				}

				break;

			case States.Grounded:

				_rigidbody2D.gravityScale = 0;
				state = States.Falling;

				colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.down * 0.75f, 0.5f, groundCastLayer);
				for(int i = 0; i < colliders.Length; i++)
				{
					if(colliders[i].gameObject != gameObject)
					{
						state = States.Grounded;
						break;
					}
				}

				_rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);

				break;

			case States.Jumping:

				jumpTimer += Time.deltaTime;

				if(jumpTimer > jumpTime)
				{
					state = States.Falling;
					Vector2 velocity = Vector2.down * 20f;
					//_rigidbody2D.velocity = Vector2.down * 20f;
				}
				else
				{
					_rigidbody2D.gravityScale = 0;

					float pourcent = jumpTimer / jumpTime;
					float pourcentBefor = Mathf.Max((jumpTimer - Time.deltaTime) / jumpTime, 0);

					Vector2 transitionPositionBefore = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcentBefor);
					transitionPositionBefore.y += JumpHeight * jumpCurve.Evaluate(Easing.EaseInOut(pourcentBefor, ease));

					Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
					transitionPosition.y += JumpHeight * jumpCurve.Evaluate(Easing.EaseInOut(pourcent, ease));

					//Appli mouvement
					_rigidbody2D.velocity = (transitionPosition - transitionPositionBefore) * MovePower;

					//collistion Au-dessus
					if(pourcent < 0.5f)
					{
						colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.up * 0.75f, 0.2f, groundCastLayer);
						for(int i = 0; i < colliders.Length; i++)
						{
							if(colliders[i].gameObject != gameObject)
							{
								jumpTimer = jumpTime * (1 - pourcent);
								break;
							}
						}
					}
					//collition Avant
					else
					{
						colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.right * 0.75f * _transform.localScale.x, 0.2f, groundCastLayer);
						for(int i = 0; i < colliders.Length; i++)
						{
							if(colliders[i].gameObject != gameObject)
							{
								state = States.Falling;
								//_rigidbody2D.velocity = Vector2.down * 20f;
								break;
							}
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

	private RaycastHit2D hitRightDown, hitLeftDown;

	[SerializeField]
	private float MovePower = 30;
	[SerializeField]
	private float JumpMaxY = 10;
	[SerializeField]
	private float JumpMinY = 2;
	[SerializeField]
	private float JumpYDistanceFactor = 0.5f;
	private float JumpHeight;

	[SerializeField]
	private float TimeJumpMax = 0.8f;
	[SerializeField]
	private float TimeJumpMin = 0.2f;
	[SerializeField]
	private float TimeFactorJumpDistance = 0.2f;
	[SerializeField]
	private float DistanceJumpMax = 7;

	[SerializeField]
	Vector2 DecalJumpTarget = new Vector2(0, 1.4f);

	public void OnTouchingStart(Vector2 target)
	{
		if(state == States.Grounded)
		{
			state = States.Jumping;
			jumpTimer = 0;

			positionBeforeJump = _transform.position;

			//capte distance de saut
			if(Vector2.Distance(target, _transform.position) > DistanceJumpMax)
			{
				target = (Vector2)_transform.position + (target - (Vector2)_transform.position).normalized * DistanceJumpMax;
			}

			//ne saute pas plus bas
			if(target.y < _transform.position.y)
			{
				positionTargetJump.Set(target.x, _transform.position.y);

#if UNITY_EDITOR
				gizLineC = gizLineA = positionTargetJump;
				gizLineD = gizLineB = target;
#endif
			}
			else
			{
				Vector2 targetDown = new Vector2(target.x, _transform.position.y);

				Vector2 VectorRight = Vector2.right * 0.5f;
				Vector2 VectorLeft = Vector2.left * 0.5f;

				hitRightDown = Physics2D.Linecast(target + VectorRight, targetDown + VectorRight, groundCastLayer);
				hitLeftDown = Physics2D.Linecast(target + VectorLeft, targetDown + VectorLeft, groundCastLayer);

				float positionTargetJumpRight;

				if(hitRightDown.collider != null && hitRightDown.transform.tag == "ground")
				{
					positionTargetJumpRight = hitRightDown.point.y;
#if UNITY_EDITOR
					gizLineB.y = hitRightDown.point.y;
#endif
				}
				else
				{
					positionTargetJumpRight = _transform.position.y;
#if UNITY_EDITOR
					gizLineB.y = targetDown.y;
#endif
				}

				float positionTargetJumpLeft;

				if(hitLeftDown.collider != null && hitLeftDown.transform.tag == "ground")
				{
					positionTargetJumpLeft = hitLeftDown.point.y;
#if UNITY_EDITOR
					gizLineD.y = hitLeftDown.point.y;
#endif
				}
				else
				{
					positionTargetJumpLeft = _transform.position.y;
#if UNITY_EDITOR
					gizLineD.y = targetDown.y;
#endif
				}

				positionTargetJump.Set(target.x, Mathf.Max(_transform.position.y, Mathf.Max(positionTargetJumpRight, positionTargetJumpLeft)));

#if UNITY_EDITOR
				gizLineA = target + VectorRight;
				gizLineB.x = targetDown.x + VectorRight.x;

				gizLineC = target + VectorLeft;
				gizLineD.x = targetDown.x + VectorLeft.x;
#endif
			}

			positionTargetJump += DecalJumpTarget;

			Flip(positionBeforeJump.x < positionTargetJump.x);

			//Calcul des paramètres du saut: hauteur et temps
			JumpHeight = Mathf.Clamp(Mathf.Abs(Vector2.Distance(positionTargetJump, positionBeforeJump)) * JumpYDistanceFactor, JumpMinY, JumpMaxY);
			jumpTime = Mathf.Clamp(Mathf.Abs(Vector2.Distance(positionTargetJump, positionBeforeJump)) * TimeFactorJumpDistance, TimeJumpMin, TimeJumpMax);
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

	private Vector2 gizLineA;
	private Vector2 gizLineB;
	private Vector2 gizLineC;
	private Vector2 gizLineD;
	private Vector2 lastVelocity;

	void OnDrawGizmos()
	{
		//LineCast
		Gizmos.color = Color.red;
		Gizmos.DrawLine(gizLineA, gizLineB);
		Gizmos.DrawLine(gizLineC, gizLineD);

		//atterisage
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(positionTargetJump + Vector2.right, positionTargetJump + Vector2.left);
		Gizmos.DrawLine(positionTargetJump + Vector2.up, positionTargetJump + Vector2.down);

		//Distance Jump
		Gizmos.DrawWireSphere(transform.position, DistanceJumpMax);

		//Last Distance Jump
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(positionBeforeJump, DistanceJumpMax);

		//Jump transition
		Gizmos.color = Color.black;
		Vector2 start = positionBeforeJump, end = Vector2.zero;
		for(float i = 0; i <= 1; i += 0.01f)
		{
			end = Vector2.Lerp(positionBeforeJump, positionTargetJump, i);
			end.y += JumpHeight * jumpCurve.Evaluate(Easing.EaseInOut(i, ease));

			Gizmos.DrawLine(start, end);

			start = end;
		}

		start = Vector2.Lerp(positionBeforeJump, positionTargetJump, 0.95f);
		start.y += JumpHeight * jumpCurve.Evaluate(Easing.EaseInOut(0.95f, ease));

		Gizmos.DrawLine(start, start + (end - start).normalized * 15);

		/*
		Vector3 origine = transform.position, start, end;
		origine.y += 20;
		start = origine;

		float sizeX = 20f;
		float sizeY = 10f;

		for(float i = 0; i <= 1; i+=0.01f)
		{
			end = origine;
			end.y += Easing.EaseInOut(i, EasingType.Elastic) * sizeY;
			end.x += i * sizeX;

			Gizmos.DrawLine(start, end);

			start = end;
		}

		Gizmos.DrawLine(origine, new Vector3(origine.x, origine.y + sizeY, origine.z));
		Gizmos.DrawLine(origine, new Vector3(origine.x + sizeX, origine.y, origine.z));
		Gizmos.DrawLine(new Vector3(origine.x + sizeX, origine.y + sizeY, origine.z), new Vector3(origine.x, origine.y + sizeY, origine.z));
		Gizmos.DrawLine(new Vector3(origine.x + sizeX, origine.y + sizeY, origine.z), new Vector3(origine.x + sizeX, origine.y, origine.z));
		*/
	}
#endif
}