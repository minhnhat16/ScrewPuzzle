using UnityEngine;

public class PuzzleCellPool : SingletonMono<PuzzleCellPool>
{
    [SerializeField] private PuzzleCellUI prefab;
    public ByUIPoolUI<PuzzleCellUI> pool;
    public int total = 25;

    public override void Awake()
    {
        base.Awake();
        pool = new ByUIPoolUI<PuzzleCellUI>(prefab, total, transform);
    }

    public void OnDisable()
    {
        ReturnAll();
    }
    public PuzzleCellUI Spawn()
    {
        return pool.Get();
    }
    public void Return(PuzzleCellUI block)
    {
        pool.Release(block.GetComponent<PuzzleCellUI>());
    }
   public void ReturnAll()
    {
        pool.ReleaseAll();
    }
}
