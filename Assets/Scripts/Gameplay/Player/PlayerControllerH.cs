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
				_rigidbody2D.position = positionTargetJump;
				//_rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
			}
		}
		//redresse le lapin pour qu'il reste debout
		/*
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
		*/
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

	[SerializeField]
	private LayerMask groundCastLayer;
	private RaycastHit2D hitRight, hitLeft;

	public void OnTouchingStart(Vector2 target)
	{
		if (!isJumping)
		{
			isJumping = true;
			jumpTimer = 0;

			_rigidbody2D.velocity = Vector2.zero;
			//_rigidbody2D.angularVelocity = 0;
			//_rigidbody2D.rotation = 0;


			Vector2 targetDown = target + Vector2.down * 10;
			Vector2 VectorRight = Vector2.right * 0.5f;
			Vector2 VectorLeft = Vector2.left * 0.5f;

			hitRight = Physics2D.Linecast(target + VectorRight, targetDown + VectorRight, groundCastLayer);
			hitLeft = Physics2D.Linecast(target + VectorLeft, targetDown + VectorLeft, groundCastLayer);

			float positionTargetJumpRight;

			if (hitRight.collider != null && hitRight.transform.tag == "ground")
			{
				positionTargetJumpRight = hitRight.point.y;
			}
			else
			{
				positionTargetJumpRight = _transform.position.y - 3;
			}

			float positionTargetJumpLeft;

			if (hitLeft.collider != null && hitLeft.transform.tag == "ground")
			{
				positionTargetJumpLeft = hitLeft.point.y;
			}
			else
			{
				positionTargetJumpLeft = _transform.position.y - 3;
			}

			positionBeforeJump = _transform.position;

			positionTargetJump.x = target.x;
			positionTargetJump.y = positionTargetJumpRight > positionTargetJumpLeft ? positionTargetJumpRight : positionTargetJumpLeft;
			positionTargetJump.y += 0.7f;

			Flip(positionBeforeJump.x < positionTargetJump.x);

#if UNITY_EDITOR
			gizLineA = target + VectorRight;
			gizLineB.x = gizLineA.x;
			gizLineB.y = hitRight.point.y;
			gizLineC = target + VectorLeft;
			gizLineD.x = gizLineC.x;
			gizLineD.y = hitLeft.point.y;
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

	void OnTriggerEnter2D(Collider2D co)
	{
		Debug.Log(co.name);
	}

	#endregion

#if UNITY_EDITOR

	Vector2 gizLineA;
	Vector2 gizLineB;
	Vector2 gizLineC;
	Vector2 gizLineD;
	void OnDrawGizmos()
	{
		Gizmos.DrawLine(gizLineA, gizLineB);
		Gizmos.DrawLine(gizLineC, gizLineD);

		Gizmos.DrawLine(positionTargetJump + Vector2.right, positionTargetJump + Vector2.left);
		
	}
#endif
}
