using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region SETUP

    private Transform _transform;
    private BoxCollider2D _collider;
    private ActorPhysics _physics;

    public Vector2 Position2D { get { return _transform.position; } }

    // Use this before initialization
    void Awake()
    {
        _transform = GetComponent<Transform>();
        _collider = GetComponent<BoxCollider2D>();
        _physics = GetComponent<ActorPhysics>();
    }

    // Use this for initialization
    void Start()
    {
        _physics.OnGrounded += OnGrounded;
        _physics.OnAirborne += OnAirborne;
        _physics.OnSliding += OnSliding;

        _physics.IsSliding = false;
        _physics.IsGrounded = false;
    }

    private Vector2 tempVector;
    private float tempFloat;
    private int tempInt;
    private int tempShort;
    private bool tempBool;

    #endregion


    #region PHYSICS CALLBACKS

    void OnGrounded()
    {
        //StartCoroutine(GroundedUpdate());
    }

    void OnAirborne()
    {
        StartCoroutine(AirborneUpdate());
    }

    void OnSliding()
    {
        StartCoroutine(SlidingUpdate());
    }

    #endregion


    #region STATES/UPDATES

    // Update is called once per frame
    IEnumerator AirborneUpdate()
    {
        _physics.BackCast();

        while (!_physics.IsGrounded && !_physics.IsSliding)
        {
            _physics.ApplyGravity();

            _physics.CalculateCastOrigins();

            //_physics.DebugDraw();

            if (_physics.HeadingY < 0) // descending
            {
                _physics.DownCast();
                if (_physics.castResult.touched)
                {
                    if (_physics.castResult.normalAngle <= walkableAngle)
                    {
                        _physics.MovementVector = Vector2.zero;
                        _physics.IsGrounded = true;
                        //Debug.Break();
                    }
                    else if (_physics.castResult.normalAngle < 90)
                    {
                        slidingAngle = _physics.castResult.normalAngle;
                        _physics.MovementVector = ((Vector2)(Quaternion.Euler(0, 0, slidingAngle * -_physics.castResult.heading) * (Vector2.right * _physics.castResult.heading))  + Vector2.down)* slidingSpeed;
                        _physics.IsSliding = true;
                        Debug.DrawRay(Position2D, _physics.MovementVector, Color.red);
                        //Debug.DrawRay(Position2D, Vector3.Cross(_physics.MovementVector, _physics.castResult.normal) * _physics.castResult.heading, Color.red); // this is the movement vector transformed to a sliding vector
                        //_physics.MovementVector = Vector2.zero; //obtain the sliding vector
                        //Debug.Break();
                    }
                }
            }
            else // ascending 
            {
                _physics.UpCast();
                if (_physics.castResult.touched) {
                    tempVector = _physics.MovementVector;
                    tempVector.y = -tempVector.y;
                    _physics.MovementVector = tempVector;
                }
            }

            _physics.ForwardCast();

            yield return null;
        }
    }


    private float slidingAngle; // use this to know if we should recalculate the MovementVector

    IEnumerator SlidingUpdate()
    {
        while (_physics.IsSliding)
        {
            _physics.CalculateCastOrigins();

            _physics.DebugDraw();
            //Debug.Break();

            _physics.ForwardCast();

            _physics.DownCast();
            if (_physics.castResult.touched)
            {
                if (_physics.castResult.normalAngle <= walkableAngle)
                {
                    _physics.IsGrounded = true;
                    _physics.MovementVector = Vector2.zero;
                    //Debug.Break();
                }
                else if (_physics.castResult.normalAngle < 90 && _physics.castResult.normalAngle != slidingAngle)
                {
                    slidingAngle = _physics.castResult.normalAngle;
                    _physics.MovementVector = ((Vector2)(Quaternion.Euler(0, 0, slidingAngle * -_physics.castResult.heading) * (Vector2.right * _physics.castResult.heading))  + Vector2.down)* slidingSpeed;
                }
            }
            else
            {
                _physics.IsSliding = false;
            }

            yield return null;
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


    #region POINTING

    [Header("Movement Capping"), SerializeField]
    private float maxDistance;
    [SerializeField]
    private float minHop;
    [Header("Movement Properties"), SerializeField]
    private float jumpImpulsion;
    [SerializeField]
    private float speed;
    [SerializeField, Range(1, 89)]
    private float walkableAngle;
    [SerializeField]
    private float slidingSpeed;

    public void OnTouchingStart(Vector2 target)
    {
        if (_physics.IsGrounded)
        {
            tempVector = Vector2.ClampMagnitude((target - (Vector2)_transform.position), maxDistance);
            tempVector.y = Mathf.Max(minHop, tempVector.y * jumpImpulsion + minHop);
            tempVector.x *= speed;
            _physics.MovementVector = tempVector;
            //Debug.Log((target - (Vector2)_transform.position)+" "+tempVector);
            _physics.IsGrounded = false;
        }
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
