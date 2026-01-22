using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public abstract class BaseInputHandler : MonoBehaviour
{
    [SerializeField] protected Camera mainCam;
    [SerializeField] protected bool isInputLocked = false;
    public bool IsInputLocked
    {
        get => isInputLocked;
        set => isInputLocked = value;
    }
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
                    HandleInput(Input.mousePosition);
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                HandleInput(Input.GetTouch(0).position);
#endif
            }

            yield return null;
        }
    }
    private bool IsClickOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#else
    return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
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

        int bestLayer = int.MaxValue; // layer nhỏ hơn = ưu tiên cao
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

    protected T PickAtScreenPosScrew<T>(Vector3 screenPos, string requiredTag = null) where T : Component



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
    // ABSTRACT FOR CHILDREN
    // ============================
    protected abstract void HandleInput(Vector3 screenPos);
}
