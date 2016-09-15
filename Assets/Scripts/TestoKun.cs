using UnityEngine;
using System.Collections;

public class TestoKun : MonoBehaviour {

    public static TestoKun Instance { get; private set; }

    public short debugCount;

    void Awake()
    {
        Instance = this;
    }

	// Update is called once per frame
	void Update () {
        debugCount++;

        if (debugCount > 10000)
            debugCount = 0;
    }
}
