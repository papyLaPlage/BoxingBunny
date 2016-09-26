using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{

	#region SETUP

	//private Transform _transform;
	[HideInInspector]
	public ActorPhysics _physics;

	[SerializeField]
	private Transform _skin;
	[SerializeField]
	private Animator _anims;

	[Header("Movement Properties"),SerializeField, Range(1, 89)]
	private float walkableAngle;
	[SerializeField]
	private float slidingSpeed;

	void Awake()
	{
		//_transform = GetComponent<Transform>();
		_physics = GetComponent<ActorPhysics>();
	}

	void Start()
	{
		_physics.OnGrounded += OnGrounded;
		_physics.OnAirborne += OnAirborne;
		_physics.OnSliding += OnSliding;

		_physics.IsSliding = false;
		_physics.IsGrounded = false;
	}

	#endregion

	// Update is called once per frame
	void Update()
	{
		_physics.ApplyGravity();
	}

	private void OnGrounded()
	{
		StartCoroutine(GroundedUpdate());
		//_anims.Play("Idle");
	}

	private void OnAirborne()
	{
		slidingAngle = 0f;
		StartCoroutine(AirborneUpdate());
		//_anims.Play("Airborne");
	}

	private void OnSliding()
	{
		StartCoroutine(SlidingUpdate());
		//Facing = (int)_physics.HeadingX;
		//_anims.Play("Sliding");
	}

	#region STATES/UPDATES

	private bool airborneUpdateActive = false; // avoid frame-perfect double AirborneUpdate
	private IEnumerator AirborneUpdate()
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
					Vector2 tempVector = _physics.MovementVector;
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

	private void DownCast()
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


	private float slidingAngle; // use this to know if we should recalculate the MovementVector

	IEnumerator SlidingUpdate()
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


	IEnumerator GroundedUpdate()
	{
		_physics.ApplyGravity();
		while(_physics.IsGrounded)
		{
			_physics.DownCast();
			_physics.IsGrounded = _physics.castResult.touched;
			yield return null;
		}
	}

	#endregion
}
