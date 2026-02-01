using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIPaintOnPath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image pathImage;
    [SerializeField] private RectTransform endRect;
    [SerializeField] private RectTransform startRect;

    [Header("Start gate")]
    [SerializeField] private bool mustStartInStartRect = true;
    [SerializeField] private float startRadiusPx = 0f; 

    [Header("Intro gate (no events)")]
    [SerializeField] private Animator introAnimator;     
    [SerializeField] private string introStateName = "Intro"; 
    [SerializeField] private int introLayer = 0;
    [SerializeField] private bool blockInputUntilIntroEnds = true;

    [Header("Brush")]
    [SerializeField] private int brushRadius = 8;
    [SerializeField] private Color32 brushColor = new Color32(0, 255, 255, 200);

    [Header("Path alpha hit")]
    [Range(0f, 1f)]
    [SerializeField] private float alphaThreshold = 0.2f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Timer")]
    [SerializeField] private float timeLimit = 10f;

    [Header("Next scene")]
    public string nextSceneName;     
    public float delayBeforeNextScene = 2f;
    public ExposureByResult_URP exposure;

    private Camera uiCamera;

    private Texture2D runtimeTexture;
    private Rect spriteRect;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerData;
    private readonly List<RaycastResult> results = new();

    private bool drawing;
    private bool locked;

    private float timeLeft;
    private bool timerRunning;

    void Start()
    {
        if (!pathImage || !pathImage.sprite)
        {
            Debug.LogError("UIPaintOnPath: pathImage or sprite missing.");
            enabled = false;
            return;
        }

        if (!introAnimator)
            blockInputUntilIntroEnds = false;

        // Auto UI camera: Overlay => null, ScreenSpaceCamera/World => canvas.worldCamera
        if (pathImage.canvas && pathImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = pathImage.canvas.worldCamera;

        // Alpha hit-test for path detection
        pathImage.raycastTarget = true;
        pathImage.alphaHitTestMinimumThreshold = alphaThreshold;

        // Raycaster + EventSystem
        Canvas c = pathImage.canvas;
        if (!c)
        {
            Debug.LogError("UIPaintOnPath: pathImage is not under a Canvas.");
            enabled = false;
            return;
        }

        raycaster = c.GetComponent<GraphicRaycaster>();
        if (!raycaster)
        {
            Debug.LogError("UIPaintOnPath: Canvas is missing a GraphicRaycaster component.");
            enabled = false;
            return;
        }

        eventSystem = EventSystem.current;
        if (!eventSystem)
        {
            Debug.LogError("UIPaintOnPath: No EventSystem in scene (GameObject > UI > Event System).");
            enabled = false;
            return;
        }
        pointerData = new PointerEventData(eventSystem);

        // Clone sprite into runtimeTexture we can paint on
        Sprite original = pathImage.sprite;
        spriteRect = original.textureRect;
        Texture2D sourceTex = original.texture;

        runtimeTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height, TextureFormat.RGBA32, false);

        Color[] pixels = sourceTex.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply(false);

        Sprite newSprite = Sprite.Create(
            runtimeTexture,
            new Rect(0, 0, runtimeTexture.width, runtimeTexture.height),
            new Vector2(0.5f, 0.5f),
            original.pixelsPerUnit
        );
        pathImage.sprite = newSprite;

        // Timer prepared but not running
        timeLeft = timeLimit;
        timerRunning = false;
        if (timerText) timerText.text = Mathf.CeilToInt(timeLeft).ToString();

    }

    void Update()
    {
        if (locked) return;

        if (blockInputUntilIntroEnds)
        {
            if (!IntroFinishedThisFrame())
                return;

            blockInputUntilIntroEnds = false; 
        }

        if (timerRunning)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                if (timerText) timerText.text = "0";
                Fail();
                return;
            }
            if (timerText) timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }

        Vector2 screen = Input.mousePosition;

        // Start drawing
        if (Input.GetMouseButtonDown(0))
        {
            // Must begin at start (prevents starting near the end)
            if (mustStartInStartRect && !IsOnStart(screen)) { Fail(); return; }

            if (!IsOnPath(screen)) { Fail(); return; }

            if (!timerRunning) timerRunning = true;

            drawing = true;
            Paint(screen);
        }

        // Stop drawing
        if (Input.GetMouseButtonUp(0))
        {
            if (drawing && !locked)
            {
                Fail();
                return;
            }
        }

        if (!drawing) return;

        // Fail if leave path
        if (!IsOnPath(screen)) { Fail(); return; }

        // Paint
        Paint(screen);

        // Win
        if (endRect && RectTransformUtility.RectangleContainsScreenPoint(endRect, screen, uiCamera))
            Win();
    }

    private bool IsIntroFinished()
    {
        // If no animator assigned, don't block at all
        if (!introAnimator) return true;

        var st = introAnimator.GetCurrentAnimatorStateInfo(introLayer);

        // If state name doesn’t match, you may be in a transition; you can either:
        // - return false until it enters Intro, or
        // - allow immediately.
        // We'll be strict:
        if (!st.IsName(introStateName)) return false;

        // normalizedTime goes 0..1 for one play. >=1 means finished.
        return st.normalizedTime >= 1f;
    }

    private bool IsOnPath(Vector2 screen)
    {
        if (!raycaster || pointerData == null) return false;

        pointerData.position = screen;
        results.Clear();
        raycaster.Raycast(pointerData, results);

        foreach (var r in results)
            if (r.gameObject == pathImage.gameObject)
                return true;

        return false;
    }

    private bool IsOnStart(Vector2 screen)
    {
        if (!startRect) return true;

        if (startRadiusPx <= 0f)
            return RectTransformUtility.RectangleContainsScreenPoint(startRect, screen, uiCamera);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            startRect, screen, uiCamera, out Vector2 local);

        Rect r = startRect.rect;
        r.xMin -= startRadiusPx;
        r.xMax += startRadiusPx;
        r.yMin -= startRadiusPx;
        r.yMax += startRadiusPx;

        return r.Contains(local);
    }
    private bool IntroFinishedThisFrame()
    {
        // No animator => no blocking
        if (!introAnimator) return true;

        // If no state name provided, don't block
        if (string.IsNullOrWhiteSpace(introStateName)) return true;

        // If it's transitioning, consider intro not finished yet
        if (introAnimator.IsInTransition(introLayer))
            return false;

        var st = introAnimator.GetCurrentAnimatorStateInfo(introLayer);

        // Only block while we're in the named intro state
        if (!st.IsName(introStateName))
            return true; // not in intro state => allow input

        // In intro state: wait until it finishes
        return st.normalizedTime >= 1f;
    }

    private void Paint(Vector2 screen)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(pathImage.rectTransform, screen, uiCamera, out Vector2 local))
            return;

        Rect imageRect = pathImage.rectTransform.rect;
        Rect drawnRect = GetDrawnSpriteRect(imageRect, pathImage.sprite, pathImage.preserveAspect);

        if (!drawnRect.Contains(local))
            return;

        float u = Mathf.InverseLerp(drawnRect.xMin, drawnRect.xMax, local.x);
        float v = Mathf.InverseLerp(drawnRect.yMin, drawnRect.yMax, local.y);

        int px = Mathf.Clamp(Mathf.RoundToInt(u * (runtimeTexture.width - 1)), 0, runtimeTexture.width - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (runtimeTexture.height - 1)), 0, runtimeTexture.height - 1);

        // If vertically flipped, uncomment:
        // py = (runtimeTexture.height - 1) - py;

        DrawCircle(px, py);
        runtimeTexture.Apply(false);
    }

    private void DrawCircle(int cx, int cy)
    {
        int r = brushRadius;
        int r2 = r * r;

        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y > r2) continue;

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= runtimeTexture.width || py < 0 || py >= runtimeTexture.height)
                    continue;

                runtimeTexture.SetPixel(px, py, brushColor);
            }
        }
    }

    private void Fail()
    {
        exposure.OnLose(); 
        locked = true;
        drawing = false;
        timerRunning = false;
        if (!string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(LoadNextSceneAfterDelay());
    }

    private void Win()
    {
        if (MedZoneContext.CurrentWalker != null)
        {
            MedZoneContext.CurrentWalker.Heal();
        }
        exposure.OnWin(); 
        locked = true;
        drawing = false;
        timerRunning = false;
        if (!string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(LoadNextSceneAfterDelay());
    }

    private Rect GetDrawnSpriteRect(Rect imageRect, Sprite sprite, bool preserveAspect)
    {
        if (!preserveAspect) return imageRect;

        float spriteW = sprite.rect.width;
        float spriteH = sprite.rect.height;

        float rectW = imageRect.width;
        float rectH = imageRect.height;

        float spriteAspect = spriteW / spriteH;
        float rectAspect = rectW / rectH;

        if (rectAspect > spriteAspect)
        {
            float newW = rectH * spriteAspect;
            float x = imageRect.x + (rectW - newW) * 0.5f;
            return new Rect(x, imageRect.y, newW, rectH);
        }
        else
        {
            float newH = rectW / spriteAspect;
            float y = imageRect.y + (rectH - newH) * 0.5f;
            return new Rect(imageRect.x, y, rectW, newH);
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene(nextSceneName);
    }
}
