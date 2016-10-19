using UnityEngine;

//Script unique pour la camera
public class Camera2D : MonoBehaviour
{
	[HideInInspector]
	public bool IFollowA = true;
	[HideInInspector]
	public Point2D pointA;
	[HideInInspector]
	public Point2D pointB;

	private Point2D point;
	private Camera2DLogic[] targets;

	public void SetTarget(Transform targetTransforme)
	{
		targets = targetTransforme.GetComponents<Camera2DLogic>();

		if(targets.Length == 0)
		{
			Debug.Log("Aucun Camera2DLogic dans la cible");
		}
		else
		{
			IFollowA = !IFollowA;

			if(IFollowA)
			{
				point = pointA;
			}
			else
			{
				point = pointB;
			}

			point.position = targetTransforme.position;
			point.decalage = Vector2.zero;

			foreach(Camera2DLogic i in targets)
			{
				point.decalage += i.decalage;
			}
		}
	}

	private void Awake()
	{
#if UNITY_EDITOR
		isPlay = true;
		player = null;
#endif
		pointA = new Point2D();
		pointB = new Point2D();
	}

	private void LateUpdate()
	{
#if UNITY_EDITOR
		if(point == null)
			EditorSetTarget();
#endif

		if(IFollowA)
		{
			foreach(Camera2DLogic i in targets)
			{
				i.UpdatePoint(ref pointA);

			}
			point = pointA;
		}
		else
		{
			foreach(Camera2DLogic i in targets)
			{
				i.UpdatePoint(ref pointB);

			}
			point = pointB;
		}

		Vector3 nPosition = point.CameraPosition;
		nPosition.z = transform.position.z;
		transform.position = nPosition;
	}

#if UNITY_EDITOR
	private bool isPlay = false;
	private Transform player;
	private Vector3 playerPosition;

	void EditorSetTarget()
	{
		pointA = new Point2D();
		pointB = new Point2D();

		player = GameObject.FindGameObjectWithTag("Player").transform;

		if(player != null)
		{
			playerPosition = player.position;
			SetTarget(player);
		}
	}

	void OnDrawGizmos()
	{
		if(!isPlay)
		{
			if(player == null || !playerPosition.Equals(player.position))
			{
				EditorSetTarget();
			}
			
			LateUpdate();
		}
	}
#endif
}
