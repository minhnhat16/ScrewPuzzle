using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class IngameController : MonoBehaviour
{
    public static IngameController instance;
    [SerializeField] public bool isOnMagnet;
    [SerializeField] public bool isOnBomb;

    [SerializeField] private float exp_Current;
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private CardType _currentCardType;
    [SerializeField] public GameObject IngameUI;
    [SerializeField] public List<SlotData> slotData;
    [HideInInspector] public UnityEvent<int> onGoldChanged;
    [HideInInspector] public UnityEvent<int> onGemChanged;
    [HideInInspector] public UnityEvent<int> onDealerClaimGold;
    [HideInInspector] public UnityEvent<int> onDealerClaimGem;
    [HideInInspector] public UnityEvent<float> onExpChange;
    [HideInInspector] public UnityEvent<bool> onBombEvent;
    [HideInInspector] public UnityEvent<bool> onMagnetEvent;
  
    public float Exp_Current
    {
        get { return exp_Current; }
        set { exp_Current = value; }
    }

    public CardType CurrentCardType
    {
        get => _currentCardType;
        set => _currentCardType = value;
    }

 

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Awake()
    {
        instance = this;
    }

    public void Init(Action callback)
    {
        StartCoroutine(InitIngameCoroutine(callback));
    }

    public IEnumerator InitIngameCoroutine(Action callback)
    {
        yield return new WaitForSeconds(0f);

        // Callback when initialization is done
        callback?.Invoke();
    }
   public LayerMask GetLayerMaskForRange(int startLayer, int endLayer)
    {
        LayerMask mask = 0;
    
        for (int i = startLayer; i <= endLayer; i++)
        {
            mask |= (1 << i); // Set the bit for each layer in the range
        }
    
        return mask;
    }
}
 
  

