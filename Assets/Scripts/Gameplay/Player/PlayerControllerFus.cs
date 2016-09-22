using UnityEngine;
using System.Collections;

public class PlayerControllerFus : MonoBehaviour
{

	#region SETUP

	private Transform _transform;
	//private BoxCollider2D _collider;
    [HideInInspector]
	public ActorPhysics _physics;

	[SerializeField]
	private Transform _skin;
	[SerializeField]
	private Animator _anims;

	public Vector2 Position2D
	{
		get
		{
			return _transform.position;
		}
	}

	// Use this before initialization
	void Awake()
	{
		_transform = GetComponent<Transform>();
		//_collider = GetComponent<BoxCollider2D>();
		_physics = GetComponent<ActorPhysics>();
	}

	// Use this for initialization
	void Start()
	{
		_physics.OnGrounded += OnGrounded;
		_physics.OnAirborne += OnAirborne;
		_physics.OnSliding += OnSliding;

		_physics.IsSliding = false;
		_physics.IsGrounded = false;
		Facing = 1;

	}

	#endregion


	#region PHYSICS CALLBACKS

	private Vector2 positionBeforeJump;
	private Vector2 positionTargetJump;

	private float jumpTime = 1;
	private float jumpTimer = 2;

	[SerializeField]
	private LayerMask groundCastLayer;

	private void OnGrounded()
	{
		//StartCoroutine(GroundedUpdate());
		_anims.Play("Idle");
	}

	private void OnAirborne()
	{
		StartCoroutine(AirborneUpdate());
		_anims.Play("Airborne");
	}

	private void OnSliding()
	{
		StartCoroutine(SlidingUpdate());
		Facing = (int)_physics.HeadingX;
		_anims.Play("Sliding");
	}

	#endregion


	#region STATES/UPDATES

	// Update is called once per frame
	private bool airborneUpdateActive = false;
	private IEnumerator AirborneUpdate()
	{
		if(airborneUpdateActive)
			yield break;

		airborneUpdateActive = true;

		_physics.BackCast();

		while(!_physics.IsGrounded && !_physics.IsSliding)
		{
			jumpTimer += Time.deltaTime;

			if(jumpTimer > jumpTime)
			{
				_physics.ApplyGravity();

				if(_physics.HeadingY <= 0) // descending
				{
					_physics.DownCast();
					if(_physics.castResult.touched)
					{
						if(_physics.castResult.normalAngle <= walkableAngle)
						{
							_physics.MovementVector = Vector2.zero;
							_physics.IsGrounded = true;

						}
						else if(_physics.castResult.normalAngle < 90)
						{
							slidingAngle = _physics.castResult.normalAngle;
							_physics.MovementVector = ((Vector2)(Quaternion.Euler(0, 0, slidingAngle * -_physics.castResult.heading) * (Vector2.right * _physics.castResult.heading)) + Vector2.down) * slidingSpeed;
							_physics.IsSliding = true;

						}
					}
				}
				else // ascending 
				{
					_physics.UpCast();
					if(_physics.castResult.touched)
					{
						Vector2 tempVector = _physics.MovementVector;
						tempVector.y = -tempVector.y;
						_physics.MovementVector = tempVector;
					}
				}
			}
			else
			{
				float pourcent = jumpTimer / jumpTime;
				float pourcentBefor = Mathf.Max((jumpTimer - Time.deltaTime) / jumpTime, 0);

				Vector2 transitionPositionBefore = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcentBefor);
				transitionPositionBefore.y += JumpHeight * jumpCurve.Evaluate(pourcentBefor);

				Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
				transitionPosition.y += JumpHeight * jumpCurve.Evaluate(pourcent);

				//Appli mouvement
				_physics.MovementVector = (transitionPosition - transitionPositionBefore) * MovePower / Time.deltaTime;
				//_physics.movementVectorScaled = (transitionPosition - transitionPositionBefore);


				//ascending
				if(_physics.HeadingY > 0)
				{
					_physics.UpCast();
					if(_physics.castResult.touched)
					{
						Vector2 tempVector = _physics.MovementVector;
						tempVector.y = -tempVector.y;
						_physics.MovementVector = tempVector;

						if(pourcent < 0.5f)
						{
							//jumpTimer = jumpTime * (1 - pourcent);
							jumpTimer = 2;
						}
					}
				}
				//descending
				else
				{
					DescendingCheck();
				}
			}

			_physics.ForwardCast();

			yield return null;
		}

		airborneUpdateActive = false;
	}

	private void DescendingCheck()
	{
		_physics.DownCast();
		if(_physics.castResult.touched)
		{
			if(_physics.castResult.normalAngle <= walkableAngle)
			{
				_physics.MovementVector = Vector2.zero;
				_physics.IsGrounded = true;

			}
			else if(_physics.castResult.normalAngle < 90)
			{
				slidingAngle = _physics.castResult.normalAngle;
				_physics.MovementVector = ((Vector2)(Quaternion.Euler(0, 0, slidingAngle * -_physics.castResult.heading) * (Vector2.right * _physics.castResult.heading)) + Vector2.down) * slidingSpeed;
				_physics.IsSliding = true;

			}
		}
	}


	private float slidingAngle; // use this to know if we should recalculate the MovementVector

	IEnumerator SlidingUpdate()
	{
		while(_physics.IsSliding)
		{
			_physics.ForwardCast();

			_physics.DownCast();
			if(_physics.castResult.touched)
			{
				if(_physics.castResult.normalAngle <= walkableAngle)
				{
					_physics.IsGrounded = true;
					_physics.MovementVector = Vector2.zero;
				}
				else if(_physics.castResult.normalAngle < 90 && _physics.castResult.normalAngle != slidingAngle)
				{
					slidingAngle = _physics.castResult.normalAngle;
					_physics.MovementVector = ((Vector2)(Quaternion.Euler(0, 0, slidingAngle * -_physics.castResult.heading) * (Vector2.right * _physics.castResult.heading)) + Vector2.down) * slidingSpeed;
				}
			}
			else
			{
				_physics.IsSliding = false;
			}

			yield return null;
		}
	}

	#endregion


	#region POINTING

	[Header("Movement Properties"),SerializeField, Range(1, 89)]
	private float walkableAngle;
	[SerializeField]
	private float slidingSpeed;

	private RaycastHit2D hitRightDown, hitLeftDown;

	[Header("Jump Properties")]
	//[SerializeField]
	//private EasingType ease = EasingType.Linear;
	[SerializeField]
	private AnimationCurve jumpCurve;
	[SerializeField]
	private float MovePower = 1;
	[SerializeField]
	private float JumpMaxY = 10;
	[SerializeField]
	private float JumpMinY = 1;
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

	[Header("Target Jump Properties")]
	[SerializeField]
	private bool groundedJump = true;
	[SerializeField]
	[Range(-5,0)]
	private float LimiteMaxDownJump = 0;


	[SerializeField]
	Vector2 DecalJumpTarget = new Vector2(0, 1.4f);

	public void OnTouchingStart(Vector2 target)
	{
		if(_physics.IsGrounded)
		{
			jumpTimer = 0;

			Facing = (int)Mathf.Sign(target.x - _transform.position.x);

			positionBeforeJump = _transform.position;

			//Limite la distance de saut
			if(Vector2.Distance(target, _transform.position) > DistanceJumpMax)
			{
				target = (Vector2)_transform.position + (target - (Vector2)_transform.position).normalized * DistanceJumpMax;
			}

			//Ne saute jamais plus bas
			if(target.y < _transform.position.y + LimiteMaxDownJump)
			{
				positionTargetJump.Set(target.x, _transform.position.y);

#if UNITY_EDITOR
				gizLineC = gizLineA = positionTargetJump;
				gizLineD = gizLineB = target;
#endif
			}
			else if(groundedJump)
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
			else
			{
				positionTargetJump = target;
#if UNITY_EDITOR
				gizLineC = gizLineA = positionTargetJump;
				gizLineD = gizLineB = target;
#endif
			}

			positionTargetJump += DecalJumpTarget;

			//Calcul des paramètres du saut: hauteur et temps
			JumpHeight = Mathf.Clamp(Mathf.Abs(Vector2.Distance(positionTargetJump, positionBeforeJump)) * JumpYDistanceFactor, JumpMinY, JumpMaxY);
			jumpTime = Mathf.Clamp(Mathf.Abs(Vector2.Distance(positionTargetJump, positionBeforeJump)) * TimeFactorJumpDistance, TimeJumpMin, TimeJumpMax);

			_physics.IsGrounded = false;
		}
		else if(punchTimer <= 0f)
		{
			OnPunching(target.x >= Position2D.x ? true : false);
		}
	}

	public void OnTouchingStay(Vector2 target) // for air control
	{
		//MovementVector = (target - (Vector2)_transform.position).normalized;
	}

    #endregion


    #region TRIGGER REACTIONS

    void OnTriggerEnter2D(Collider2D co)
    {
        Debug.Log(co.name);
        co.GetComponent<ITrigger>().OnPlayerEnter(this);
    }
    void OnTriggerExit2D(Collider2D co)
    {
        //Debug.Log(co.name);
        co.GetComponent<ITrigger>().OnPlayerExit(this);
    }

    #endregion


    #region PUNCHING! + FACING

    [Header("Punch Properties"), SerializeField]
	private LayerMask punchLayer;
	[SerializeField]
	private float punchRadius;
	[SerializeField]
	private float punchDistance;
	[SerializeField]
	private float punchStartup;
	[SerializeField]
	private float punchRecovery;
	private float punchTimer;
	private RaycastHit2D[] punchHits;

	public void OnPunching(bool rightPunch) // from the buttons
	{
		if(punchTimer <= 0f && !_physics.IsSliding)
		{
			Facing = rightPunch ? 1 : -1;
			StartCoroutine(Punch());
		}
		/*else {
            //unavaible
            // punchTimer -= Time.deltaTime; //accelerate the recovery a bit?
        }*/
	}

	IEnumerator Punch()
	{
		punchTimer = punchStartup + punchRecovery;
		_anims.Play("PunchCooldown"); //hack-ish
		_anims.Play("PunchR");

		while(punchTimer > 0f) // recovery
		{
			while(punchTimer > punchRecovery) // startup
			{
				punchTimer -= Time.deltaTime;
				yield return null;
			}

			if(Physics2D.CircleCastNonAlloc(Position2D, punchRadius, Vector2.right * Facing, punchHits, punchDistance, punchLayer) > 0) // maybe have active frames?
			{
				// IPunchable punchable punchHit.collider.GetComponent<IPunchable>();
			}

			while(punchTimer > 0f) // recovery
			{
				punchTimer -= Time.deltaTime;
				yield return null;
			}
		}
	}

	private int Facing
	{
		get
		{
			return _facing;
		}
		set
		{
			if(value != _facing)
			{
				_facing = value;
				_skin.localScale = new Vector2(value, 1);
			}
		}
	}
	private int _facing;

	#endregion

	#region UNITY_EDITOR
#if UNITY_EDITOR

	private Vector2 gizLineA;
	private Vector2 gizLineB;
	private Vector2 gizLineC;
	private Vector2 gizLineD;

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
			end.y += JumpHeight * jumpCurve.Evaluate(i);

			Gizmos.DrawLine(start, end);

			start = end;
		}

		//Pronostique de direction près le saut
		Gizmos.color = Color.red;
		start = Vector2.Lerp(positionBeforeJump, positionTargetJump, 0.999f);
		start.y += JumpHeight * jumpCurve.Evaluate(0.999f);
		Gizmos.DrawLine(start, start + (end - start).normalized * 2);
	}
#endif
	#endregion
}
