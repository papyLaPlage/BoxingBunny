using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TestLevelManager : MonoBehaviour {

    //public static TestLevelManager Instance;
    public string[] levels;

    /*void Awake()
    {
        Instance = this;
    }*/

    public void GoToLevel(int levelID)
    {
        SceneManager.LoadScene(levels[levelID]);
    }
    public void GoToLevel(string levelID)
    {
        SceneManager.LoadScene(levelID);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
