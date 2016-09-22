using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerInputs : MonoBehaviour
{

	#region SETUP

	private Transform _transform;
	//private PlayerInputs _inputs;

	public PlayerControllerFus Player
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
	private PlayerControllerFus _player;

	void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	// Use this for initialization
	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerFus>();
		StartInputSampling();
	}

	void ActivateControls(bool state)
	{
		enabled = state;
		foreach(Button button in GetComponentsInChildren<Button>())
			button.interactable = state;
	}

	#endregion


	#region BUTTON INPUTS

	[SerializeField]
	private GameObject rightPunchButton;
	[SerializeField]
	private GameObject leftPunchButton;

	public void OnPunchClicked(bool rightPunch)
	{
		_player.OnPunching(rightPunch);
	}

	#endregion


	#region GAMEPLAY INPUTS

	private bool gameplayOn;
	private Vector2 clickPosition;

	// Update is called once per frame
	IEnumerator InputSampling()
	{
		while(gameplayOn)
		{
			if(Input.GetMouseButton(0))
			{
				if(EventSystem.current.IsPointerOverGameObject())
				{
					if(Input.GetMouseButtonDown(0))
					{
						if(EventSystem.current.currentSelectedGameObject == rightPunchButton)
						{
							_player.OnPunching(true);
						}
						else if(EventSystem.current.currentSelectedGameObject == leftPunchButton)
						{
							_player.OnPunching(false);
						}
						else
						{

						}
					}
				}
				else
				{
					clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition - (Vector3.forward * Camera.main.transform.position.z)); //getting target position for player
					if(Input.GetMouseButtonDown(0))
					{ // first frame touching
						_player.OnTouchingStart(clickPosition);
						StartCoroutine(SwipeDetection());
					}
					else
						_player.OnTouchingStay(clickPosition);
				}
			}
			else if(swipeTimer > 0f)
			{
				//Debug.Log(swipeDistance);
				if(swipeDistance >= swipeMinHeight)
				{
					gameplayOn = false; //PAUSE
					Time.timeScale = 0f;
					SetPauseActive(true);
				}
				swipeTimer = 0f;
			}

			yield return null;
		}
	}

	public void StartInputSampling()
	{
		if(gameplayOn)
			return;
		gameplayOn = true;
		Time.timeScale = 1f;
		StartCoroutine(InputSampling());
	}

	#endregion


	#region PAUSE

	[Header("Pause Part"), SerializeField]
	private GameObject pausePanel;
	[SerializeField]
	private float swipeMinHeight;
	[SerializeField]
	private float swipeMaxDuration;

	private float swipeTimer;
	private float swipeDistance;

	IEnumerator SwipeDetection()
	{
		swipeTimer = swipeMaxDuration;
		swipeDistance = 0f;

		while(swipeTimer > 0f)
		{
			swipeDistance += Input.GetAxis("Mouse Y");
			swipeTimer -= Time.deltaTime;
			yield return null;
		}
	}

	public void SetPauseActive(bool state)
	{
		pausePanel.SetActive(state);
	}

	#endregion
}
