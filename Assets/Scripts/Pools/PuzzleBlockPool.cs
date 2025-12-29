using UnityEngine;

public class PuzzleBlockPool : SingletonMono<PuzzleBlockPool>
{
    [SerializeField] private PuzzleBlock prefab;
    public ByUIPoolUI<PuzzleBlock> pool;
    public int total = 10;

    public override void Awake()
    {
        base.Awake();
        pool = new ByUIPoolUI<PuzzleBlock>(prefab, total, transform);
    }

    public void OnDisable()
    {
        ReturnAll();
    }
    public PuzzleBlock Spawn()
    {
        return pool.Get();
    }
    public void Return(PuzzleBlock block)
    {
        pool.Release(block.GetComponent<PuzzleBlock>());
    }
   public void ReturnAll()
    {
        pool.ReleaseAll();
    }
}
