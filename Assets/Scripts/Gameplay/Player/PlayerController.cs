using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    #region SETUP

    private Transform _transform;
    private BoxCollider2D _collider;

    public Vector2 Position2D
    {
        get
        {
            return _transform.position;
        }
    }

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
    }

    private Vector2 tempVector;
    private float tempFloat;
    private int tempInt;

    #endregion


    #region UPDATE + PHYSICS

    [SerializeField]
    private LayerMask groundCastLayer;
    private RaycastHit2D hit;

    private Vector2 movementAnchor; //debug draw purpose
    //private Vector2 previousPosition;

    private Vector2 movementVector;
    private int horizontalFactor = 1;
    private int verticalFactor = 1;
    private Vector2 extentX; 
    private Vector2 extentY;
    private Vector2 sizeX;
    private Vector2 sizeY;

    private Vector2 mainCastTarget; // main corner position. normally, only this one should need to return a RaycastHit2D
    private Vector2 frontCastTarget; // testing wall
    private Vector2 backCastTarget; // testing ground or ceiling
                                                                               

    // Update is called once per frame
    void Update()
    {

    }

    void CalculateFacing()
    {
        horizontalFactor =  movementVector.x >= 0f ? 1 : -1;
        verticalFactor =    movementVector.y > 0f  ? 1 : -1;
    }

    void PlaceCastTarget()
    {
        mainCastTarget = Position2D + movementVector*Time.deltaTime + extentX*horizontalFactor + extentY*verticalFactor;
        frontCastTarget = mainCastTarget + sizeY*verticalFactor;
        backCastTarget = mainCastTarget + sizeX*horizontalFactor;
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
        movementVector = (target - (Vector2)_transform.position).normalized;
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
