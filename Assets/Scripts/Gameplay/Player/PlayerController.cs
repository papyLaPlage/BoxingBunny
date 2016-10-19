using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActorPhysics))]
public class PlayerController : MonoBehaviour
{
	#region SETUP

	[SerializeField]
	private Collider2D punchCollider;
	[SerializeField]
	private Collider2D footCollider;

	private Transform _transform;
	[HideInInspector]
	public ActorPhysics _physics;

	[SerializeField]
	private Transform _skin;
	[SerializeField]
	private Animator _anims;

	public int lifes = 3;
	[SerializeField]
	private int pvMax = 3;
	public int pv = 3;

	[SerializeField,Range(0, 3)]
	private float noDamageTime = 1.0f;
	private float noDamageTimer = 0;

	public Vector2 Position2D
	{
		get
		{
			return _transform.position;
		}
	}

	void Awake()
	{
		_transform = GetComponent<Transform>();
		//_collider = GetComponent<BoxCollider2D>();
		_physics = GetComponent<ActorPhysics>();
	}

	void Start()
	{
		_physics.OnGrounded += OnGrounded;
		_physics.OnAirborne += OnAirborne;
		_physics.OnSliding += OnSliding;

		_physics.IsSliding = false;
		_physics.IsGrounded = false;
		Facing = 1;

		punchTime = punchStartDuration + punchDamageDuration + punchRecoveryDuration;
		punchStartDuration = punchDamageDuration + punchRecoveryDuration;
	}

	public void UpdateLife(int _pv)
	{
		pv += _pv;
		pv = Mathf.Clamp(pv, 0, pvMax);

		if(pv == 0)
		{
			lifes--;
			if(lifes == 0)
			{
				GameOver();
			}
			else
			{
				restart();
			}
		}
	}

	void restart()
	{

	}

	void GameOver()
	{

	}

	public void AfterFootTouch()
	{
		Vector2 tempVector = _physics.MovementVector;
		tempVector.y = Mathf.Abs(tempVector.y);
		_physics.MovementVector = tempVector;
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
		//Debug.Log("OnGrounded");
		footCollider.enabled = false;
		powerInAirActivate = false;
		_anims.Play("Idle");
		_physics.ApplyGravity();

		StartCoroutine(GroundedUpdate());
	}

	private void OnAirborne()
	{
		//Debug.Log("OnAirborne");
		slidingAngle = 0f;
		_anims.Play("Airborne");

		StartCoroutine(AirborneUpdate());
	}

	private void OnSliding()
	{
		//Debug.Log("OnSliding");
		footCollider.enabled = false;
		_anims.Play("Sliding");

		StartCoroutine(SlidingUpdate());
		Facing = (int)_physics.HeadingX;
	}

	#endregion


	#region STATES/UPDATES

	void StopJump(Vector2 newVector)
	{
		if(airborneUpdateActive)
		{
			jumpTimer = jumpTime + 1;
			if(newVector != null)
			{
				_physics.MovementVector = newVector;
			}
		}
	}

	private bool airborneUpdateActive = false; // avoid frame-perfect double AirborneUpdate
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
					DownCheck();
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
					_physics.ForwardCast();
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
				_physics.MovementVector = Time.deltaTime > 0f ? (transitionPosition - transitionPositionBefore) * MovePower / Time.deltaTime : Vector2.zero;
				//_physics.movementVectorScaled = (transitionPosition - transitionPositionBefore);

				if(_physics.HeadingY < 0) //descending
				{
					DownCheck();
				}
				else //ascending
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
					_physics.ForwardCast();
				}
			}

			yield return null;
		}

		airborneUpdateActive = false;
	}

	private void DownCheck()
	{
		footCollider.enabled = true;

		_physics.DownCast();

		if(_physics.castResult.touched)
		{
			if(_physics.castResult.normalAngle <= walkableAngle)
			{
				_physics.MovementVector = Vector2.zero;
				_physics.IsGrounded = true;
			}
			else if(_physics.castResult.normalAngle < 90 && _physics.castResult.normalAngle != slidingAngle)
			{
				slidingAngle = _physics.castResult.normalAngle;
				_physics.MovementVector = new Vector2(_physics.castResult.normal.y * _physics.castResult.heading, Mathf.Abs(_physics.castResult.normal.x) * -1) * slidingSpeed;
				_physics.ApplyGravity();
				_physics.IsSliding = true;
			}
		}
		else
			_physics.ForwardCast();
	}


	private float slidingAngle; // use this to know if we should recalculate the MovementVector

	IEnumerator SlidingUpdate()
	{
		while(_physics.IsSliding)
		{
			_physics.ForwardCast();

			DownCheck();
			if(!_physics.castResult.touched)
			{
				_physics.IsSliding = false;
				slidingAngle = 0f;
			}

			yield return null;
		}
	}

	IEnumerator GroundedUpdate()
	{
		while(_physics.IsGrounded)
		{
			_physics.DownCast();
			_physics.IsGrounded = _physics.castResult.touched;
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

	ITrigger iTrigger;
	void OnTriggerEnter2D(Collider2D co)
	{
		//Debug.Log(co.name);
		iTrigger = co.GetComponent<ITrigger>();
		if(iTrigger != null)
		{
			iTrigger.OnPlayerEnter(this);
		}
	}

	void OnTriggerExit2D(Collider2D co)
	{
		//Debug.Log(co.name);
		iTrigger = co.GetComponent<ITrigger>();
		if(iTrigger != null)
		{
			iTrigger.OnPlayerExit(this);
		}
	}

	#endregion


	#region PUNCHING! + FACING

	[Header("Punch Properties")]
	[SerializeField]
	private float punchStartDuration = 0.1f;
	[SerializeField]
	private float punchDamageDuration = 0.3f;
	[SerializeField]
	private float punchRecoveryDuration = 0.1f;
	private float punchTimer;
	private float punchTime;

	public void OnPunching(bool rightPunch) // from the buttons
	{
		if(punchTimer <= 0f && !_physics.IsSliding)
		{
			Facing = rightPunch ? 1 : -1;
			StartCoroutine(Punch());
		}
	}

	public Power powerActual = Power.Normal;
	[SerializeField]
	private float powerQuantity = 0;
	public float PowerQuantity
	{
		get
		{
			return powerQuantity;
		}

		set
		{
			powerQuantity = Mathf.Clamp(value, 0, powerQuantityMax);
		}
	}

	public float powerQuantityMax = 100;

	[SerializeField]
	private float powerLossSpeed = 10;

	[SerializeField]
	private GameObject projetileFire;
	[SerializeField]
	private GameObject projetileWater;
	[SerializeField]
	private GameObject projetileWind;
	[SerializeField]
	private GameObject projetileEarth;

	private bool powerActivate = false;
	private bool powerInAirActivate = false;

	public bool powerUpdateActivate = false;
	IEnumerator PowerUpdate()
	{
		if(powerUpdateActivate)
			yield break;

		powerUpdateActivate = true;

		while(powerQuantity > 0)
		{
			if(!powerUpdateActivate)
			{
				yield break;
			}
			powerQuantity -= Time.deltaTime * powerLossSpeed;
			yield return null;
		}

		powerActual = Power.Normal;
		powerUpdateActivate = false;
	}

	public void ActivePowerUpdate()
	{
		StartCoroutine(PowerUpdate());
	}

	private Projectile projetile;

	IEnumerator Punch()
	{
		_anims.Play("PunchCooldown"); //hack-ish
		_anims.Play("PunchR");

		powerActivate = false;
		punchTimer = punchTime;

		while(punchTimer > 0)
		{
			//préparation
			if(punchTimer > punchStartDuration)
			{
				//Debug.Log("punchStartDuration");
			}
			//punch
			else if(punchTimer >= punchRecoveryDuration)
			{
				//Debug.Log("punchDamageDuration");
				if(!powerActivate)
				{
					switch(powerActual)
					{
						case Power.Fire:
							//lance boule de feu
							projetile = Instantiate(projetileFire).GetComponent<Projectile>();
							break;
						case Power.Wind:
							//lance tornade
							projetile = Instantiate(projetileWind).GetComponent<Projectile>();
							if(!_physics.IsGrounded)
							{
								//saut vertical
							}
							break;
						case Power.Water:
							//lance une vague
							projetile = Instantiate(projetileWater).GetComponent<Projectile>();
							break;
						case Power.Earth:
							if(!_physics.IsGrounded && !_physics.IsSliding)
							{
								//smach vertical
							}
							break;
					}

					if(null != projetile)
					{
						projetile.transform.position = _transform.position;
						projetile.GiveDirection(_facing);
						projetile = null;
					}

					punchCollider.enabled = true;
					powerActivate = true;
					powerInAirActivate = true;
				}
			}
			//recovery
			else
			{
				//Debug.Log("punchRecoveryDuration");
				punchCollider.enabled = false;
			}

			punchTimer -= Time.deltaTime;
			yield return null;
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
