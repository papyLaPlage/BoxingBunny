﻿using UnityEngine;
using System.Collections;

public class Plateforme : MonoBehaviour
{
	private Transform _transforme;
	private Vector2 originePosition;

	//pour calculer le déplacement effectuer à donner aux objets sur la plateforme
	private Vector2 lastPosition;

	private ArrayList objetsToSupport = new ArrayList();

	private uint nextPoint, pointsLength;
	[SerializeField]
	private bool inverseDirection = false;

	private enum Comportement
	{
		Loop,
		GoAndReturn,
		GoAndTeleport,
		Teleport
	}

	[SerializeField]
	private Comportement comportent;
	[SerializeField]
	private uint startPoint = 0;
	[SerializeField]
	private float stopTime = 0;
	private float stopTimer = 0;
	[SerializeField]
	private float decalTime = 0;
	[SerializeField]
	private float speed = 1;
	[SerializeField]
	private Vector2[] points = new Vector2[2];

#if UNITY_EDITOR
	private bool isPlay = false;
#endif


	void Awake()
	{
		if(points.Length < 2)
		{
			Destroy(this);
		}

		switch(comportent)
		{
			case Comportement.Teleport:
				Destroy(GetComponent<EdgeCollider2D>());
				break;
		}
	}

	void Start()
	{
		_transforme = transform;
		originePosition = _transforme.position;
		pointsLength = (uint)points.Length;

		//save des position exacte
		for(int i = 0; i < pointsLength; i++)
		{
			points[i] = points[i] + originePosition;
		}

		startPoint = (uint)Mathf.Min(startPoint, points.Length - 1);
		nextPoint = startPoint;
		nextPoint = GetNextObjectif();

		_transforme.position = points[startPoint];

		stopTimer = decalTime;

#if UNITY_EDITOR
		isPlay = true;
#endif
	}

	void Update()
	{
		if(stopTimer <= 0)
		{
			lastPosition = _transforme.position;

			switch(comportent)
			{
				default:
				case Comportement.Loop:
					_transforme.position = Vector2.MoveTowards(_transforme.position, points[nextPoint], Time.deltaTime * speed);

					if((Vector2)_transforme.position == points[nextPoint])
					{
						nextPoint = GetNextObjectif();
						stopTimer = stopTime;
					}

					MoveObjets();
					break;

				case Comportement.Teleport:
					_transforme.position = points[nextPoint];
					nextPoint = GetNextObjectif();
					stopTimer = stopTime;
					break;

				case Comportement.GoAndTeleport:

					if(nextPoint == 0 || nextPoint == pointsLength - 1 && inverseDirection)
					{
						_transforme.position = points[nextPoint];
					}
					else
					{
						_transforme.position = Vector2.MoveTowards(_transforme.position, points[nextPoint], Time.deltaTime * speed);
						MoveObjets();
					}


					if((Vector2)_transforme.position == points[nextPoint])
					{
						nextPoint = GetNextObjectif();
						stopTimer = stopTime;
					}
					break;
			}
		}
		else
			stopTimer -= Time.deltaTime;
	}

	void MoveObjets()
	{
		Vector3 move = _transforme.position - (Vector3)lastPosition;
		foreach(Transform objet in objetsToSupport)
		{
			objet.position += move;
		}
	}

	uint GetNextObjectif()
	{
		if(inverseDirection)
		{
			if(nextPoint == 0)
				switch(comportent)
				{
					default:
					case Comportement.Loop:
					case Comportement.GoAndTeleport:
					case Comportement.Teleport:
						return pointsLength - 1;

					case Comportement.GoAndReturn:
						inverseDirection = !inverseDirection;
						return nextPoint + 1;
				}
			else
				return nextPoint - 1;
		}
		else
		{
			if(nextPoint == pointsLength - 1)
				switch(comportent)
				{
					default:
					case Comportement.Loop:
					case Comportement.GoAndTeleport:
					case Comportement.Teleport:
						return 0;

					case Comportement.GoAndReturn:
						inverseDirection = !inverseDirection;
						return nextPoint - 1;
				}
			else
				return nextPoint + 1;
		}

	}


	void OnTriggerEnter2D(Collider2D co)
	{
		if(objetsToSupport.IndexOf(co.transform) == -1)
		{
			objetsToSupport.Add(co.transform);
		}
	}

	void OnTriggerExit2D(Collider2D co)
	{
		objetsToSupport.Remove(co.transform);
	}

#if UNITY_EDITOR

	public float drawSize = 0.5f;
	void OnDrawGizmos()
	{
		if(points.Length > 1)
		{
			int L = points.Length - 1;
			Vector2 transPosition;
			if(isPlay)
				transPosition = Vector2.zero;
			else
				transPosition = transform.position;

			if(inverseDirection)
			{
				Gizmos.color = Color.blue;
			}
			else
			{
				Gizmos.color = Color.cyan;
			}

			for(int i = 0; i < L; i++)
			{
				Gizmos.DrawWireSphere(transPosition + points[i], drawSize);
				Gizmos.DrawLine(transPosition + points[i], transPosition + points[i + 1]);
			}

			Gizmos.DrawWireSphere(transPosition + points[L], drawSize);

			if(comportent != Comportement.GoAndReturn)
			{
				Gizmos.DrawLine(transPosition + points[0], transPosition + points[L]);
			}
		}
	}
#endif

}
