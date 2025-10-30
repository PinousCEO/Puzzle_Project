using UnityEngine;
using UnityEngine.UI;
public class CanvasScriptHolder : MonoBehaviour
{
    [Header("UI Reference")]
    public Text timerText;
    public Button FullScreenButton;

    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool isCompleted;

    [SerializeField] private ParticleSystem confetti;
    void Start()
    {
        StartTimer();
        FullScreenButton.onClick.AddListener(ToggleFullscreen);
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
            }
            return;
        }
        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }
}
