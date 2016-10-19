using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{

	[SerializeField]
	private float pvMax = 1;
	private float pv = 1;
	private bool alive = true;

	[SerializeField]
	private Animator _anims;

	// Use this for initialization
	void Start()
	{
		pv = pvMax;
	}

	public void UpdateLife(int _pv)
	{
		pv += _pv;
		pv = Mathf.Clamp(pv, 0, pvMax);

		if(pv == 0)
		{
			alive = false;

			if(_anims != null)
			{
				_anims.Play("Death");
			}
		}
	}


}
