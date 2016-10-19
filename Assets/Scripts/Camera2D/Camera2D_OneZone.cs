using UnityEngine;

//limite le deplacement de la cible dans un carré
public class Camera2D_OneZone : Camera2DLogic
{
	[SerializeField,Range(0,20)]
	private float width = 2;
	public float Width
	{
		get
		{
			return width;
		}

		set
		{
			width = value;
			widthD2 = value / 2;
		}
	}

	[SerializeField, Range(0, 20)]
	private float height = 2;
	private float Height
	{
		get
		{
			return height;
		}

		set
		{
			height = value;
			heightD2 = value / 2;
		}
	}

	private float widthD2 = 1;
	private float heightD2 = 1;

	void Start()
	{
		widthD2 = Width / 2;
		heightD2 = Height / 2;
	}

	public override void UpdatePoint(ref Point2D point2D)
	{
#if UNITY_EDITOR
		if(point2D == null)
			return;

		_transform = transform;
		_point2D = point2D;
#endif
		if(plan == EnumCameraPlan.X || plan == EnumCameraPlan.XY)
		{
			point2D.position.x = Calcul(_transform.position.x, point2D.position.x, widthD2);
		}
		if(plan == EnumCameraPlan.Y || plan == EnumCameraPlan.XY)
		{
			point2D.position.y = Calcul(_transform.position.y, point2D.position.y, heightD2);
		}
	}

	private float Calcul(float transPosition, float point, float distanceMax)
	{
		float distance = transPosition - point;

		if(Mathf.Abs(distance) > distanceMax)
		{
			float correct = distance - distanceMax * Mathf.Sign(distance);

			return point + correct;
		}

		return point;
	}

#if UNITY_EDITOR
	private Point2D _point2D;

	void OnDrawGizmos()
	{
		if(_point2D == null)
		{
			_point2D = new Point2D(Camera.main.transform.position);
		}

		_point2D.decalage = decalage;

		widthD2 = Width / 2;
		heightD2 = height / 2;

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(new Vector3(_point2D.position.x, _point2D.position.y), 0.1f);

		if(plan == EnumCameraPlan.X || plan == EnumCameraPlan.XY)
		{
			Debug.DrawLine(
				new Vector2(_point2D.position.x + widthD2, _point2D.position.y + heightD2),
				new Vector2(_point2D.position.x + widthD2, _point2D.position.y - heightD2),
				Color.blue
			);
			Debug.DrawLine(
				new Vector2(_point2D.position.x - widthD2, _point2D.position.y - heightD2),
				new Vector2(_point2D.position.x - widthD2, _point2D.position.y + heightD2),
				Color.blue
			);

		}
		if(plan == EnumCameraPlan.Y || plan == EnumCameraPlan.XY)
		{
			Debug.DrawLine(
				new Vector2(_point2D.position.x + widthD2, _point2D.position.y + heightD2),
				new Vector2(_point2D.position.x - widthD2, _point2D.position.y + heightD2),
				Color.blue
			);
			Debug.DrawLine(

				new Vector2(_point2D.position.x - widthD2, _point2D.position.y - heightD2),
				new Vector2(_point2D.position.x + widthD2, _point2D.position.y - heightD2),
				Color.blue
			);
		}
	}
#endif
}
