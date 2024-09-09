using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashVfx : MonoBehaviour
{
    [SerializeField] ParticleSystem mainModule;
    [SerializeField] ParticleSystem splash;
    [SerializeField] ParticleSystem  shadow;
    [SerializeField] ParticleSystem drops;
    public float playbackSpeed = 1.0f;
    public void SetPositionAndRotation(Vector3 position, Quaternion q)
    {
        gameObject.transform.SetPositionAndRotation(position, q);
    }
    public void SetColorVFX(Color color)
    {
        AdjustParticleSystemSecondColorKey(mainModule, color);
        SetParticleSystemColor(splash, color);
        SetParticleSystemColor(shadow, color);
        SetParticleSystemColor(drops, color);
    }

    private void SetParticleSystemColor(ParticleSystem particleSystem, Color color)
    {
        var mainModule = particleSystem.main;
        mainModule.startColor = color;
    }
    public void PlayAndDeactivate()
    {
        SetPlaybackSpeed(splash, playbackSpeed);
        SetPlaybackSpeed(shadow, playbackSpeed);
        SetPlaybackSpeed(drops, playbackSpeed);

        // Start the coroutine to deactivate the GameObject after the ParticleSystems finish
        StartCoroutine(DeactivateAfterPlayback());
    }

    private void SetPlaybackSpeed(ParticleSystem particleSystem, float speed)
    {
        var mainModule = particleSystem.main;
        mainModule.simulationSpeed = speed;
    }

    private IEnumerator DeactivateAfterPlayback()
    {
        // Wait for all ParticleSystems to stop playing
        while (splash.isPlaying || shadow.isPlaying || drops.isPlaying)
        {
            yield return null;
        }

        // Deactivate the GameObject
        gameObject.SetActive(false);
    }
    private void AdjustParticleSystemSecondColorKey(ParticleSystem particleSystem, Color newColor)
    {
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // Retrieve the existing gradient
        Gradient gradient = colorOverLifetime.color.gradient;

        // Create new color keys based on the existing ones but adjust the second key
        GradientColorKey[] colorKeys = gradient.colorKeys;
        if (colorKeys.Length > 1)
        {
            colorKeys[1].color = newColor;
        }

        // Apply the new color keys to the gradient
        gradient.SetKeys(colorKeys, gradient.alphaKeys);

        // Assign the modified gradient back to the color over lifetime module
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }
}
