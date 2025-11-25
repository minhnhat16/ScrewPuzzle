using Ingame.Board;
using UnityEngine;

public class LayerHelperDebug : MonoBehaviour
{
    [SerializeField] private KeyCode showNextLayerKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode hideTopLayerKey = KeyCode.DownArrow;

    // Locate the LayerManager on demand (keeps inspector clean and works in edit/play)
    private LayerManager LayerManager => FindAnyObjectByType<LayerManager>();

    private void Update()
    {
        // Poll keyboard only (no external OnKeyboardInput dependency)
        if (Input.GetKeyDown(showNextLayerKey))
        {
            TryShowNextLayer();
        }

        if (Input.GetKeyDown(hideTopLayerKey))
        {
            TryHideTopLayer();
        }
    }

    private void TryShowNextLayer()
    {
        var lm = LayerManager;
        if (lm == null)
        {
            Debug.LogWarning("LayerManager not found when trying to show next layer.");
            return;
        }

        var vc = lm.visibilityController;
        if (vc == null)
        {
            Debug.LogWarning("VisibilityController not found on LayerManager.");
            return;
        }

        vc.ShowNextLayer();
    }

    private void TryHideTopLayer()
    {
        var lm = LayerManager;
        if (lm == null)
        {
            Debug.LogWarning("LayerManager not found when trying to hide top layer.");
            return;
        }

        var vc = lm.visibilityController;
        if (vc == null)
        {
            Debug.LogWarning("VisibilityController not found on LayerManager.");
            return;
        }

        vc.HideTopLayer();
    }
}