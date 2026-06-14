using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingMinigame : MonoBehaviour
{
    public static CookingMinigame Instance;

    [Header("UI")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressFill;

    [Header("Colors")]
    [SerializeField] private Color goodColor = new Color(0.4f, 0.85f, 0.4f);
    [SerializeField] private Color warningColor = new Color(0.95f, 0.75f, 0.2f);
    [SerializeField] private Color badColor = new Color(0.9f, 0.3f, 0.3f);

    private bool minigameActive = false;

    public bool IsMinigameActive => minigameActive;
    private float progress = 0f;
    private System.Action onComplete;

    private Coroutine activeMinigame;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        Instance = this;
        minigamePanel.SetActive(false);
        progressBar.gameObject.SetActive(false);

        feedbackText.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(false);
    }

    public void StartMinigame(MinigameType type, System.Action onFinished)
    {
        if (minigameActive) return;

        onComplete = onFinished;
        progress = 0f;
        minigameActive = true;
        minigamePanel.SetActive(true);
        progressBar.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        instructionText.gameObject.SetActive(true);
        progressBar.value = 0f;

        if (activeMinigame != null)
            StopCoroutine(activeMinigame);

        switch (type)
        {
            case MinigameType.Crush:
                activeMinigame = StartCoroutine(CrushMinigame());
                break;
            case MinigameType.CircularMix:
                activeMinigame = StartCoroutine(CircularMixMinigame());
                break;
            case MinigameType.Beat:
                activeMinigame = StartCoroutine(BeatMinigame());
                break;
            case MinigameType.Pour:
                activeMinigame = StartCoroutine(PourMinigame());
                break;
            case MinigameType.Fold:
                activeMinigame = StartCoroutine(FoldMinigame());
                break;
            case MinigameType.Wait:
                activeMinigame = StartCoroutine(WaitMinigame());
                break;
        }
    }

    void EndMinigame()
    {
        minigameActive = false;
        minigamePanel.SetActive(false);

        instructionText.text = "";
        feedbackText.gameObject.SetActive(false);
        progressBar.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onComplete?.Invoke();
    }

    void SetInstruction(string text) => instructionText.text = text;

    void ShowFeedback(string text, Color color)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackFlash(text, color));
    }

    IEnumerator FeedbackFlash(string text, Color color)
    {
        feedbackText.text = text;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        feedbackText.gameObject.SetActive(false);
    }

    void UpdateBar(float value)
    {
        progressBar.value = value;
        if (progressFill != null)
            progressFill.color = Color.Lerp(warningColor, goodColor, value);
    }

    IEnumerator CrushMinigame()
    {
        SetInstruction("CLICK rapidly to crush the graham crackers!");
        float required = 20f;
        float clicks = 0f;
        float decay = 2f;

        while (clicks < required)
        {
            if (Input.GetMouseButtonDown(0))
            {
                clicks += 1f;
                ShowFeedback("CRUNCH!", goodColor);
            }

            clicks = Mathf.Max(0, clicks - decay * Time.deltaTime);
            UpdateBar(clicks / required);
            yield return null;
        }

        ShowFeedback("Crushed!", goodColor);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
    }

    IEnumerator CircularMixMinigame()
    {
        SetInstruction("Move the mouse in circles to mix!");
        float required = 5f;
        float circles = 0f;
        float lastAngle = 0f;
        float totalDelta = 0f;
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        yield return null;

        while (circles < required)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 dir = mousePos - center;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float delta = Mathf.DeltaAngle(lastAngle, angle);
            lastAngle = angle;

            if (Mathf.Abs(delta) < 30f)
            {
                totalDelta += delta;
                if (Mathf.Abs(totalDelta) >= 360f)
                {
                    circles += 1f;
                    totalDelta = 0f;
                    ShowFeedback("Nice circle!", goodColor);
                }
            }

            UpdateBar(circles / required);
            yield return null;
        }

        ShowFeedback("Mixed!", goodColor);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
    }

    IEnumerator BeatMinigame()
    {
        SetInstruction("Shake the mouse left and right to beat the cream cheese!");
        float required = 8f;
        float shakes = 0f;
        float lastX = Input.mousePosition.x;
        float shakeThreshold = 40f;
        bool movingRight = true;

        while (shakes < required)
        {
            float currentX = Input.mousePosition.x;
            float delta = currentX - lastX;

            if (movingRight && delta < -shakeThreshold)
            {
                movingRight = false;
                shakes += 0.5f;
                ShowFeedback("Beat!", goodColor);
            }
            else if (!movingRight && delta > shakeThreshold)
            {
                movingRight = true;
                shakes += 0.5f;
                ShowFeedback("Beat!", goodColor);
            }

            lastX = currentX;
            UpdateBar(shakes / required);
            yield return null;
        }

        ShowFeedback("Smooth!", goodColor);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
    }

    IEnumerator PourMinigame()
    {
        SetInstruction("Hold the mouse button and slowly move DOWN to pour!");
        float required = 150f;
        float poured = 0f;
        float lastY = 0f;
        bool holding = false;

        while (poured < required)
        {
            if (Input.GetMouseButtonDown(0))
            {
                holding = true;
                lastY = Input.mousePosition.y;
            }
            if (Input.GetMouseButtonUp(0))
                holding = false;

            if (holding)
            {
                float currentY = Input.mousePosition.y;
                float delta = lastY - currentY;
                lastY = currentY;

                if (delta > 0 && delta < 20f)
                {
                    poured += delta;
                    ShowFeedback("Pouring...", goodColor);
                }
                else if (delta >= 20f)
                {
                    ShowFeedback("Too fast!", badColor);
                }
            }

            UpdateBar(poured / required);
            yield return null;
        }

        ShowFeedback("Added!", goodColor);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
    }

    IEnumerator FoldMinigame()
    {
        SetInstruction("Gently fold side to side. Take it easy!");
        float required = 4f;
        float folds = 0f;
        float overmix = 0f;
        float foldThreshold = 80f;
        float speedLimit = 600f;
        float accumulated = 0f;
        float lastX = Input.mousePosition.x;
        int direction = 0;

        while (folds < required && overmix < 3f)
        {
            float currentX = Input.mousePosition.x;
            float frameDelta = currentX - lastX;
            float speed = Mathf.Abs(frameDelta) / Time.deltaTime;
            lastX = currentX;

            if (speed > speedLimit)
            {
                overmix += Time.deltaTime * 0.5f;
                ShowFeedback("Too fast! Don't overmix!", badColor);
                if (progressFill != null)
                    progressFill.color = Color.Lerp(goodColor, badColor, overmix / 3f);
            }
            else if (Mathf.Abs(frameDelta) > 0.1f)
            {
                int newDir = frameDelta > 0 ? 1 : -1;

                if (direction == 0)
                    direction = newDir;

                if (newDir == direction)
                {
                    accumulated += Mathf.Abs(frameDelta);

                    if (accumulated >= foldThreshold)
                    {
                        accumulated = 0f;
                        direction = -newDir;
                        folds += 1f;
                        ShowFeedback("Good fold!", goodColor);
                    }
                }
                else
                {
                    direction = newDir;
                    accumulated = Mathf.Abs(frameDelta);
                }
            }

            UpdateBar(Mathf.Clamp01(folds / required));
            yield return null;
        }

        if (overmix >= 3f)
        {
            ShowFeedback("Overmixed! But continuing...", badColor);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            ShowFeedback("Perfectly folded!", goodColor);
            yield return new WaitForSeconds(0.5f);
        }

        EndMinigame();
    }

    IEnumerator WaitMinigame()
    {
        SetInstruction("Refrigerating... please wait.");
        float duration = 6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            UpdateBar(elapsed / duration);

            int remaining = Mathf.CeilToInt(duration - elapsed);
            feedbackText.text = $"{remaining}s";
            feedbackText.color = goodColor;
            feedbackText.gameObject.SetActive(true);

            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        ShowFeedback("Done!", goodColor);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
    }
}

public enum MinigameType
{
    Crush,
    CircularMix,
    Beat,
    Pour,
    Fold,
    Wait
}