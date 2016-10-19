using UnityEngine;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{

	PlayerController player;
	public Transform powerBar;
	private float powerBarOriginWidth = 0;

	// Use this for initialization
	void Start()
	{
		player = FindObjectOfType<PlayerController>();
		if(player == null)
		{
			Debug.Log("No player find");
			DestroyImmediate(this);
		}
		powerBarOriginWidth = powerBar.transform.localScale.x;
	}

	Vector3 tempVector;

	// Update is called once per frame
	void Update()
	{
		tempVector = powerBar.localScale;
		tempVector.x = powerBarOriginWidth * (player.PowerQuantity / player.powerQuantityMax);
		powerBar.localScale = tempVector;
	}
}
