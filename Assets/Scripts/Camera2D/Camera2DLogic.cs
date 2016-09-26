using UnityEngine;

//Script à mettre sur les objets pour un suivi de la camera
public class Camera2DLogic : MonoBehaviour
{
    static int IDs = -1;

    [SerializeField]
    protected int iD;
	[SerializeField]
	protected EnumCameraPlan plan = EnumCameraPlan.XY;
	public Vector2 decalage = Vector2.zero;

    protected Transform _transform;

    public int ID
     {
         get
         {
             return iD;
         }
     }

    virtual protected void Awake()
    {
        _transform = transform;
        iD = ++IDs;
    }

    virtual public void UpdatePoint(ref Point2D point2D)
	{
#if UNITY_EDITOR
		_transform = transform;
#endif
		switch(plan)
        {
            case EnumCameraPlan.XY:
                point2D.position = _transform.position;
                break;
            case EnumCameraPlan.X:
                point2D.position.x = _transform.position.x;
                break;
            case EnumCameraPlan.Y:
                point2D.position.y = _transform.position.y;
                break;
        }

		point2D.decalage = decalage;

	}
}
