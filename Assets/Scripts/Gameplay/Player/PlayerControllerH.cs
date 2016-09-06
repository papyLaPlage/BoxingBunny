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
	}

	// Use this for initialization
	void Start()
	{

	}


	#endregion


	#region UPDATE + PHYSICS

	[SerializeField]
	private LayerMask groundCastLayer;
	private RaycastHit2D hit;

	private Vector2 positionBeforeJump;
	private Vector2 positionTargetJump;

	private float jumpTime = 1;
	private float jumpTimer = 0;

	private bool isJumping = false;
	private bool isFalling = false;

	private float redresTime = 0.3f;
	private float redresTimer = 0;
	private Quaternion lastRotation = Quaternion.identity;


	void Update()
	{
		//action de saut physiqué
		if (isJumping)
		{
			jumpTimer += Time.deltaTime;

			jumpTime = Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 20, 0.2f, 0.8f);

			float pourcent = jumpTimer / jumpTime;

			Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
			transitionPosition.y += Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 2, 2, 10) * Mathf.Sin(Mathf.PI * pourcent);

			_rigidbody2D.MovePosition(transitionPosition);

			if (jumpTimer >= jumpTime)
			{
				isJumping = false;
				//_rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
			}
		}
		//redresse le lapin pour qu'il reste debout
		else if (_rigidbody2D.rotation > 35 || _rigidbody2D.rotation < -35 || isFalling && !isJumping)
		{
			Debug.Log(_rigidbody2D.rotation);


			if (!isFalling)
			{
				redresTimer = 0;
				lastRotation = _transform.rotation;
			}

			_rigidbody2D.rotation = Quaternion.Lerp(lastRotation, Quaternion.identity, redresTimer / redresTime).eulerAngles.z;
			redresTimer += Time.deltaTime;

			isFalling = _rigidbody2D.rotation != 0;
		}

	}

	#endregion


	#region PUNCHING!

	public void OnPunching(bool rightPunch)
	{
		Punch();
	}
	public void Punch()
	{

	}

	#endregion


	#region MOVING

	public void OnTouchingStart(Vector2 target)
	{
		if (!isJumping)
		{
			isJumping = true;
			jumpTimer = 0;

			_rigidbody2D.velocity = Vector2.zero;
			_rigidbody2D.angularVelocity = 0;
			_rigidbody2D.rotation = 0;

			RaycastHit2D hit = Physics2D.Linecast(target, target + Vector2.down * 10, groundCastLayer);
			if (hit.collider != null && hit.transform.tag == "ground")
			{
				positionTargetJump = hit.point;
				positionTargetJump.y += 1;
			}
			else
			{
				positionTargetJump = new Vector2(target.x, _transform.position.y);
			}

			positionBeforeJump = _transform.position;

			if (positionBeforeJump.x < positionTargetJump.x)
			{
				_transform.localScale = new Vector3(1, _transform.localScale.y, _transform.localScale.z);
			}
			else
			{
				_transform.localScale = new Vector3(-1, _transform.localScale.y, _transform.localScale.z);
			}
		}
	}

	public void OnTouchingStay(Vector2 target)
	{

	}

	#endregion


	#region TRIGGER REACTIONS

	void OnTriggerEnter2D(Collider2D co)
	{
		Debug.Log(co.name);
	}

	#endregion
}
