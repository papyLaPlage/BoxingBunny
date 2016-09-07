using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerInputs : MonoBehaviour
{

	#region SETUP

	private Transform _transform;
	//private PlayerInputs _inputs;

	public PlayerController Player
	{
		get
		{
			return _player;
		}
		set
		{
			if (value == null)
			{
				ActivateControls(false);
			}
			else
			{
				ActivateControls(true);
				_player = value;
			}
		}
	}
	private PlayerController _player;

	void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	// Use this for initialization
	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}

	void ActivateControls(bool state)
	{
		enabled = state;
		foreach (Button button in GetComponentsInChildren<Button>())
			button.interactable = state;
	}

	#endregion


	#region BUTTON INPUTS

	public void OnPunchClicked(bool rightPunch)
	{
		//_player.OnPunching(rightPunch);
	}

	#endregion


	#region ONSCREEN INPUTS

	private Vector2 clickPosition;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition - (Vector3.forward * Camera.main.transform.position.z)); //getting target position for player
			if (Input.GetMouseButtonDown(0))
			{ // first frame touching
				_player.OnTouchingStart(clickPosition);
			}
			else
				_player.OnTouchingStay(clickPosition);
		}
	}

	#endregion
}
