using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

// Rename this class to avoid circular inheritance with UnityEngine.UI.Toggle
[Serializable]
public class CustomToggle : MonoBehaviour
{
    public Toggle m_Toggle;
    [SerializeField]
    private Image activeIcon;
    [SerializeField]
    private Image disabledIcon;
    private Sequence swapSeq;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }
    protected void OnEnable()
    {
        m_Toggle.onValueChanged.AddListener(SwapSprite);
    }
    protected void OnDisable()
    {
        m_Toggle.onValueChanged.RemoveListener(SwapSprite);
    }
    public void SwapSprite(bool value)
    {
        Debug.Log("Swap sprite: " + value);
        swapSeq?.Kill();

        transform.localScale = originalScale;

        swapSeq = DOTween.Sequence().SetUpdate(true); ;

        swapSeq.Append(
            transform.DOPunchScale(Vector3.one * 0.5f, 0.25f, 10, 5)
        );

        swapSeq.Join(
            disabledIcon.DOFade(value ? 0f : 1f, 0.25f)
        );


        swapSeq.Join(
            activeIcon.DOFade(value ? 1f : 0f, 0.25f)
        );

    }

}
