using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Central router: subscribes to InputController and dispatches to ITappable hits.
/// Keep input acquisition separate from game logic (Player).
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }

    [SerializeField] private InputController inputController;

    public bool IsInputLocked { get; set; } = false;
    // High level events
    public event Action<ITappable, Vector2> OnTappableTapped;
    public event Action<Vector2> OnTapGlobal; // fallback for non-tappable systems

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        if (inputController == null)
            inputController = FindAnyObjectByType<InputController>();

        if (inputController == null)
            Debug.LogWarning("[InputRouter] No InputController found in scene.");
    }

    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.OnTap += HandleTap;
            inputController.OnDragStart += pos => { };
            inputController.OnDrag += pos => { };
            inputController.OnDragEnd += pos => { };
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
            inputController.OnTap -= HandleTap;
    }

    private void HandleTap(Vector2 screenPos)
    {
        if (IsInputLocked) return ;
        // Global hook
        OnTapGlobal?.Invoke(screenPos);

        // Block if pointer over blocking UI (preserve existing behavior)
        if (EventSystem.current != null && IsPointerOverBlockingUI(screenPos))
            return;

        var tappable = PickTappableAt(screenPos);
        if (tappable != null)
        {
            // If the tappable consumes the tap (returns true), do NOT forward to Player.
            // If it DOES NOT consume (returns false), the router forwards to game-level listeners (Player).
            bool consumed = tappable.OnTap(screenPos);
            if (!consumed)
            {
                OnTappableTapped?.Invoke(tappable, screenPos);
            }
            return;

        }
    }

    private bool IsPointerOverBlockingUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<SpotlightRaycastBlocker>() == null)
                return true;
        }
        return false;
    }

    private ITappable PickTappableAt(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;
        Vector2 worldPoint = cam.ScreenToWorldPoint(screenPos);

        var colliders = Physics2D.OverlapPointAll(worldPoint);
        if (colliders == null || colliders.Length == 0) return null;

        // candidate list with sort keys
        var candidates = new System.Collections.Generic.List<(ITappable tappable, int layerValue, int order, float z, MonoBehaviour mb)>();

        foreach (var c in colliders)
        {
            if (c == null) continue;
            var mbs = c.GetComponentsInParent<MonoBehaviour>(true);
            if (mbs == null || mbs.Length == 0) continue;

            var mb = mbs.FirstOrDefault(m => m is ITappable) as MonoBehaviour;
            if (mb == null) continue;

            var tappable = mb as ITappable;
            int layerValue = 0; // higher = on top
            int order = 0;
            float z = mb.transform.position.z;

            // Prefer any SpriteRenderer sorting data if present
            var sr = mb.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                layerValue = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
                order = sr.sortingOrder;
            }
            else
            {
                // Fallback: if this is ScrewController, use its sorting helpers
                var sc = mb as Ingame.Screw.ScrewController;
                if (sc != null)
                {
                    try
                    {
                        var name = sc.GetSortingLayerName();
                        int id = SortingLayer.NameToID(name);
                        layerValue = SortingLayer.GetLayerValueFromID(id);
                    }
                    catch
                    {
                        layerValue = 0;
                    }
                    order = sc.GetSortingOrder();
                }
                else
                {
                    // final fallback: use negative z (closer to camera -> higher priority)
                    order = Mathf.RoundToInt(-z * 1000f);
                }
            }

            candidates.Add((tappable, layerValue, order, z, mb));
        }

        if (candidates.Count == 0) return null;

        // sort by: sorting layer value desc, sorting order desc, world z asc (smaller z = closer to camera usually)
        var best = candidates
            .OrderByDescending(x => x.layerValue)
            .ThenByDescending(x => x.order)
            .ThenBy(x => x.z) // prefer smaller z (closer to camera)
            .FirstOrDefault();

        return best.tappable;
    }
}