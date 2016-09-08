using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region SETUP

    private Transform _transform;
    private BoxCollider2D _collider;

    public Vector2 Position2D { get { return _transform.position; } }
    private Vector2 extentX;
    private Vector2 extentY;
    private Vector2 sizeX;
    private Vector2 sizeY;

    // Use this for initialization
    void Awake()
    {
        _transform = GetComponent<Transform>();
        _collider = GetComponent<BoxCollider2D>();
    }

    // Use this for initialization
    void Start()
    {
        sizeX = Vector2.right * _collider.size.x;
        sizeY = Vector2.up * _collider.size.y;
        extentX = sizeX * 0.5f;
        extentY = sizeY * 0.5f;

        HeadingX = HeadingY = 1;

        _isGrounded = true; // hack-ish
        IsGrounded = false;
    }

    private Vector2 tempVector;
    private float tempFloat;
    private int tempInt;

    #endregion


    #region UPDATE + PHYSICS

    [Header("Physics Settings"), SerializeField]
    private LayerMask groundCastLayer;
    [SerializeField, Range(0f, 0.5f)]
    private float boxOffset;
    private Vector2 currentOffset;

    private RaycastHit2D MainHit { get { return _mainHits[0]; } }
    private RaycastHit2D[] _mainHits = new RaycastHit2D[1];
    private RaycastHit2D SecondHit { get { return _secondHits[0]; } }
    private RaycastHit2D[] _secondHits = new RaycastHit2D[1];

    //private Vector2 previousPosition;

    private Vector2 MovementVector
    {
        get { return _movementVector; }
        set
        {
            _movementVector = value;
            movementVectorScaled = value * Time.deltaTime;

            if (HeadingX > 0) { if (value.x < 0) HeadingX = -1; }
            else if (value.x >= 0) HeadingX = 1;
            if (HeadingY > 0) { if (value.y < 0) HeadingY = -1; }
            else if (value.y >= 0) HeadingY = 1;
 
        }
    }
    private Vector2 _movementVector; // used to know the next movement, unscaled
    private Vector2 movementVectorScaled;

    private float HeadingX
    {
        get { return _heading.x; }
        set { _heading.x = value; }
    }
    private float HeadingY
    {
        get { return _heading.y; }
        set { _heading.y = value; }
    }
    private Vector2 _heading; // general direction, used mainly for calculation 

    private bool IsGrounded
    {
        get { return _isGrounded; }
        set
        {
            if(value != _isGrounded)
            {
                _isGrounded = value;
                if (value)
                {
                    //_transform.parent = MainHit.transform; // need to make sure the level objects are all normalized
                }
                else
                {
                    //_transform.parent = null;
                    StartCoroutine(AirborneUpdate());
                }     
            }
        }
    }
    private bool _isGrounded = true;
    private bool isSliding = false;

    // Update is called once per frame
    IEnumerator AirborneUpdate()
    {
        while (!IsGrounded)
        {
            if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.up * HeadingY, _mainHits, movementVectorScaled.y + boxOffset, groundCastLayer) > 0)
            {
                //Debug.DrawLine(Position2D + _heading * boxOffset, MainHit.point, Color.red);
                //Debug.DrawRay(MainHit.point, MainHit.normal, Color.green);

                _transform.position = MainHit.centroid - Vector2.up * boxOffset * HeadingY;

                if (HeadingY < 0) // descending
                {
                    tempFloat = Vector2.Angle(MainHit.normal, Vector2.up);
                    if (tempFloat <= 45f) // walkable
                    {
                        if (!ForwardBoxCast())
                            _transform.Translate(Vector2.right * movementVectorScaled.x);
                        MovementVector = Vector2.zero;
                        IsGrounded = true;
                    }
                    else if (tempFloat < 90f) // slidable (not walkable)
                    {
                        Vector3 tempVector = Vector3.Cross(MainHit.normal, _movementVector);
                        MovementVector = Vector3.Cross(tempVector, MainHit.normal) * HeadingX * (MainHit.normal.x >= 0 ? 1 : -1); // this is the movement vector transformed to a sliding vector
                    }
                    else // wall, or ceiling(!?)
                    {
                        if (!ForwardBoxCast())
                            _transform.Translate(Vector2.right * movementVectorScaled.x);
                    }
                }
                else // ascending, meaning ceiling
                {
                    tempVector = _movementVector;
                    tempVector.y = 0;
                    MovementVector = tempVector; // stopping upward momentum

                    if (!ForwardBoxCast())
                        _transform.Translate(Vector2.right * movementVectorScaled.x);
                }
            } // so, you touched nothing under
            else
            {
                if (!ForwardBoxCast()) // so, you touched nothing forward
                {   
                    _transform.Translate(movementVectorScaled);
                }
                else
                    _transform.Translate(Vector2.up * movementVectorScaled.y);
            }
            yield return false;
        }
    }

    bool ForwardBoxCast()
    {
        if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.right * HeadingX, _secondHits, movementVectorScaled.x + boxOffset, groundCastLayer) > 0) // finish forward movement
        {
            _transform.position = SecondHit.centroid - Vector2.right * boxOffset * HeadingX;
            return true;
        }
        return false;
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
        MovementVector = (target - (Vector2)_transform.position).normalized;
        if (HeadingY > 0)
            IsGrounded = false;
    }

    public void OnTouchingStay(Vector2 target)
    {
        MovementVector = (target - (Vector2)_transform.position).normalized;
    }

    #endregion


    #region TRIGGER REACTIONS

    void OnTriggerEnter2D(Collider2D co)
    {
        Debug.Log(co.name);
    }

    #endregion
}
