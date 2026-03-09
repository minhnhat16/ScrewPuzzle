using DG.Tweening;
using Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoxStarReward : MonoBehaviour, IBoxReward
{
    [SerializeField] private GameObject starPrefab;

    [Header("Fly Settings")]
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float spawnInterval = 0.1f;
    [SerializeField] private float waitBeforeFly = 0.1f;
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float delayBetweenFlies = 0.08f;
    [SerializeField] private Ease flyEase = Ease.InBack;

    [Header("Sound")]
    [SerializeField]
    private List<SoundManager.SFX> streakSounds = new()
    {
        SoundManager.SFX.Star_1,
        SoundManager.SFX.Star_2,
        SoundManager.SFX.Star_3,
    };

    public event Action<int> OnStarLanded;

    public void SpawnReward(List<Vector3> spawnPositions)
    {
        if (starPrefab == null)
        {
            Debug.LogWarning("[BoxStarReward] starPrefab chưa assign.");
            return;
        }

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("[BoxStarReward] Không có vị trí spawn.");
            return;
        }

        var anchor = GetStarAnchor();
        if (anchor == null)
        {
            Debug.LogWarning("[BoxStarReward] Không tìm thấy star anchor.");
            return;
        }

        // Cache anchor position ngay — box sắp SetActive(false)
        Vector3 anchorPos = anchor.position;

        // Dùng DOTween Sequence thuần — không coroutine, không bị kill khi box deactivate
        var sequence = DOTween.Sequence()
            .SetUpdate(true)   // chạy kể cả khi timeScale = 0
            .SetAutoKill(true);

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            int idx = i;
            float spawnDelay = i * spawnInterval;

            // Spawn + pop tại thời điểm spawnDelay
            sequence.InsertCallback(spawnDelay, () =>
            {
                var star = Instantiate(starPrefab, spawnPositions[idx], Quaternion.identity);
                star.transform.localScale = Vector3.zero;

                // Pop scale
                star.transform
                    .DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                PlayStreakSound(idx);

                // Fly sau khi pop xong + waitBeforeFly
                float flyDelay = popDuration + waitBeforeFly;
                DOVirtual.DelayedCall(flyDelay, () =>
                {
                    if (star == null) return;
                    star.transform
                        .DOMove(anchorPos, flyDuration)
                        .SetEase(flyEase)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            IngameController.ins?.AddStar(1);
                            OnStarLanded?.Invoke(idx);
                            Destroy(star);
                        });
                }).SetUpdate(true);
            });
        }
    }

    private void PlayStreakSound(int streakIndex)
    {
        if (streakSounds == null || streakSounds.Count == 0) return;
        int idx = Mathf.Clamp(streakIndex, 0, streakSounds.Count - 1);
        SoundHelper.PlaySFX(streakSounds[idx]);
    }

    private Transform GetStarAnchor()
    {
        var gameView = ViewManager.Instance?.GetView<GameView>();
        return gameView?.StarAnchor;
    }
}