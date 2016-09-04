using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	#region SETUP

	private Transform _transform;
	private BoxCollider2D _collider;
	//private Rigidbody2D _rigidbody2D;

	// Use this for initialization
	void Awake()
	{
		_transform = GetComponent<Transform>();
		_collider = GetComponent<BoxCollider2D>();
		//_rigidbody2D = GetComponent<Rigidbody2D>();
	}

	// Use this for initialization
	void Start()
	{

	}

	private Vector2 tempVector;
	private float tempFloat;
	private int tempInt;

	#endregion


	#region UPDATE + PHYSICS

	[SerializeField]
	private LayerMask groundCastLayer;
	private RaycastHit2D hit;

	private Vector2 movementAnchor;
	private Vector2 movementVector;

	private Vector2 positionBeforeJump;
	private Vector2 positionTargetJump;
	private float jumpTime = 1;
	private float jumpTimer = 0;

	private bool isJumping = false;
	private bool isFalling = false;

	private float redresTime = 0.3f;
	private float redresTimer = 0;
	private Quaternion lastRotation = Quaternion.identity;
	// Update is called once per frame
	void Update()
	{
		//action de saut physiqué
		if (isJumping)
		{
			jumpTimer += Time.deltaTime;

			jumpTime = Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 20, 0.2f, 0.8f);

			float pourcent = Mathf.Min(1, jumpTimer / jumpTime);

			Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
			transitionPosition.y += Mathf.Clamp(Mathf.Abs(positionTargetJump.x - positionBeforeJump.x) / 2, 2, 10) * Mathf.Sin(Mathf.PI * pourcent);


			_transform.position = transitionPosition;
			//_rigidbody2D.MovePosition(transitionPosition);

				if (jumpTimer >= jumpTime)
				{
					isJumping = false;
					//_rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
				}
		}
		//redresse le lapin pour qu'il reste debout
		else if(_transform.rotation.eulerAngles.z > 35 || _transform.rotation.eulerAngles.z < -35 || isFalling && !isJumping)
		{
			if (!isFalling)
			{
				redresTimer = 0;
				lastRotation = _transform.rotation;
			}

			_transform.rotation = Quaternion.Lerp(lastRotation, Quaternion.identity, redresTimer/redresTime);
			redresTimer += Time.deltaTime;

			isFalling = _transform.rotation.eulerAngles.z != 0;
		}


		//_transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.identity, 0.5f);

		//_rigidbody2D.angularVelocity -= Time.deltaTime;
		//_rigidbody2D.MoveRotation(0);

		/*
        if(movementVector.magnitude > 0f) // if you must, move it!
        {
            hit = Physics2D.BoxCast((Vector2)_transform.position, _collider.size, 0f, movementVector, movementVector.magnitude * Time.deltaTime, groundCastLayer);

            movementAnchor = movementVector * (_collider.bounds.extents.magnitude / movementVector.magnitude);
            Debug.DrawLine((Vector2)_transform.position, (Vector2)_transform.position + movementAnchor + movementVector * Time.deltaTime, Color.green);

            if (hit.centroid.magnitude > 0f) // hit something
            {
                if (Vector2.Angle(hit.normal, Vector2.up) == 90f) // wall
                {
                    tempFloat = _transform.position.y + (movementVector * Time.deltaTime).y - hit.centroid.y;
                    tempVector = new Vector2(0f, tempFloat);//.up * tempFloat;
                    _transform.position = hit.centroid;

                    hit = Physics2D.Raycast((Vector2)_transform.position, tempVector, tempVector.magnitude, groundCastLayer);

                    movementAnchor = tempVector * (_collider.bounds.extents.magnitude / tempVector.magnitude);
                    Debug.DrawLine((Vector2)_transform.position, (Vector2)_transform.position + movementAnchor + tempVector, Color.blue);

                    if (hit.centroid.magnitude > 0f) // hit something again, no second check
                    {
                        _transform.position = hit.point - _collider.offset;
                    }
                    else
                    {
                        _transform.Translate(tempVector);
                    }
                }
                else
                { 
				//if(Vector2.Angle(hit.normal, Vector2.up) == 0f) // grounded flat
                    _transform.position = hit.centroid;
                }
            }
            else
                _transform.Translate(movementVector * Time.deltaTime);
        }
		*/
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

	public LayerMask mask;

	public void OnTouchingStart(Vector2 target)
	{
		if (!isJumping)
		{
			isJumping = true;
			jumpTimer = 0;
			

			RaycastHit2D hit = Physics2D.Linecast(target, target + Vector2.down * 30, mask.value);
			if (hit.collider != null)
			{
				Debug.Log(mask.value);
				positionTargetJump = hit.point;
				positionTargetJump.y += 0.75f;
				//movementVector = (correctTarget - (Vector2)_transform.position).normalized;
			}
			else
			{
				positionTargetJump = new Vector2(target.x, _transform.position.y);
			}

			positionBeforeJump = _transform.position;

			if (positionBeforeJump.x < positionTargetJump.x) {
				_transform.localScale = new Vector3(1, _transform.localScale.y, _transform.localScale.z);
			} 
			else {
				_transform.localScale = new Vector3(-1, _transform.localScale.y, _transform.localScale.z);
			}
		}
	}

	public void OnTouchingStay(Vector2 target)
	{
		movementVector = (target - (Vector2)_transform.position).normalized;
	}

	#endregion


	#region TRIGGER REACTIONS

	void OnTriggerEnter2D(Collider2D co)
	{
		Debug.Log(co.name);
	}

	#endregion
}
