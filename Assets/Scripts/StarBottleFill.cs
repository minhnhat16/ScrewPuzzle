using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StarBottleFill : MonoBehaviour
{
    [SerializeField] private Image imgBackground;
    [SerializeField] private Image imgFill;
    public float animationDuration = 1.0f; // Time for the animation to complete

    public UnityEvent<float> fillChange = new();
    public void OnEnable()
    {
        fillChange.AddListener(AnimateToPercent);
    }


    public void Awake()
    {
        //imgBackground = transform.GetChild(0).GetComponent<Image>();
        //imgBackground = transform.GetChild(1).GetComponent<Image>();
    }


    /// <summary>
    /// Smoothly animates the progress bar to the given percentage.
    /// </summary>
    /// <param name="targetPercent">Target fill amount (0 to 1)</param>
    public void AnimateToPercent(float targetPercent)
    {
        if (imgFill == null) return;
        Debug.LogWarning($"Animate To Percent {targetPercent}");

        StartCoroutine(AnimateProgressCoroutine(targetPercent));
    }

    private IEnumerator AnimateProgressCoroutine(float targetPercent)
    {
        float startPercent = imgFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            imgFill.fillAmount = Mathf.Lerp(startPercent, targetPercent, t);
            yield return null; // Wait for the next frame
        }

        imgFill.fillAmount = targetPercent; // Ensure it ends exactly at the target
    }

    public void Reset()
    {
        imgFill.fillAmount = 0;
    }
}
