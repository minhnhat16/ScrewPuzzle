using System;
using System.Collections;
using System.ConfigFile;
using ConfigFile;
using UnityEngine;
using Action = System.Action;

public class ConfigFileManager : MonoBehaviour
{
    public static ConfigFileManager Instance;
    public bool isDone;

    [Header("CSV configs")] [SerializeField]
    private LevelConfig levelConfig;

    [SerializeField] private PriceConfig priceConfig;
    [SerializeField] private PackConfig packConfig;

    [SerializeField] private CollectionConfig collectionConfig;

    [SerializeField] private ColorConfig colorConfig;
    [SerializeField] private ItemConfig itemConfig;
    [SerializeField] private DailyRewardConfig dailyConfig;
    [SerializeField] private SpinConfig spinConfig;
    [Header("Factory")] [SerializeField] private SoundFactory soundFactory;

    public LevelConfig LevelConfig
    {
        get => levelConfig;
    }

    public PriceConfig PriceConfig
    {
        get => priceConfig;
    }

    public PackConfig PackConfig => packConfig;

    public ColorConfig ColorConfig
    {
        get => colorConfig;
    }

    public ItemConfig ItemConfig
    {
        get => itemConfig;
    }

    public DailyRewardConfig DailyRewardConfig
    {
        get => dailyConfig;
    }

    public SpinConfig SpinConfig
    {
        get => spinConfig;
    }

    public SoundFactory SoundFactory
    {
        get => soundFactory;
    }
    public CollectionConfig CollectionConfig { get => collectionConfig; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Init(null);
    }

    public void Init(Action callback)
    {
        Debug.Log("(BOOT) // INIT CONFIG");
        StartCoroutine(WaitInit(callback));
    }

    // TODO: FIX PRICE SLOT CONFIG DOESNT INIT ON CONFIG FILE MANAGER INIT
    IEnumerator WaitInit(Action callback)
    {
        isDone = false;
        /*levelConfig = Resources.Load("Config/LevelConfig", typeof(ScriptableObject)) as LevelConfig;
        yield return new WaitUntil(() => levelConfig != null);
        slotConfig = Resources.Load("Config/SlotConfig", typeof(ScriptableObject)) as SlotConfig;
        yield return new WaitUntil(() => slotConfig != null);*/
        colorConfig = Resources.Load("Config/ColorConfig", typeof(ScriptableObject)) as ColorConfig;
        yield return new WaitUntil(() => colorConfig != null);
        itemConfig = Resources.Load("Config/ItemConfig", typeof(ScriptableObject)) as ItemConfig;
        yield return new WaitUntil(() => ItemConfig != null);
        priceConfig = Resources.Load("Config/PriceConfig", typeof(ScriptableObject)) as PriceConfig;
        yield return new WaitUntil(() => priceConfig != null);
        packConfig = Resources.Load("Config/PackConfig", typeof(ScriptableObject)) as PackConfig;
        yield return new WaitUntil(() => packConfig != null);
        collectionConfig = Resources.Load("Config/CollectionConfig", typeof(ScriptableObject)) as CollectionConfig;
        yield return new WaitUntil(() => collectionConfig != null);
        dailyConfig = Resources.Load("Config/DailyRewardConfig", typeof(ScriptableObject)) as DailyRewardConfig;
        yield return new WaitUntil(() => dailyConfig != null);
        /* spinConfig = Resources.Load("Config/SpinConfig", typeof(ScriptableObject)) as SpinConfig;
         yield return new WaitUntil(() => spinConfig != null);*/

        soundFactory = Resources.Load("Factory/SoundFactory", typeof(ScriptableObject)) as SoundFactory;
        // SoundManager.instance.Init();
        Debug.Log("(BOOT) // INIT CONFIG DONE");
        yield return new WaitUntil(() => soundFactory != null);
        yield return null;
        isDone = true;
        callback?.Invoke();
    }
}