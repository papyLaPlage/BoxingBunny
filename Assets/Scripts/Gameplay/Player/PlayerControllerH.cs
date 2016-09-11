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

	private enum States
	{
		Grounded,
		Jumping,
		Falling
	}

	private States state = States.Falling;

	private float redresTime = 0.3f;
	private float redresTimer = 0;
	private Quaternion lastRotation = Quaternion.identity;

	[SerializeField]
	private LayerMask groundCastLayer;

	private Collider2D[] colliders;
	void FixedUpdate()
	{
		switch (state)
		{
			case States.Falling:
			case States.Grounded:

			_rigidbody2D.gravityScale = 2;
			state = States.Falling;

			colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.down * 0.75f, 0.5f, groundCastLayer);
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].gameObject != gameObject)
					state = States.Grounded;
			}

			break;

			case States.Jumping:

			_rigidbody2D.gravityScale = 0;

			jumpTimer += Time.deltaTime;
			
			if (jumpTimer >= jumpTime)
			{
				state = States.Falling;
				_rigidbody2D.position = positionTargetJump;
				_rigidbody2D.velocity = Vector2.down * 20f;
			}
			else
			{
				float pourcent = jumpTimer / jumpTime;

				Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
				transitionPosition.y += Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 2, 2, 10) * Mathf.Sin(Mathf.PI * pourcent);

				_rigidbody2D.MovePosition(transitionPosition);

				colliders = Physics2D.OverlapCircleAll((Vector2)_transform.position + Vector2.right * 0.75f * _transform.localScale.x, 0.2f, groundCastLayer);
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].gameObject != gameObject)
						state = States.Falling;
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

	private RaycastHit2D hitRight, hitLeft;

	public void OnTouchingStart(Vector2 target)
	{
		if (state == States.Grounded)
		{
			state = States.Jumping;
			jumpTimer = 0;

			Vector2 targetDown = target + Vector2.down * 1;
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
				positionTargetJumpRight = _transform.position.y;
			}

			float positionTargetJumpLeft;

			if (hitLeft.collider != null && hitLeft.transform.tag == "ground")
			{
				positionTargetJumpLeft = hitLeft.point.y;
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

			jumpTime = Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 20, 0.2f, 0.8f);

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
		Gizmos.DrawLine(gizLineA, gizLineB);
		Gizmos.DrawLine(gizLineC, gizLineD);

		Gizmos.DrawLine(positionTargetJump + Vector2.right, positionTargetJump + Vector2.left);
		
	}
#endif
}
