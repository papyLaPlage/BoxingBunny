using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region SETUP

    private Transform _transform;
    private BoxCollider2D _collider;
    private ActorPhysics _physics;

    [SerializeField]
    private Transform _skin;
    [SerializeField]
    private Animator _anims;

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
        Facing = 1;
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
        _anims.Play("Idle");
    }

    void OnAirborne()
    {
        
        StartCoroutine(AirborneUpdate());
        _anims.Play("Airborne");
    }

    void OnSliding()
    {
        StartCoroutine(SlidingUpdate());
        Facing = (int)_physics.HeadingX;
        _anims.Play("Sliding");
    }

    #endregion


    #region STATES/UPDATES

    // Update is called once per frame
    IEnumerator AirborneUpdate()
    {
        _physics.BackCast();
        Facing = (int)_physics.HeadingX;

        while (!_physics.IsGrounded && !_physics.IsSliding)
        {
            _physics.ApplyGravity();

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
                        //Debug.DrawRay(Position2D, _physics.MovementVector, Color.red);
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

            //_physics.DebugDraw();

            yield return null;
        }
    }


    private float slidingAngle; // use this to know if we should recalculate the MovementVector

    IEnumerator SlidingUpdate()
    {
        while (_physics.IsSliding)
        {
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

            //_physics.DebugDraw();

            yield return null;
        }
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
        else if(punchTimer <= 0f)
        {
            OnPunching(target.x >= Position2D.x ? true : false);
        }
    }

    public void OnTouchingStay(Vector2 target) // for air control
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


    #region PUNCHING! + FACING

    [Header("Punch Properties"), SerializeField]
    private LayerMask punchLayer;
    [SerializeField]
    private float punchRadius;
    [SerializeField]
    private float punchDistance;
    [SerializeField]
    private float punchStartup;
    [SerializeField]
    private float punchRecovery;
    private float punchTimer;
    private RaycastHit2D[] punchHits;

    public void OnPunching(bool rightPunch) // from the buttons
    {
        if (punchTimer <= 0f && !_physics.IsSliding)
        {
            Facing = rightPunch ? 1 : -1;
            StartCoroutine(Punch());
        }
        /*else {
            //unavaible
            // punchTimer -= Time.deltaTime; //accelerate the recovery a bit?
        }*/
    }

    IEnumerator Punch()
    {
        punchTimer = punchStartup + punchRecovery;
        _anims.Play("PunchCooldown"); //hack-ish
        _anims.Play("PunchR");

        while (punchTimer > 0f) // recovery
        {
            while (punchTimer > punchRecovery) // startup
            {
                punchTimer -= Time.deltaTime;
                yield return null;
            }

            if (Physics2D.CircleCastNonAlloc(Position2D, punchRadius, Vector2.right * Facing, punchHits, punchDistance, punchLayer) > 0) // maybe have active frames?
            {
                // IPunchable punchable punchHit.collider.GetComponent<IPunchable>();
            }

            while (punchTimer > 0f) // recovery
            {
                punchTimer -= Time.deltaTime;
                yield return null;
            }
        }
    }

    private int Facing
    {
        get { return _facing; }
        set
        {
            if (value != _facing)
            {
                _facing = value;
                _skin.localScale = new Vector2(value, 1);
            }
        }
    }
    private int _facing;

    #endregion
}
