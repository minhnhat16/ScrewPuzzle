using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GemGroup : MonoBehaviour
{
    [SerializeField] private Dealer dealer;
    [SerializeField] private RectTransform gemlb;
    [SerializeField] private Vector3 target_Position;
    [SerializeField] Animator animator;
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private int radius;
    [SerializeField] Vector3 anchor3D;

    [HideInInspector] public UnityEvent<int> gemClaimEvent = new();
    private RectTransform rect;

    private void OnEnable()
    {
        gemClaimEvent = dealer.dealSlot.gemCollected;
        gemClaimEvent.AddListener(GroupGemSpawn);
    }
    private void OnDisable()
    {
        gemClaimEvent.RemoveListener(GroupGemSpawn);
    }
    private void Start()
    {
        StartCoroutine(GetGemLb());
        rect = GetComponent<RectTransform>();
    }
    IEnumerator GetGemLb()
    {
        ViewManager.Instance.dicView.TryGetValue(ViewIndex.GamePlayView, out BaseView view);
        yield return new WaitUntil(() => view != null);
        gemlb = view.GetComponent<GamePlayView>().GemParent;
        //transform.SetParent(view.transform);
        ScreenToWorld.Instance.SetWorldToAnchorView(transform.position, rect);
        anchor3D = rect.anchoredPosition3D;
        target_Position = gemlb.anchoredPosition3D - anchor3D;

    }
    public void SetPositionWithParent(GameObject parent)
    {
        Vector3 pos = parent.transform.position;
        ScreenToWorld.Instance.SetWorldToAnchorView(pos, rect);
    }
    public void SetTargetPosition(Vector3 target)
    {
        target_Position = target;
    }
    public void GroupGemSpawn(int amountGem)
    {
        //Debug.Log("GroupGoldSpawn");
        StartCoroutine(SpawnByTime(amountGem));
    }
    public int FixAmountSpawn(int amountGold)
    {
        int scale = amountGold / 100;
        if (scale < 1) return (int)SizeAmoutGold.S;
        else if (scale > 1 && scale < 5) return (int)SizeAmoutGold.M;
        else if (scale > 5 && scale < 10) return (int)SizeAmoutGold.L;
        else if (scale > 10 && scale < 20) return (int)SizeAmoutGold.XL;
        else return 2;
    }
    public IEnumerator SpawnByTime(int amountGem)
    {
        bool isSpawn = false;
        for (int i = 0; i < amountGem;)
        {
            isSpawn = true;
            SpawGoldUI(() =>
            {
                isSpawn = true;
            });
            yield return new WaitUntil(() => isSpawn == true);
            //yield return new WaitForSeconds(0.05f);
            i++;
        }
        DataAPIController.instance.AddGem(amountGem);

    }
    public void SpawGoldUI(Action callback)
    {
        //GameObject gemUI = Instantiate(gemPrefab, randomPos, Quaternion.identity, transform.parent);
        GemUI gemUI = GemPool.Instance.pool.SpawnNonGravity();
        gemUI.Transf.SetParent(transform);
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-10f, 10f);
        gemUI.Rect.anchoredPosition3D = new Vector3(x, y);
        gemUI.DoScaleUp(Vector3.zero, Vector3.one, () =>
        {
            gemUI.DoMoveToTarget(target_Position, () =>
            {
                if (gemUI != null)
                {
                    gemUI.Transf.SetParent(GemPool.Instance.gameObject.transform);
                    GemPool.Instance.pool.DeSpawnNonGravity(gemUI);
                }
                SoundManager.instance.PlaySFX(SoundManager.SFX.CoinSFX);
                callback?.Invoke();
            });

        });

    }
  
}
