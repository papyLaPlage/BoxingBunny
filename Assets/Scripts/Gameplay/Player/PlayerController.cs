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
        //StartCoroutine(AirborneUpdate());
    }

    void OnAirborne()
    {
        StartCoroutine(AirborneUpdate());
    }

    #endregion


    #region STATES/UPDATES

    // Update is called once per frame
    IEnumerator AirborneUpdate()
    {
        _physics.BackCast();

        while (!_physics.IsGrounded)
        {
            _physics.ApplyGravity();

            _physics.CalculateCastOrigins();

            _physics.DebugDraw();

            if (_physics.HeadingY < 0) // descending
            {
                _physics.DownCast();
                if(_physics.castResult.touched)
                {
                    _physics.IsGrounded = true;
                    _physics.MovementVector = Vector2.zero;
                    //Debug.Break();
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

    public void OnTouchingStart(Vector2 target)
    {
        _physics.MovementVector = (target - (Vector2)_transform.position);// Vector2.Distance(target, (Vector2)_transform.position);
        //if (HeadingY > 0)
        _physics.IsGrounded = false;
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
