using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

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
	private int jumpTime = 1;
	private float jumpTimer = 0;

	private bool isJumping = false;

    // Update is called once per frame
    void Update()
    {

		if (isJumping)
		{
			jumpTimer += Time.deltaTime;

			float pourcent = jumpTimer / jumpTime;

			Vector2 transitionPosition = Vector2.Lerp(positionBeforeJump, positionTargetJump, pourcent);
			transitionPosition.y += 4 * Mathf.Sin(Mathf.PI * pourcent);

			_transform.position = transitionPosition;

			if (jumpTimer >= jumpTime)
			{
				isJumping = false;
			}
		}

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

	public float floatHeight;
	public float liftForce;
	public float damping;
	public Rigidbody2D rb2D;

	public void OnTouchingStart(Vector2 target)
    {
		if (!isJumping)
		{
			isJumping = true;
			jumpTimer = 0;

			RaycastHit2D hit = Physics2D.Raycast(new Vector2(target.x, 30), -Vector2.up);
			if (hit.collider != null && hit.transform.tag == "ground")
			{
				positionTargetJump = hit.point;
				positionTargetJump.y += 1;
				//movementVector = (correctTarget - (Vector2)_transform.position).normalized;
			}
			else
			{
				positionTargetJump = new Vector2(target.x, _transform.position.y);
			}

			positionBeforeJump = _transform.position;
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
