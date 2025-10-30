using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class CanvasScriptHolder : MonoBehaviour
{
    [Header("UI Reference")]
    public Text timerText;
    public Button FullScreenButton;
    public Button EdgeViewButton;

    public Color ChangeColor;

    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool isCompleted;
    private bool isEdgeViewActive = false;  

    [SerializeField] private ParticleSystem confetti;

    private List<PuzzlePiece> allPieces = new List<PuzzlePiece>();

    [SerializeField] private Text TimerText;
    [SerializeField] private GameObject ResultObject;

    public void StartGameButton(int count)
    {
        GameManager.instance.totalPieceCount = count;
        GameManager.instance.SetGameStart();
        Init();
    }

    public void ResultGame()
    {
        ResultObject.SetActive(true);
        TimerText.text = timerText.text;
    }

    public void ReturnAll() => SceneManager.LoadScene("SampleScene");

    public void Init()
    {
        StartTimer();
        FullScreenButton.onClick.AddListener(ToggleFullscreen);
        EdgeViewButton.onClick.AddListener(ToggleEdgeView);

        allPieces.AddRange(FindObjectsOfType<PuzzlePiece>());
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    void Update()
    {
        if (!isRunning) return;

        if (GameManager.instance.isGameCompleted)
        {
            if (isCompleted == false)
            {
                isCompleted = true;
                confetti.Play();
                StartCoroutine(DelayResult());
            }
            return;
        }

        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private IEnumerator DelayResult()
    {
        yield return new WaitForSeconds(2.0f);
        ResultGame();
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }
    void ToggleEdgeView()
    {
        isEdgeViewActive = !isEdgeViewActive;

        foreach (var piece in allPieces)
        {
            if (piece == null) continue;

            Transform parent = piece.transform.parent;
            bool isGrouped = parent != null && parent.name.StartsWith("Group_");
            if (isGrouped)
                continue;

            int childCount = piece.transform.childCount;

            var sr = piece.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            if (isEdgeViewActive)
            {
                if (childCount > 3)
                    SetAlpha(sr, 0.2f); 
                else
                    SetAlpha(sr, 1f);  
            }
            else
            {
                SetAlpha(sr, 1f);
            }
            EdgeViewButton.GetComponent<Image>().color = isEdgeViewActive ? ChangeColor : Color.white;
        }
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
