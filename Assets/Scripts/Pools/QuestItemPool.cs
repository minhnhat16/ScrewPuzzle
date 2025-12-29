

using UnityEngine;

public class QuestItemPool : SingletonMono<QuestItemPool>

{
    [SerializeField] private QuestItem prefab;
    public ByUIPoolUI<QuestItem> pool;
    public int total = 10;


    public override void Awake()
    {
        base.Awake();
        pool = new ByUIPoolUI<QuestItem>(prefab, total, transform);
    }

    public QuestItem Spawn()
    {
        return pool.Get();
    }
    public void Return(QuestItem questGameObject)
    {
        pool.Release(questGameObject.GetComponent<QuestItem>());
    }
}