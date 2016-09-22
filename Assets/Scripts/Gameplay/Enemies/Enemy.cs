using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
	
	#region SETUP

	private Transform _transform;
	private ActorPhysics _physics;

	void Awake()
	{
		_transform = GetComponent<Transform>();
		_physics = GetComponent<ActorPhysics>();
	}

	void Start () {
	
	}

	#endregion

	// Update is called once per frame
	void Update () {
	
	}
}
