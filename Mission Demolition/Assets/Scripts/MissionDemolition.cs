using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameMode
{
    idle,
    playing,
    levelEnd
}
public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S; // a private Singleton

    [Header("Inscribed")]
    public Text uitLevel; // The UIText_Level Text
    public Text uitShots;
    public Vector3 castlePos;
    public GameObject[] castles;

    [Header("Dynamic")]
    public int level;
    public int levelMax;
    public int shotsTaken;
    public GameObject castle;
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot"; // FollowCam mode 
    // Start is called before the first frame update
    void Start()
    {
        S = this; // Define the Singleton
        level = 0;
        shotsTaken = 0;
        levelMax = castles.Length;
        StartLevel();
    }

    void StartLevel()
    {
        // Get rid of the old castle if one exists 
        if (castle != null)
        {
            Destroy(castle);
        }

        // Delete rigid

        foreach (Rigidbody rb in FindObjectsOfType<Rigidbody>())
        {
        // Skip the slingshot or camera if they exist
        if (rb.gameObject.name.Contains("Slingshot") || rb.gameObject.name.Contains("FollowCam"))
            continue;
        Destroy(rb.gameObject);
        }
        // Destroy old projectiles if they exist 
        Projectile.DESTROY_PROJECTILES();

        // Instantiate the new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        // Reset the goal
        Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;

        // Zoom out to show both 
        FollowCam.SWITCH_VIEW(FollowCam.eView.both); //  a 
    }

    void UpdateGUI()
    {
        //Show the data in  the GUITexts
        uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        uitShots.text = "Shots Taken: " + shotsTaken;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateGUI();

        //Check for level end 
        if ((mode == GameMode.playing) && Goal.goalMet)
        {
            // Change mode to stop checking for level end 
            mode = GameMode.levelEnd;

            // Zoom out to show both 
            FollowCam.SWITCH_VIEW(FollowCam.eView.both); //b 
        if (level >= levelMax - 1)
            {
                // (optional) save any stats you want to show
                PlayerPrefs.SetInt("LastShots", shotsTaken);
                int bestScore = PlayerPrefs.GetInt("BestScore", int.MaxValue);
                if (shotsTaken < bestScore)
                {
                    PlayerPrefs.SetInt("BestScore", shotsTaken);
                }
                PlayerPrefs.Save();

                SceneManager.LoadScene("GameOver");   // <-- make sure this scene is in Build Settings
            }
            else
            {
                // not the last level: proceed as before
                Invoke("NextLevel", 2f);
            }
        }
    }

    void NextLevel()
    {
        level++;
        if (level == levelMax)
        {
            level = 0;
            shotsTaken = 0;
        }
        StartLevel();
    }

    static public void SHOT_FIRED()
    {
        S.shotsTaken++;
    }
    
    static public GameObject GET_CASTLE()
    {
        return S.castle;
    }
}
