using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		IFollowA = !IFollowA;
		targets = targetTransforme.GetComponents<Camera2DLogic>();

		if (targets.Length == 0)
		{
			Debug.Log("Aucun Camera2DLogic dans la cible");
		}
		else if (IFollowA)
		{
			pointA.decalage = Vector2.zero;
			foreach (Camera2DLogic i in targets)
			{
				pointA.decalage += i.decalage;
			}
		}
		else
		{
			pointB.decalage = Vector2.zero;
			foreach (Camera2DLogic i in targets)
			{
				pointB.decalage += i.decalage;
			}
		}
	}

	private void Start()
	{
		pointA = gameObject.AddComponent<Point2D>();
		pointB = gameObject.AddComponent<Point2D>();
	}

	private void LateUpdate()
	{

		if (IFollowA)
		{
			foreach (Camera2DLogic i in targets)
			{
				i.UpdatePoint(ref pointA);

			}
			point = pointA;
		}
		else
		{
			foreach (Camera2DLogic i in targets)
			{
				i.UpdatePoint(ref pointB);

			}
			point = pointB;
		}

		Vector3 nPosition = point.CameraPosition;
		nPosition.z = transform.position.z;
		transform.position = nPosition;
	}
}
