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
    private int tempShort;
    private bool tempBool;

    #endregion


    #region UPDATE + PHYSICS

    [Header("Physics Settings"), SerializeField]
    private LayerMask solidCastLayer;
    [SerializeField]
    private LayerMask groundCastLayer;
    [SerializeField, Range(0f, 0.5f)]
    private float boxOffset;
    private Vector2 currentOffset;

    private RaycastHit2D MainHit { get { return _mainHits[0]; } }
    private RaycastHit2D[] _mainHits = new RaycastHit2D[1];
    private RaycastHit2D SecondHit { get { return _secondHits[0]; } }
    private RaycastHit2D[] _secondHits = new RaycastHit2D[1];

    private Vector2 mainCastOrigin;
    private Vector2 frontCastOrigin;
    private Vector2 backCastOrigin;

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
            //MovementVector -= Vector2.up * 5 * Time.deltaTime; // Apply Dat Gravity

            mainCastOrigin = Position2D + extentX * HeadingX + extentY * HeadingY;
            frontCastOrigin = mainCastOrigin - sizeY * HeadingY;
            backCastOrigin = mainCastOrigin - sizeX * HeadingX;

            Debug.DrawLine(Position2D, mainCastOrigin, Color.black);
            Debug.DrawLine(Position2D, frontCastOrigin, Color.black);
            Debug.DrawLine(Position2D, backCastOrigin, Color.black);
            Debug.DrawLine(mainCastOrigin, mainCastOrigin + Vector2.down * (boxOffset - movementVectorScaled.y), Color.red);
            Debug.DrawLine(backCastOrigin, backCastOrigin + Vector2.down * (boxOffset - movementVectorScaled.y), Color.red);
            Debug.DrawLine(mainCastOrigin, mainCastOrigin + Vector2.right * (boxOffset + movementVectorScaled.x) * HeadingX, Color.blue);
            Debug.DrawLine(frontCastOrigin, frontCastOrigin + Vector2.right * (boxOffset + movementVectorScaled.x) * HeadingX, Color.blue);

            if (HeadingY < 0) // descending
            {
                if (DownCast())
                {
                    // grounded
                }
            }
            else if(UpCast()) // ascending
            {
                tempVector = MovementVector;
                tempVector.y = -tempVector.y;
                MovementVector = tempVector;
            }

            ForwardCast();

            yield return null;
        }
    }

    bool ForwardCast()
    {
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.right * HeadingX, _mainHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(frontCastOrigin, Vector2.right * HeadingX, _secondHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
            {
                if (HeadingX > 0) { // facing right
                    if(SecondHit.point.x < MainHit.point.x) // know where you're blocked first
                        _transform.Translate(Vector2.right * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
                    else
                        _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
                }
                else // facing left
                {
                    if (MainHit.point.x <= SecondHit.point.x) // know where you're blocked first
                        _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
                    else
                        _transform.Translate(Vector2.right * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x * 2f));
                }
                return true;
            }
            else // no hesitation
            {
                _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            }
            return true;
        }
        else if (Physics2D.RaycastNonAlloc(frontCastOrigin, Vector2.right * HeadingX, _mainHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
        {
            _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            return true;
        }
        
        _transform.Translate(Vector2.right * movementVectorScaled.x);  
        return false;
    }
    bool BackCast() // this one should not be called every frame
    {
        if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.left * HeadingX, _secondHits, boxOffset, solidCastLayer) > 0)
        {
            _transform.Translate(Vector2.left * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            return true;
        }

        //_transform.Translate(Vector2.right * movementVectorScaled.x);
        return false;
    }

    bool DownCast()
    {
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.down, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.down, _secondHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
            {
                if (SecondHit.point.y > MainHit.point.y) // know where you're blocked first
                    _transform.Translate(Vector2.up * ((SecondHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
                else
                    _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
            }
            else // no hesitation
            {
                _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
            }
            return true;
        }
        else if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.down, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
            return true;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);  
        return false;
    }
    bool UpCast()
    {
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.up, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.up, _secondHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
            {
                if (SecondHit.point.y > MainHit.point.y) // know where you're blocked first
                    _transform.Translate(Vector2.up * ((SecondHit.point.y - (boxOffset + extentY.y)) - Position2D.y));
                else
                    _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y)) - Position2D.y));
            }
            else // no hesitation
            {
                _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y)) - Position2D.y));
            }
            return true;
        }
        else if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.up, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y)) - Position2D.y));
            return true;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);
        return false;
    }

    /*bool ForwardBoxCast()
    {
        if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.right * HeadingX, _secondHits, movementVectorScaled.x + boxOffset, groundCastLayer) > 0) // finish forward movement
        {
            Debug.Log(((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            //Debug.DrawLine(Vector2.up, Vector2.up + Vector2.right * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x), Color.blue);
            Debug.DrawLine(SecondHit.point, SecondHit.point + SecondHit.normal, Color.green);
            //Debug.Break();
            _transform.Translate(Vector2.right * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            return true;
        }
        return false;
    }
    bool BackBoxCast()
    {
        if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.left * HeadingX, _secondHits, movementVectorScaled.x + boxOffset, groundCastLayer) > 0) // finish forward movement
        {
            _transform.Translate(Vector2.left * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            return true;
        }
        return false;
    }

    bool DownBoxCast()
    {
        if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.up * HeadingY, _mainHits, movementVectorScaled.y + boxOffset, groundCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y) * HeadingY) - Position2D.y));

            if (HeadingY < 0) // descending
            {
                tempFloat = Vector2.Angle(MainHit.normal, Vector2.up);
                if (tempFloat <= 45f) // walkable
                {
                    IsGrounded = true;
                }
                else if (tempFloat < 90f) // slidable (not walkable)
                {
                    //Vector3 tempVector = Vector3.Cross(MainHit.normal, _movementVector);
                    //MovementVector = Vector3.Cross(tempVector, MainHit.normal) * HeadingX * (MainHit.normal.x >= 0 ? 1 : -1); // this is the movement vector transformed to a sliding vector
                }
            }
            return true;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);
        return false;
    }
    bool UpBoxeCast()
    {
        if (Physics2D.BoxCastNonAlloc(Position2D, _collider.size, 0f, Vector2.up * HeadingY, _mainHits, movementVectorScaled.y + boxOffset, groundCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y) * HeadingY) - Position2D.y));

            if (!ForwardBoxCast()) // so, you touched nothing forward
            {
                _transform.Translate(movementVectorScaled);
            }
            else
                _transform.Translate(Vector2.up * movementVectorScaled.y);

            return true;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);
        return false;
    }*/

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
        MovementVector = (target - (Vector2)_transform.position);// Vector2.Distance(target, (Vector2)_transform.position);
        //if (HeadingY > 0)
        IsGrounded = false;
    }

    public void OnTouchingStay(Vector2 target)
    {
        //MovementVector = (target - (Vector2)_transform.position).normalized;
    }

    #endregion


    #region TRIGGER REACTIONS

    void OnTriggerEnter2D(Collider2D co)
    {
        Debug.Log(co.name);
    }

    #endregion
}
