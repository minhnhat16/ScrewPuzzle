using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    public string targetKey;

    private void Awake()
    {
        TutorialTargetRegistry.Register(targetKey, transform);
    }

    private void OnDestroy()
    {
        TutorialTargetRegistry.Unregister(targetKey);
    }
}
