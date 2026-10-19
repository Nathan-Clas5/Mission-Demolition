using System.Collections;
using UnityEngine;
using TMPro;

public enum GameMode
{
    idle,
    playing,
    levelEnd,
    gameOver,
    victory
}

public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S;

    [Header("Inscribed")]
    public TextMeshProUGUI uitLevel;
    public TextMeshProUGUI uitShots;
    public Vector3 castlePos;
    public GameObject[] castles;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public int shotsAllowed = 3;
    public float failureWaitTime = 12f;

    [Header("Dynamic")]
    public int level;
    public int levelMax;
    public int shotsTaken;
    public GameObject castle;
    public GameMode mode = GameMode.idle;

    void Start()
    {
        S = this;
        level = 0;
        shotsTaken = 0;
        levelMax = castles.Length;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        StartLevel();
    }

    void StartLevel()
    {
        if (castle != null) Destroy(castle);

        Projectile.DESTROY_PROJECTILES();

        castle = Instantiate(castles[level]);
        castle.transform.position = castlePos;

        Goal.goalMet = false;
        mode = GameMode.playing;
        UpdateGUI();

        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI()
    {
        if (uitLevel != null)
            uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;

        if (uitShots != null)
            uitShots.text = "Shots: " + shotsTaken + " / " + shotsAllowed;
    }

    void Update()
    {
        UpdateGUI();

        if (mode == GameMode.playing && Goal.goalMet)
        {
            mode = GameMode.levelEnd;
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);

            if (level == levelMax - 1)
                Invoke(nameof(ShowVictory), 2f);
            else
                Invoke(nameof(NextLevel), 2f);
        }
    }

    void NextLevel()
    {
        if (mode != GameMode.levelEnd) return;

        level++;
        shotsTaken = 0;
        StartLevel();
    }

    IEnumerator CheckForFailure()
    {
        float elapsed = 0f;

        // Give the projectile time to hit the castle or settle.
        while (elapsed < failureWaitTime)
        {
            if (mode != GameMode.playing || Goal.goalMet)
                yield break;

            if (shotsTaken >= shotsAllowed && FollowCam.POI == null)
            {
                ShowGameOver();
                yield break;
            }

            elapsed += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }

        if (mode == GameMode.playing && !Goal.goalMet &&
            shotsTaken >= shotsAllowed)
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        mode = GameMode.gameOver;
        FollowCam.POI = null;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void ShowVictory()
    {
        if (mode != GameMode.levelEnd) return;

        mode = GameMode.victory;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void PlayAgain()
    {
        CancelInvoke();
        StopAllCoroutines();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        level = 0;
        shotsTaken = 0;
        StartLevel();
    }

    static public void SHOT_FIRED()
    {
        if (S == null || S.mode != GameMode.playing) return;

        S.shotsTaken++;
        S.StartCoroutine(S.CheckForFailure());
    }

    static public bool IsGameOver()
    {
        return S != null &&
            (S.mode == GameMode.gameOver || S.mode == GameMode.victory);
    }

    static public GameObject GET_CASTLE()
    {
        return S != null ? S.castle : null;
    }
}