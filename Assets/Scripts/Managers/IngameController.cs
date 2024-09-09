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
    readonly List<Slot> _slot = new();
    [SerializeField] List<Slot> _slotSorted = new();
    [SerializeField] private int playerLevel;
    [SerializeField] public bool isOnMagnet;
    [SerializeField] public bool isOnBomb;

    [SerializeField] private float exp_Current;
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private CardType _currentCardType;
    [SerializeField] private Player player;
    [SerializeField] public DealerParent dealerParent;
    [SerializeField] public SlotCamera slotCam;
    [SerializeField] public GameObject IngameUI;
    [SerializeField] public List<SlotData> slotData;
    [HideInInspector] public UnityEvent<int> onGoldChanged;
    [HideInInspector] public UnityEvent<int> onGemChanged;
    [HideInInspector] public UnityEvent<int> onDealerClaimGold;
    [HideInInspector] public UnityEvent<int> onDealerClaimGem;
    [HideInInspector] public UnityEvent<float> onExpChange;
    [HideInInspector] public UnityEvent<bool> onBombEvent;
    [HideInInspector] public UnityEvent<bool> onMagnetEvent;

    public float Exp_Current { get { return exp_Current; } set { exp_Current = value; } }

    public CardType CurrentCardType { get => _currentCardType; set => _currentCardType = value; }

    public int GetPlayerLevel()
    {
        //Debug.Log($"Player level {playerLevel}"); 
        playerLevel = DataAPIController.instance.GetPlayerLevel();
        return playerLevel;
    }

    public void SetPlayerLevel(int level)
    {
        if (level <= playerLevel) return;
        //Debug.Log($"Player level up to {level}");
        playerLevel = level;
        // note: Set data to player through DataApiController
        DataAPIController.instance.SetLevel(level, () =>
        {
            //Debug.Log($"Save level up to data {level}");

        });
    }
    private void OnEnable()
    {
        onBombEvent.AddListener(BomItem);
        onMagnetEvent.AddListener(MagnetItem);

    }
    private void OnDisable()
    {

        onBombEvent.RemoveListener(BomItem);
        onMagnetEvent.RemoveListener(MagnetItem);
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
        Debug.LogError("CardType" + CurrentCardType);
        // Small initial delay if necessary
        GetPlayerLevel();
        yield return new WaitForSeconds(0.5f);
        // Enable IngameUI and wait for it to become active
        IngameUI.SetActive(true);
        yield return new WaitUntil(() => IngameUI.activeInHierarchy);

        // Enable and initialize the slot camera
        slotCam.gameObject.SetActive(true);
        slotCam.Init();
        yield return new WaitWhile(() => slotCam.IsInitDone == false);
        // Enable and initialize the slot camera


        // Instantiate the player
        player = Instantiate(Resources.Load<Player>("Prefabs/Player"), transform);
        yield return new WaitUntil(() => player != null);

        bool isInitDone = false;
        slotData = DataAPIController.instance.AllSlotDataInDict(CurrentCardType);

        yield return new WaitUntil(() => slotData != null);
        InitCardSlot(() =>
        {
            GameManager.instance.GetCardListColorFormData(CurrentCardType);
            Player.Instance.isAnimPlaying = false;
            isInitDone = true;
            //dealerParent.gameObject.SetActive(isInitDone);
        });
        // Instantiate and initialize the dealer parent
        dealerParent = Instantiate(Resources.Load<DealerParent>("Prefabs/DealerParent"), transform);
        yield return new WaitUntil(() => dealerParent != null);
        dealerParent.Init();

        // Load current experience and card type
        exp_Current = DataAPIController.instance.GetCurrentExp();

        // Load and update background
        bg.sprite = SpriteLibControl.Instance.LoadBGSprite(CurrentCardType);
        UpdateBG(SlotCamera.Instance);

        // Initialize card slots
        foreach (var slot in GetListSlotActive())
        {
            slot.SetCollideActive(true);
        }

        yield return new WaitUntil(() => isInitDone);

        // Callback when initialization is done
        callback?.Invoke();
    }

    protected internal void InitCardSlot(Action callback)
    {
        StartCoroutine(InitCardSlotCoroutine(slotData, callback));
    }

    private IEnumerator InitCardSlotCoroutine(List<SlotData> data, Action callback)
    {
        //Debug.Log("Init Card Slot");
        var all = ConfigFileManager.Instance.SlotConfig.GetAllRecord();
        int row = 0;
        for (int i = 4; i < all.Count; i++)
        {
            var slotRecord = all[i];
            Slot newSlot = SlotPool.Instance.pool.SpawnNonGravity();
            SlotData sData = data[i];
            newSlot.ID = slotRecord.ID;
            newSlot.FibIndex = slotRecord.FibIndex;
            newSlot.transform.position = slotRecord.Pos;
            /*if (data != null)*/
            newSlot.status = sData.status;
            newSlot.SetSprite();

            if (slotRecord != null && sData.status != SlotStatus.Active)
            {
                newSlot.SetSlotPrice(slotRecord.ID, slotRecord.Price, slotRecord.Currency);
            }

            newSlot.EnableWhenInCamera();
            newSlot.SetCollideActive(true);
            newSlot.UpdateSlotState();
            newSlot.IsOnMagnet = false; 
            if (i % 7 == 0) row++;
            _slot.Add(newSlot);
            if (GameManager.instance.IsNewPlayer)
            {
                Debug.Log("InitCardSlotCoroutine load card data");
                Stack<CardColorPallet> stackColorData = new();
                if (newSlot.ID == 4 || newSlot.ID == 5)
                {
                    stackColorData = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow
                            });
                    Debug.Log("InitCardSlotCoroutine load card data");
                    newSlot.LoadCardData(stackColorData);

                }
                else if (newSlot.ID == 6)
                {
                    stackColorData = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red
                            });
                    Debug.Log("InitCardSlotCoroutine load card data");
                    newSlot.LoadCardData(stackColorData);
                }
            }
            else
            {
                newSlot.LoadCardData(sData.stack);
            }
            if (i == all.Count - 1)
            {
                callback?.Invoke();
            }
            //if (newSlot.BoxCol.enabled) Debug.Log($"newslot {newSlot.ID}");
            yield return null; // Yield control back to the main thread
        }

        _slotSorted = new List<Slot>(_slot);
        _slotSorted.Sort((slot1, slot2) => slot1.FibIndex.CompareTo(slot2.FibIndex));
    }

    public List<Slot> GetNeighbors(Slot slot)
    {
        var neighbors = new List<Slot>();

        foreach (var s in _slot)
        {
            if (s.CheckNeighborSlot(slot))
            {
                neighbors.Add(s);
            }
        }
        return neighbors;
    }
    public void SwitchNearbyCanUnlock(Slot slot)
    {
        //Debug.Log("SET NEARBY CAN UNLOCK" + slot.ID);
        var neighbors = GetNeighbors(slot);
        //Debug.Log($"NEIGHBOR COUNT {neighbors.Count}");
        foreach (var neighbor in neighbors)
        {

            UpdateNearbyNeigbor(neighbor);
        }
    }
    public void SwitchNearbyInActive(Slot slot)
    {
        var neighbors = GetNeighbors(slot);
        foreach (var neighbor in neighbors)
        {
            neighbor.gameObject.SetActive(slot.status != SlotStatus.InActive);
        }
    }
    public void UpdateNearbyNeigbor(Slot nei)
    {
        if (nei.status == SlotStatus.Active) return;
        int ID = nei.ID;
        if (nei.gameObject.activeSelf) nei.gameObject.SetActive(true);
        if (nei.status == SlotStatus.InActive || nei.status == SlotStatus.Locked)
        {
            //Debug.Log("nei ID" + ID);
            if (nei.gameObject.activeSelf == false) nei.gameObject.SetActive(true);
            var slotconfig = ConfigFileManager.Instance.SlotConfig.GetRecordByKeySearch(ID);
            nei.status = SlotStatus.Locked;
            nei.SetSlotPrice(ID, slotconfig.Price, slotconfig.Currency);
            nei.UpdateSlotConfig();
            nei.EnableWhenInCamera();
            nei.SetSprite();
            nei.SettingBuyBtn(true);
            nei.ReloadSlotButton();
        }

    }
    private Slot RandomOneActiveSlot()
    {
        var activeSlots = GetListSlotActive();
        List<Slot> activeAndHaveCardSlots = new();
        foreach (Slot s in activeSlots)
        {
            //Debug.Log(s.ID);
            if (s._cards.Any())
            {
                activeAndHaveCardSlots.Add(s);
            }
        }
        if (!activeAndHaveCardSlots.Any()) return null;
        int randomIndex = UnityEngine.Random.Range(0, activeAndHaveCardSlots.Count - 1);
        Slot randomSlot = activeAndHaveCardSlots[randomIndex];
        //Debug.Log("Random Slot " + randomIndex + " slotindext " + randomSlot.ID);
        return randomSlot;
    }
    void ClearOneSlotOnBomb(Action callback)
    {
        Slot slotBomb = RandomOneActiveSlot();
        if (slotBomb is null) return;
        //TODO: play bom animation
        var listCardInslot = slotBomb._cards;
        slotBomb.SplashCardOnBomb(() =>
        {
            //Debug.Log("SplashCardONBOMB DONE");
            slotBomb.UpdateSlotState();
            int total = DataAPIController.instance.GetItemTotal(ItemType.Bomb);
            total -= 1;
            DataAPIController.instance.SetItemTotal(ItemType.Bomb, total);
            callback?.Invoke();
        });

    }
    public void BomItem(bool isOnBomb)
    {
        //Debug.Log("ON BOMB ITEM EVENT");
        if (!isOnBomb) return;
        else
        {
            //Debug.Log("ON BOMB ITEM EVENT TRUE");
            ClearOneSlotOnBomb(() =>
            {
                var view = ViewManager.Instance.currentView as GamePlayView;
                view.Bomb_Btn.interactable = true;
            });
        }
    }
    public void MagnetCardToDealer(Action<bool> callback)
    {
        bool isDone = false;
        if(Player.Instance.fromSlot != null)
        {
            Player.Instance.fromSlot.MoveSelectedCardsBack();
            Player.Instance.fromSlot = null;
        }
        var activeSlot = GetListSlotActive(); // Assume this method is defined elsewhere and returns a list of Slot objects.
        List<Card> listCard = new();
        List<Dealer> activeDealers = dealerParent.ActiveDealers(); // Assume this method is defined elsewhere and returns a list of active Dealer objects.
        int animationCount = 0; // Counter to track the number of ongoing animations

        // Gather all cards from active slots
        foreach (Slot s in activeSlot)
        {
            if (s._cards.Count > 0)
            {
                listCard.AddRange(s._cards); // Correctly add cards from slot s to listCard.
            }
        }

        // Check if all dealers have cards and there are no matching colors
        bool allDealersHaveCards = true;
        bool noMatchingCards = true;
        Player.Instance.isAnimPlaying = true;

        foreach (Dealer dealer in activeDealers)
        {
            var dealerTopColor = dealer.dealSlot.TopColor();
            if (dealerTopColor == CardColorPallet.Empty)
            {
                allDealersHaveCards = false;
                break;
            }

            if (listCard.Any(card => card.cardColor == dealerTopColor))
            {
                noMatchingCards = false;
                break;
            }
        }

        // If all dealers have cards and no matching cards exist, invoke the callback with false and return
        if (allDealersHaveCards && noMatchingCards)
        {
            callback?.Invoke(false);
            return;
        }

        // If there are no cards, invoke the callback with false and return
        if (listCard.Count == 0)
        {
            callback?.Invoke(false);
            return;
        }

        // Sort the list of cards by cardColor
        listCard.Sort((c1, c2) => c2.cardColor.CompareTo(c1.cardColor)); // Sort descending by card color

        // Group cards by color and sort by the count of each group in descending order
        var groupedCards = listCard
            .GroupBy(card => card.cardColor)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .ToList();
        int total = DataAPIController.instance.GetItemTotal(ItemType.Magnet) - 1;
        DataAPIController.instance.SetItemTotal(ItemType.Magnet, total);
        foreach (Dealer dealer in activeDealers)
        {
            IGrouping<CardColorPallet, Card> selectedGroup = null;
            dealer.dealSlot.IsOnMagnet = true;

            if (dealer.dealSlot.TopColor() == CardColorPallet.Empty)
            {
                selectedGroup = groupedCards.FirstOrDefault();
            }
            else
            {
                // Find the group that matches the top color
                selectedGroup = groupedCards
                    .FirstOrDefault(g => g.Key == dealer.dealSlot.TopColor());
            }

            // Get the list of the selected group's cards
            var selectedCards = selectedGroup?.ToList();

            // Check if there are any selected cards
            if (selectedCards == null || selectedCards.Count == 0)
            {
                Debug.Log("No colors found for dealer.");
                continue;
            }

            // Additional logic for animations and other operations

            // Store references to slots to update them after moving the cards
            Dictionary<Slot, List<Card>> cardsToRemove = new Dictionary<Slot, List<Card>>();
            float d = Player.Instance.duration;
            float cardOffset = dealer.dealSlot.transform.position.y + 0.1f;
            float z = dealer.dealSlot._cards.Count == 0
                ? dealer.transform.position.z + 0.1f
                : dealer.dealSlot._cards.Last().transform.position.z + Player.Instance.cardPositionOffsetZ;
            float delay = 0;
            Color color = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(selectedCards[0].cardColor).Color;
            selectedCards.Reverse();
            Player.Instance.isAnimPlaying = true;
            foreach (Card c in selectedCards)
            {
                Slot slot = activeSlot.FirstOrDefault(s => s._cards.Contains(c));
                if (slot != null)
                {
                    if (!cardsToRemove.ContainsKey(slot))
                    {
                        cardsToRemove[slot] = new List<Card>();
                    }
                    cardsToRemove[slot].Add(c);
                    animationCount++; // Increment the counter for each animation
                    var t = c.PlayAnimation(dealer.dealSlot, d, Player.Instance.height,
                                             Player.Instance.ease, cardOffset, z, delay);
                    t.OnPlay(() => {
                        slot.SetColliderSize(-1);
                        slot.UpdateSlotState();
                    });

                    t.OnComplete(() =>
                    {
                        dealer.fillImg.fillAmount += 0.1f;
                        dealer.fillImg.color = color;
                        dealer.dealSlot._cards.Add(c);
                        dealer.dealSlot.FixCardsHeigh();
                        if (c == selectedCards.Last())
                        {
                            dealer.dealSlot.UpdateSlotState();
                            t.Kill();
                        }
                        animationCount--; // Decrement the counter when an animation completes
                        if (animationCount == 0 && isDone) // Check if all animations are done and if the process is marked as done
                        {
                            var view = ViewManager.Instance.currentView as GamePlayView;
                            view.Magnet_btn.interactable = true;
                            StartCoroutine(DelayedCallback(callback, true, Player.Instance.delay));
                            foreach (Slot s in activeSlot)
                            {
                                s.IsOnMagnet = true;
                                s.FixCardsHeigh();
                                s.IsOnMagnet = false;
                                s.CenterCollider();
                            }
                        }
                    });
                    cardOffset += Player.Instance.cardPositionOffsetY;
                    delay += Player.Instance.delay;
                    z += Player.Instance.cardPositionOffsetZ;
                    dealer.dealSlot.UpdateSlotState();
                    dealer.dealSlot.SetColliderSize(1);
                    slot.UpdateSlotState();
                }
            }

            // Remove the moved cards from their respective slots
            foreach (var kvp in cardsToRemove)
            {
                foreach (var card in kvp.Value)
                {
                    kvp.Key._cards.Remove(card);
                }
            }

            // Remove the selected group from the groupedCards list
            groupedCards.Remove(selectedGroup);
        }
        isDone = true; // Mark the process as done
        if (animationCount == 0) // If there are no ongoing animations, invoke the callback immediately
        {
            var view = ViewManager.Instance.currentView as GamePlayView;
            view.Magnet_btn.interactable = true;
            StartCoroutine(DelayedCallback(callback, true, Player.Instance.delay));
            
            Player.Instance.isAnimPlaying = false;
        }
    }

    private IEnumerator DelayedCallback(Action<bool> callback, bool result, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke(result);
    }

    public void MagnetItem(bool isOnMagnet)
    {
        this.isOnMagnet = false;
        if (!isOnMagnet) return;
        else
        {
            MagnetCardToDealer((isTrue) =>
            {
                if (!isTrue)
                {
                    var view = ViewManager.Instance.currentView as GamePlayView;
                    view.Magnet_btn.interactable = true;
                    Player.Instance.isAnimPlaying = false;

                }
                else
                {
                    
                    Player.Instance.isAnimPlaying = false;
                }


            });
        }
    }
    public List<Slot> GetListSlotInActive()
    {
        return SlotPool.Instance.pool.activeList.Where(slot => slot.status == SlotStatus.InActive).ToList();
    }

    public List<Slot> GetListSlotActive()
    {
        return SlotPool.Instance.pool.activeList.Where(slot => slot.status == SlotStatus.Active).ToList();
    }
    public List<Slot> GetListActiveSortByCardCount()
    {
        var _activeSlot = GetListSlotActive();
        return _activeSlot.OrderBy(slot => slot._cards.Count).ToList();
    }
    public void SaveCardListToSLots()
    {
        var actives = GetListSlotActive();
        CardType type = CurrentCardType;
        foreach (Slot s in actives)
        {
            s.SaveCardListToData(type);
        }
    }
    public void UpdateBG(SlotCamera cam)
    {
        Vector2 size = new Vector2(cam.width, cam.height);
        Vector3 pos = cam.transform.position;
        //Debug.Log($"UPDATEBG + size");
        bg.transform.position = pos + new Vector3(0, 0, 10f);
        bg.size = size;
    }
    public void OnQuitIngame()
    {
        if (ViewManager.Instance.currentView.viewIndex != ViewIndex.GamePlayView) return;
        if (!GameManager.instance.IsNewPlayer)
        {
            SaveCardListToSLots();
            dealerParent.SaveDataDealer(CurrentCardType);
        }

        foreach (Slot slot in _slot)
        {
            slot.SettingBuyBtn(false);
        }
        _slotSorted.Clear();
        Destroy(player.gameObject);
        Destroy(dealerParent.gameObject);
    }
    public void SetCurrentCardType(CardType type)
    {
        _currentCardType = type;
    }
    private void UpdateBG(SpriteRenderer BG)
    {
        float width = SlotCamera.Instance.width;
        float heigh = SlotCamera.Instance.height;
        BG.size = new Vector2(width, heigh);
    }
    public bool IsSortedSlotIsEmty()
    {
        return _slotSorted.Count == 0;
    }
    public Slot TakeSlotByIndex(int index)
    {
        return _slotSorted[index] ?? null;
    }
    public void AllSlotCheckCamera()
    {
        for (int i = 0; i < _slot.Count; i++)
        {
            //Debug.Log($"Slot {i} enable incamera");
            _slot[i].EnableWhenInCamera();
        }
    }
    private void OnApplicationQuit()
    {
        if (!GameManager.instance.IsNewPlayer) OnQuitIngame();
    }
}
