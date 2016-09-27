using UnityEngine;

//Script unique pour la camera
public class Camera2D : MonoBehaviour
{
	public bool IFollowA = true;
	public Point2D pointA;
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

			point.position = targetTransforme.transform.position;
			point.decalage = Vector2.zero;

			foreach(Camera2DLogic i in targets)
			{
				point.decalage += i.decalage;
			}
		}
	}

	private void Awake()
	{
		pointA = new Point2D();
		pointB = new Point2D();

#if UNITY_EDITOR
		isPlay = true;
		player = null;
#endif
	}

	private void LateUpdate()
	{
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

		/*foreach(Camera2DLogic i in targets)
		{
			i.UpdatePoint(ref point);
		}*/

		Vector3 nPosition = point.CameraPosition;
		nPosition.z = transform.position.z;
		transform.position = nPosition;
	}

#if UNITY_EDITOR
	public bool isPlay = false;
	Transform player;
	void OnDrawGizmos()
	{
		if(!isPlay)
		{
			if(player == null)
			{
				pointB = new Point2D();
				pointA = new Point2D();

				//point = new Point2D();
				player = GameObject.FindGameObjectWithTag("Player").transform;
				SetTarget(player);
			}
			else
			{
				LateUpdate();
				//transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
			}
		}
	}
#endif
}
