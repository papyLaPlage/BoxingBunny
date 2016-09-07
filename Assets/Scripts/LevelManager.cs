using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	private Transform player, mainCamera;

	// Use this for initialization
	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		mainCamera = Camera.main.transform;

		mainCamera.GetComponent<Camera2D>().SetTarget(player);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
