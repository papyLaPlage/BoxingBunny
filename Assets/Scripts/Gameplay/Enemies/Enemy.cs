using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActorPhysics))]
public class Enemy : MonoBehaviour
{
	#region SETUP

	protected Transform _transform;
	[HideInInspector]
	public ActorPhysics _physics;

	[SerializeField]
	protected Transform _skin;
	[SerializeField]
	protected Animator _anims;

	[SerializeField,Range(1, 30)]
	protected int pvMax = 1;
	protected int pv = 1;
	[SerializeField,Range(0, 30)]
	protected int damage = 1;

	[SerializeField,Range(0, 2)]
	protected float noDamageTime = 0.5f;
	protected float noDamageTimer = 0;

	public bool alive = true;

	[Header("Movement Properties"), SerializeField, Range(1, 89)]
	protected float walkableAngle;
	[SerializeField]
	protected float slidingSpeed;

	public Vector2 Position2D
	{
		get
		{
			return _transform.position;
		}
	}
	protected Vector2 tempVector;

	protected virtual void Awake()
	{
		_transform = GetComponent<Transform>();
		_physics = GetComponent<ActorPhysics>();

		pv = pvMax;
	}

	protected virtual void Start()
	{
		_physics.OnGrounded += OnGrounded;
		_physics.OnAirborne += OnAirborne;
		_physics.OnSliding += OnSliding;

		_physics.IsSliding = false;
		_physics.IsGrounded = false;
	}

	protected virtual void Update()
	{
		if(noDamageTimer > 0)
		{
			noDamageTimer -= Time.deltaTime;
		}
	}

	public void UpdateLife(int _pv)
	{
		if(noDamageTimer <= 0)
		{
			noDamageTimer = noDamageTime;

			pv += _pv;
			pv = Mathf.Clamp(pv, 0, pvMax);

			if(alive && pv == 0)
			{
				Death();
			}
		}
	}

	protected virtual void Death()
	{
		alive = false;
	}

	#endregion

	#region PHYSICS CALLBACKS

	protected virtual void OnGrounded()
	{
		//Debug.Log("base:OnGrounded");
		StartCoroutine(GroundedUpdate());
	}

	protected virtual void OnAirborne()
	{
		//Debug.Log("base:OnAirborne");
		slidingAngle = 0f;
		StartCoroutine(AirborneUpdate());
	}

	protected virtual void OnSliding()
	{
		//Debug.Log("base:OnSliding");
		StartCoroutine(SlidingUpdate());
		Facing = (int)_physics.HeadingX;
	}

	#endregion

	#region STATES/UPDATES

	protected bool airborneUpdateActive = false; // avoid frame-perfect double AirborneUpdate
	protected virtual IEnumerator AirborneUpdate()
	{
		if(airborneUpdateActive)
			yield break;

		airborneUpdateActive = true;

		_physics.BackCast();

		while(!_physics.IsGrounded && !_physics.IsSliding)
		{
			if(_physics.HeadingY <= 0) // descending
			{
				DownCast();
			}
			else // ascending 
			{
				_physics.UpCast();
				if(_physics.castResult.touched)
				{
					tempVector = _physics.MovementVector;
					tempVector.y = -tempVector.y;
					_physics.MovementVector = tempVector;
				}
			}

			_physics.ForwardCast();
			_physics.ApplyGravity();

			yield return null;
		}

		airborneUpdateActive = false;
	}

	protected void DownCast()
	{
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


	protected float slidingAngle; // use this to know if we should recalculate the MovementVector

	protected virtual IEnumerator SlidingUpdate()
	{
		while(_physics.IsSliding)
		{
			_physics.ForwardCast();

			DownCast();
			if(!_physics.castResult.touched)
			{
				_physics.IsSliding = false;
				slidingAngle = 0f;
			}

			yield return null;
		}
	}


	protected virtual IEnumerator GroundedUpdate()
	{
		_physics.ApplyGravity();
		while(_physics.IsGrounded)
		{
			_physics.DownCast();
			_physics.IsGrounded = _physics.castResult.touched;
			yield return null;
		}
	}

	protected int Facing
	{
		get
		{
			return _facing;
		}
		set
		{
			if(value != _facing)
			{
				_facing = (int)Mathf.Sign(value);
				_skin.localScale = new Vector2(value, 1);
			}
		}
	}
	[SerializeField, Range(-1,1)]
	protected int _facing = 1;

	#endregion

	#region TRIGGER REACTIONS

	void OnTriggerEnter2D(Collider2D co)
	{
		if(co.tag == "Player")
		{
			co.GetComponent<PlayerController>().UpdateLife(damage);
		}
	}

	#endregion
}
