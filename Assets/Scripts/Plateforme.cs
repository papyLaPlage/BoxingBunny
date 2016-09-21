using UnityEngine;
using System.Collections;

public class Plateforme : MonoBehaviour
{
	private Transform _transforme;
	private Vector2 originePosition;

	//pour calculer le déplacement effectuer à donner aux objets sur la plateforme
	private Vector2 lastPosition;

	private ArrayList objetsToSupport = new ArrayList();

	private uint lastPoint, nextPoint, pointsLength;
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
		GetNextObjectif();

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

					moveTimer += Time.deltaTime;
					_transforme.position = Vector2.Lerp(points[lastPoint], points[nextPoint], moveTimer / moveTime);

					if(moveTimer >= moveTime)
					{
						GetNextObjectif();
					}

					MoveObjets();
					break;

				case Comportement.Teleport:
					_transforme.position = points[nextPoint];
					GetNextObjectif();
					break;

				case Comportement.GoAndTeleport:
					if(nextPoint == 0 || nextPoint == pointsLength - 1 && inverseDirection)
					{
						_transforme.position = points[nextPoint];
					}
					else
					{
						moveTimer += Time.deltaTime;
						_transforme.position = Vector2.Lerp(points[lastPoint], points[nextPoint], moveTimer / moveTime);
						MoveObjets();

						if(moveTimer >= moveTime)
						{
							GetNextObjectif();
						}
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

	float moveTimer, moveTime;

	void GetNextObjectif()
	{
		lastPoint = nextPoint;

		if(inverseDirection)
		{
			if(nextPoint == 0)
				switch(comportent)
				{
					default:
					case Comportement.Loop:
					case Comportement.GoAndTeleport:
					case Comportement.Teleport:
						nextPoint--;
						break;
					case Comportement.GoAndReturn:
						inverseDirection = !inverseDirection;
						nextPoint++;
						break;
				}
			else
				nextPoint--;
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
						nextPoint = 0;
						break;
					case Comportement.GoAndReturn:
						inverseDirection = !inverseDirection;
						nextPoint--;
						break;
				}
			else
				nextPoint++;
		}

		moveTime = Vector2.Distance(points[nextPoint], points[lastPoint]) / speed;
		moveTimer = 0;
		stopTimer = stopTime;
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
