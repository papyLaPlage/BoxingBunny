using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerInputs_pedro : MonoBehaviour
{

	#region SETUP

	//private Transform _transform;

	public PlayerController_pedro Player
	{
		get
		{
			return _player;
		}
		set
		{
			if(value == null)
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
	private PlayerController_pedro _player;

	void Awake()
	{
		//_transform = GetComponent<Transform>();
	}

	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_pedro>();
	}

	void ActivateControls(bool state)
	{
		enabled = state;
		foreach(Button button in GetComponentsInChildren<Button>())
			button.interactable = state;
	}

	#endregion


	#region BUTTON INPUTS

	public void OnPunchClicked(bool rightPunch)
	{
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_pedro>();
		_player.OnPunching(rightPunch);
	}

	#endregion


	#region ONSCREEN INPUTS

	private Vector2 clickPosition;

	void Update()
	{
		if(Input.GetMouseButton(0))
		{

			clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition - (Vector3.forward * Camera.main.transform.position.z)); //getting target position for player
			if(Input.GetMouseButtonDown(0))
			{ // first frame touching
				_player.OnTouchingStart(clickPosition);
			}
			else
			{
				_player.OnTouchingStay(clickPosition);
			}
		}
	}

	#endregion
}
