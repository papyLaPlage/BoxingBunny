using UnityEngine;
using System.Collections;

public class ActorPhysics : MonoBehaviour {

    #region SETUP

    private Transform _transform;
    private BoxCollider2D _collider;

    public Vector2 Position2D { get { return _transform.position; } }
    private Vector2 extentX;
    private Vector2 extentY;
    private Vector2 sizeX;
    private Vector2 sizeY;

    private float tempFloat;

    // Use this before initialization
    void Awake()
    {
        _transform = GetComponent<Transform>();
        _collider = GetComponent<BoxCollider2D>();

        sizeX = Vector2.right * _collider.size.x;
        sizeY = Vector2.up * _collider.size.y;
        extentX = sizeX * 0.5f;
        extentY = sizeY * 0.5f;

        HeadingX = HeadingY = 1;
        _isGrounded = true;
    }

    // Use this for initialization
    /*void Start()
    {
        
    }*/

    #endregion

    [Header("Physics Settings"), SerializeField]
    private LayerMask solidCastLayer;
    [SerializeField]
    private LayerMask groundCastLayer;
    [SerializeField, Range(0f, 0.5f)]
    private float boxOffset;

    public Vector2 MovementVector
    {
        get { return _movementVector; }
        set
        {
            _movementVector = value;
            _movementVector.y = Mathf.Max(_movementVector.y, -gravityForce);
            movementVectorScaled = value * Time.deltaTime;

            if (HeadingX > 0) { if (value.x < 0) HeadingX = -1; }
            else if (value.x >= 0) HeadingX = 1;
            if (HeadingY > 0) { if (value.y < 0) HeadingY = -1; }
            else if (value.y >= 0) HeadingY = 1;
        }
    }
    private Vector2 _movementVector; // used to know the next movement, unscaled
    private Vector2 movementVectorScaled;

    public float HeadingX
    {
        get { return _heading.x; }
        set { _heading.x = value; }
    }
    public float HeadingY
    {
        get { return _heading.y; }
        set { _heading.y = value; }
    }
    private Vector2 _heading; // general direction, used mainly for calculation 

    [SerializeField, Range(0, 100)]
    private float gravityForce;

    public void ApplyGravity()
    {
         MovementVector -= Vector2.up * gravityForce * Time.deltaTime;
    }

    #region CALLBACKS

    public delegate void PhysicsCallback();
    public PhysicsCallback OnGrounded;
    public PhysicsCallback OnAirborne;
    public PhysicsCallback OnSliding;

    public bool IsGrounded
    {
        get { return _isGrounded; }
        set
        {
            if (value != _isGrounded)
            {
                _isGrounded = value;
                if (value)
                {
                    ExecuteOnGrounded();
                }
                else
                {
                    ExecuteOnAirborne();
                }
            }
        }
    }
    private bool _isGrounded = true;
    private bool isSliding = false;

    void ExecuteOnGrounded()
    {
        if (OnGrounded != null)
        {
            OnGrounded();
        }
    }
    void ExecuteOnAirborne()
    {
        if(OnAirborne != null)
        {
            OnAirborne();
        }
    }

    #endregion

    #region CAST

    private RaycastHit2D MainHit { get { return _mainHits[0]; } }
    private RaycastHit2D[] _mainHits = new RaycastHit2D[1];
    private RaycastHit2D SecondHit { get { return _secondHits[0]; } }
    private RaycastHit2D[] _secondHits = new RaycastHit2D[1];

    private Vector2 mainCastOrigin;
    private Vector2 frontCastOrigin;
    private Vector2 backCastOrigin;

    public struct CastResult
    {
        public bool touched;
        public float normalAngle;
    }
    public CastResult castResult = new CastResult();

    public void CalculateCastOrigins()
    {
        mainCastOrigin = Position2D + extentX * HeadingX + extentY * HeadingY;
        frontCastOrigin = mainCastOrigin - sizeY * HeadingY;
        backCastOrigin = mainCastOrigin - sizeX * HeadingX;
    }

    public void DebugDraw()
    {
        Debug.DrawLine(Position2D, mainCastOrigin, Color.black);
        Debug.DrawLine(Position2D, frontCastOrigin, Color.black);
        Debug.DrawLine(Position2D, backCastOrigin, Color.black);
        Debug.DrawLine(mainCastOrigin, mainCastOrigin + Vector2.down * (boxOffset - movementVectorScaled.y), Color.red);
        Debug.DrawLine(backCastOrigin, backCastOrigin + Vector2.down * (boxOffset - movementVectorScaled.y), Color.red);
        Debug.DrawLine(mainCastOrigin, mainCastOrigin + Vector2.right * (boxOffset + movementVectorScaled.x) * HeadingX, Color.blue);
        Debug.DrawLine(frontCastOrigin, frontCastOrigin + Vector2.right * (boxOffset + movementVectorScaled.x) * HeadingX, Color.blue);
    }

    public void ForwardCast()
    {
        castResult.touched = true;
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.right * HeadingX, _mainHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(frontCastOrigin, Vector2.right * HeadingX, _secondHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
            {
                if (HeadingX > 0)
                { // facing right
                    if (SecondHit.point.x < MainHit.point.x) // know where you're blocked first
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
            }
            else // no hesitation
            {
                _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            }
            return;
        }
        else if (Physics2D.RaycastNonAlloc(frontCastOrigin, Vector2.right * HeadingX, _mainHits, Mathf.Abs(movementVectorScaled.x) + boxOffset, solidCastLayer) > 0)
        {
            _transform.Translate(Vector2.right * ((MainHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));

            return;
        }

        _transform.Translate(Vector2.right * movementVectorScaled.x);
        castResult.touched = false;
    }

    public void BackCast() // this one is more of an urgency cast, it should not be used every frame if possible
    {
        castResult.touched = true;
        if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.left * HeadingX, _secondHits, boxOffset, solidCastLayer) > 0)
        {
            _transform.Translate(Vector2.left * ((SecondHit.point.x - (boxOffset + extentX.x) * HeadingX) - Position2D.x));
            return;
        }

        //_transform.Translate(Vector2.right * movementVectorScaled.x);
        castResult.touched = false;
    }

    public void DownCast()
    {
        castResult.touched = true;
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.down, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.down, _secondHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
            {
                if (SecondHit.point.y > MainHit.point.y)
                { // know where you're blocked first
                    _transform.Translate(Vector2.up * ((SecondHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
                    castResult.normalAngle = Vector2.Angle(Vector2.up, SecondHit.normal);
                }
                else
                {
                    _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
                    castResult.normalAngle = Vector2.Angle(Vector2.up, MainHit.normal);
                }
            }
            else // no hesitation
            {
                _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
                castResult.normalAngle = Vector2.Angle(Vector2.up, MainHit.normal);

                /*
                    Vector3 tempVector = Vector3.Cross(MainHit.normal, _movementVector);
                    MovementVector = Vector3.Cross(tempVector, MainHit.normal) * (MainHit.normal.x >= 0 ? 1 : -1); // this is the movement vector transformed to a sliding vector
                    Debug.DrawRay(MainHit.point, Vector3.Cross(tempVector, MainHit.normal) * (MainHit.normal.x >= 0 ? 1 : -1), Color.red); // this is the movement vector transformed to a sliding vector
                */
            }
            return;
        }
        else if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.down, _mainHits, boxOffset - movementVectorScaled.y, groundCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y + (boxOffset + extentY.y)) - Position2D.y));
            castResult.normalAngle = Vector2.Angle(Vector2.up, MainHit.normal);
            return;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);
        castResult.touched = false;
    }

    public void UpCast()
    {
        castResult.touched = true;
        if (Physics2D.RaycastNonAlloc(mainCastOrigin, Vector2.up, _mainHits, boxOffset - movementVectorScaled.y, solidCastLayer) > 0)
        {
            if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.up, _secondHits, boxOffset - movementVectorScaled.y, solidCastLayer) > 0)
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
            return;
        }
        else if (Physics2D.RaycastNonAlloc(backCastOrigin, Vector2.up, _mainHits, boxOffset - movementVectorScaled.y, solidCastLayer) > 0)
        {
            _transform.Translate(Vector2.up * ((MainHit.point.y - (boxOffset + extentY.y)) - Position2D.y));
            return;
        }

        _transform.Translate(Vector2.up * movementVectorScaled.y);
        castResult.touched = false;
    }

    #endregion
}
