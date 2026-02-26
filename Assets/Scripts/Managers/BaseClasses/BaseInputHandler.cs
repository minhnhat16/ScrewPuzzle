using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class BaseInputHandler : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] protected Camera mainCam;

    [Header("Input Lock")]
    [SerializeField] protected bool isInputLocked = false;
    public bool IsInputLocked
    {
        get => isInputLocked;
        set => isInputLocked = value;
    }

    [Header("Tutorial")]
    [Tooltip("Id dùng cho TutorialInputController")]
    [SerializeField] protected string tutorialInputId;

    protected Coroutine inputLoop;

    public UnityEvent onClicked = new();
    public UnityEvent<Component> onObjectClicked = new();

    protected virtual void Awake()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    protected virtual void OnEnable()
    {
        if (inputLoop == null)
            inputLoop = StartCoroutine(InputLoop());
    }

    protected virtual void OnDisable()
    {
        if (inputLoop != null)
        {
            StopCoroutine(inputLoop);
            inputLoop = null;
        }
    }

    // ============================
    // MAIN INPUT LOOP
    // ============================
    protected virtual IEnumerator InputLoop()
    {
        while (true)
        {
            if (!isInputLocked && !IsClickOverUI())
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetMouseButtonDown(0))
                    TryHandleInput(Input.mousePosition);
#elif UNITY_ANDROID || UNITY_IOS
                if (Input.touchCount > 0 &&
                    Input.GetTouch(0).phase == TouchPhase.Began)
                    TryHandleInput(Input.GetTouch(0).position);
#endif
            }

            yield return null;
        }
    }

    // ============================
    // TUTORIAL GATE (KEY POINT)
    // ============================
    private void TryHandleInput(Vector3 screenPos)
    {
        // Tutorial gate
        if (!string.IsNullOrEmpty(tutorialInputId) )
        {
            return;
        }

        HandleInput(screenPos);
        onClicked?.Invoke();
    }

    // ============================
    // UI CHECK
    // ============================
    internal bool IsClickOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(
                Input.GetTouch(0).fingerId);
        return false;
#else
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }

    // ============================
    // GENERIC PICKER
    // ============================
    protected T PickAtScreenPos<T>(
        Vector3 screenPos,
        string requiredTag = null
    ) where T : Component
    {
        if (mainCam == null) return null;

        Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPos);
        var hits = Physics2D.RaycastAll(worldPos, Vector2.zero);

        int bestLayer = int.MaxValue;
        T best = null;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            var obj = hit.collider.gameObject;

            if (requiredTag != null && !obj.CompareTag(requiredTag))
                continue;

            if (!obj.TryGetComponent<T>(out var comp)) continue;

            int layer = obj.layer;
            if (layer < bestLayer)
            {
                bestLayer = layer;
                best = comp;
            }
        }

        return best;
    }

    protected T PickAtScreenPosScrew<T>(
        Vector3 screenPos,
        string requiredTag = null
    ) where T : Component
    {
        if (mainCam == null) return null;

        Vector2 worldPos = mainCam.ScreenToWorldPoint(screenPos);
        var hits = Physics2D.RaycastAll(worldPos, Vector2.zero);

        float highestZ = float.MinValue;
        T best = null;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            var obj = hit.collider.gameObject;

            if (requiredTag != null && !obj.CompareTag(requiredTag))
                continue;

            if (!obj.TryGetComponent<T>(out var comp)) continue;

            float z = obj.transform.position.z;
            if (z > highestZ)
            {
                highestZ = z;
                best = comp;
            }
        }

        return best;
    }

    // ============================
    // ABSTRACT
    // ============================
    protected abstract void HandleInput(Vector3 screenPos);
}
