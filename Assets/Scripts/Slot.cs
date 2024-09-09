using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Slot : MonoBehaviour, IComparable<Slot>
{
    public List<Card> _cards; //Cards current in slot
    public SlotStatus status;

    private const float sizePerCard = 0.075f;
    [SerializeField] private bool isDealer;
    [SerializeField] private bool isEmpty;
    [SerializeField] private bool isDealBtnTarget;
    [SerializeField] private bool isOnMagnet;

    [SerializeField] private int id;
    [SerializeField] private int fibIndex;
    [SerializeField] private int unlockCost;
    [SerializeField] private float cardOffset;
    private static readonly int sCounter = 10;

    [SerializeField] private CardColorPallet _topCardColor;
    //[SerializeField] private List<CardColorPallet> cardColorPallets;

    [SerializeField] private BoxCollider boxCol;
    [SerializeField] private Stack<Card> _selectedCard;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private RectTransform buyBtnRect;
    [SerializeField] private SlotBtn buyBtn;
    [SerializeField] private Transform anchor;
    [SerializeField] private Currency buyType;
    [SerializeField] private Vector3 scaleValue;

    [SerializeField] public UnityEvent<bool> onToucheHandle = new();
    public int ID { get => id; set => id = value; }
    public float X { get => transform.position.x; }
    public float Y { get => transform.position.y; }
    public float Z { get => transform.position.z; }
    public Vector3 Pos { get { return new Vector3(X, Y, Z); } }

    public int FibIndex { get => fibIndex; set => fibIndex = value; }
    public RectTransform BuyBtn { get => buyBtnRect; set => buyBtnRect = value; }
    //public List<CardColorPallet> CardColorPallets { get => cardColorPallets; set => cardColorPallets = value; }
    public float CardOffset { get => cardOffset; set => cardOffset = value; }
    public int UnlockCost { get => unlockCost; set => unlockCost = value; }
    public BoxCollider BoxCol { get => boxCol; set => boxCol = value; }
    public bool IsOnMagnet { get => isOnMagnet; set => isOnMagnet = value; }
    #region Dealer
    [SerializeField] private Dealer dealer;

    [SerializeField] private int slotLevel;
    [SerializeField] private int exp = 1;
    private static int _cardCounter;
    #endregion

    #region UnityAction
    [HideInInspector] public UnityEvent<float> expChanged;
    [HideInInspector] public UnityEvent<int> goldCollected = new();
    [HideInInspector] public UnityEvent<int> gemCollected = new();
    [HideInInspector] public UnityEvent<int> slotCanUnlock = new();
    [HideInInspector] public UnityEvent<bool> slotBtnClicked = new();
    [HideInInspector] public UnityEvent<bool> slotUnlocked = new();
    [HideInInspector] public UnityEvent<bool> onScalingCamera = new();



    //[HideInInspector] public UnityEvent<>
    #endregion

    public Vector3 GetPos()
    {
        return transform.position;
    }
    private void OnEnable()
    {
        buyBtn.slotBtnClicked = new();
        buyBtn.slotBtnClicked.AddListener(IsSlotUnlocking);
        slotUnlocked = new();
        if (!isDealer) onToucheHandle.AddListener(TapHandler);
        slotUnlocked.AddListener(SlotUnlocked);
        onScalingCamera.AddListener(HandleCameraScaling);
    }

    private void OnDisable()
    {
        slotUnlocked.RemoveListener(SlotUnlocked);
        if (!isDealer) onToucheHandle.RemoveListener(TapHandler);
        onScalingCamera.RemoveListener(HandleCameraScaling);
        _cards.Clear();
        if (!isDealer) Reset();
    }
    private void Awake()
    {
        buyBtn.SetSlotParent(this);
    }
    public virtual void Start()
    {
        Init();
        InvokeRepeating(nameof(SlotUpdating), 0.1f, 0.1f);
    }

    public void Init()
    {
        boxCol = GetComponentInChildren<BoxCollider>();
        buyBtnRect.GetComponent<SlotBtn>().ParentAnchor = anchor;
        _selectedCard = new();
        isEmpty = true;
        if (status != SlotStatus.Active) boxCol.enabled = false;
        if (isDealer)
        {
            dealer = transform.parent.GetComponent<Dealer>();
            BuyBtn.gameObject.SetActive(false);
        }
        if (_cards.Count == 0) return;
        CenterCollider();
        isEmpty = false;
        _topCardColor = _cards.Last().cardColor;
        if (_topCardColor != CardColorPallet.Empty && !GameManager.instance.listCurrentCardColor.Contains(_topCardColor))
        {
            GameManager.instance.listCurrentCardColor.Add(_topCardColor);
        }
    }
    public void SetSprite()
    {
        switch (status)
        {
            case SlotStatus.Active:

                spriteRenderer.sprite = SpriteLibControl.Instance.GetSpriteByName(status.ToString());
                break;
            case SlotStatus.InActive:
                SettingBuyBtn(false);
                spriteRenderer.sprite = SpriteLibControl.Instance.GetSpriteByName(status.ToString());
                break;
            case SlotStatus.Locked:
                SettingBuyBtn(true);
                spriteRenderer.sprite = SpriteLibControl.Instance.GetSpriteByName(status.ToString());
                break;
            default: break;
        }
    }
    public void SettingBuyBtn(bool isEnable)
    {
        //int count = SlotCamera.Instance.mulCount;
        //buyBtnRect.DOScale(new Vector3(scale, scale, scale), 0);
        buyBtnRect.gameObject.SetActive(isEnable);
        ScreenToWorld.Instance.SetWorldToCanvas(buyBtnRect);
        SwitchBtnType(isEnable, buyType);
    }
    internal void SwitchBtnType(bool isActive, Currency currencyType)
    {
        buyBtn.SetBtnType(isActive, currencyType);
    }
    public void UpdateCardPositionX(Vector3 pos)
    {
        List<Card> temp = new(_cards);
        int c = _cards.Count();
        float offset = pos.x;
        //Debug.Log("start update CARD POS " + pos);

        for (int i = 0; i < c; i++)
        {
            Card tCard = temp[i];
            //Debug.Log("UPDATE CARD POS " + pos);
            tCard.transform.DOMoveX(offset, 0.2f);
        }
    }
    public void UpdateCardPositionY()
    {

        List<Card> temp = new(_cards);
        int c = _cards.Count();
        float offset = transform.position.y + Player.Instance.cardPositionOffsetY;
        IsOnMagnet = true;
        for (int i = 0; i < c; i++)
        {
            Card tCard = temp[i];
            tCard.transform.DOMoveY(offset, 0.2f);
            offset += Player.Instance.cardPositionOffsetY;

        }
    }
    internal void SetTargetToDealCard(bool b)
    {
        //Debug.LogWarning("set target to deal card");
        isDealBtnTarget = b;
    }
    public void SlotUpdating()
    {

        bool isEmptyNow = _cards.Count == 0;
        if (status != SlotStatus.Active) return;
        if (isEmpty != isEmptyNow)
        {
            isEmpty = isEmptyNow;
            _topCardColor = isEmpty ? CardColorPallet.Empty : (_cards.Count > 0 ? _cards.Last().cardColor : CardColorPallet.Empty);
        }
    }
    internal void ReloadSlotButton()
    {
        //Debug.Log("Btn active" + buyBtn.gameObject.activeInHierarchy + "id " + id);
        ScreenToWorld.Instance.SetWorldToCanvas(buyBtnRect);
        buyBtnRect.transform.DOMove(anchor.position, 0.075f);
        float a = SlotCamera.Instance.ScaleValue[SlotCamera.Instance.mulCount];
        scaleValue = new Vector3(a, a, a);
        //tween = buyBtnRect.DOScale(scaleValue, SlotCamera.Instance.Mul_Time);
        //tween.OnComplete(() => tween.Kill());
    }
    public CardColorPallet TopColor()
    {
        return _topCardColor;
    }

    internal void LoadCardData<T>(Stack<T> stackCardColor)
    {
        if (stackCardColor is null || stackCardColor.Count == 0)
        {
            if (isDealer) Debug.LogWarning($"Stack card is null ");
            if (gameObject.activeInHierarchy) UpdateSlotState();
            return;
        }
        StartCoroutine(LoadCardDataCoroutine(stackCardColor));

        //else
        //{
        //    StartCoroutine(LoadCardDataCoroutine(stackCardColor));
        //}
    }

    private IEnumerator LoadCardDataCoroutine<T>(Stack<T> stackCardColor)
    {
        float delay = 0;
        float d = Player.Instance.duration;
        float offset = transform.position.y + 0.1f;
        float z = Player.Instance.cardPositionOffsetZ;
        CardType currentCardType = IngameController.instance.CurrentCardType;
        var colorConfig = ConfigFileManager.Instance.ColorConfig;
        //if (isDealer) Debug.LogWarning($"Load card data dealer {stackCardColor.Count}");
        while (stackCardColor.Count > 0)
        {
            isDealBtnTarget = true;
            Player.Instance.isAnimPlaying = true;
            T spawnColor = stackCardColor.Pop();
            ColorConfigRecord colorRecord = colorConfig.GetRecordByKeySearch(spawnColor);

            Card c = CardPool.Instance.pool.SpawnNonGravity();
            c.ColorSetBy(colorRecord.Name, currentCardType, colorRecord.Color);
            Vector3 worldPoint = new(0, -5, -10);
            c.transform.SetLocalPositionAndRotation(worldPoint, Quaternion.identity);
            c.PlayAnimation(this, d, Player.Instance.height, Player.Instance.ease, offset, z, delay);
            _cards.Add(c);
            //CardColorPallets.Add(c.cardColor);
            delay += 0.075f;
            offset += Player.Instance.cardPositionOffsetY;
            z += Player.Instance.cardPositionOffsetZ;

            // Update collision size;
            SetColliderSize(1);
            _topCardColor = _cards.Last().cardColor;
            yield return new WaitForSeconds(0.075f); // Wait for 0.075 seconds before spawning the next card
            if (stackCardColor.Count < 1)
            {
                Player.Instance.isAnimPlaying = false;
                StartCoroutine(UpdateSlotType(delay + d + 0.4f));
                if (isDealer)
                {

                    dealer.fillImg.color = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(this.TopColor()).Color;
                    dealer.fillImg.fillAmount = this._cards.Count * 0.1f;
                }
            }
            //FixCardsHeigh();
        }
        yield return new WaitUntil(() => stackCardColor.Count < 0);
        StartCoroutine(UpdateSlotType(delay + d + 0.4f));
        if(isDealer) DataAPIController.instance.SaveStackCard(this.id, IngameController.instance.CurrentCardType, null);
    }

    IEnumerator UpdateSlotType(float v)
    {
        yield return new WaitForSeconds(v);
        //yield return new WaitUntil(()=> _cards.Count)
        UpdateSlotState();
    }
    public void TapHandler(bool onTap)
    {

        //Debug.Log($"ontap handler {isDealer} ");
        if (status != SlotStatus.Active || Player.Instance.isAnimPlaying) return;
        if (Player.Instance.fromSlot == null && !isEmpty)
        {
            Player.Instance.fromSlot = this;
            Player.Instance.toSlot = null;
            FindSameColorCards();
        }
        else if (Player.Instance.fromSlot != null && Player.Instance.fromSlot != this)
        {
            Slot toSlot = Player.Instance.toSlot = this;
            if (_topCardColor != Player.Instance.fromSlot._selectedCard.Peek().cardColor
                && _topCardColor != CardColorPallet.Empty && _cards.Count != 0)
            {
                //IF SELECTED CARD PEEKD NOT SAME COLOR AS TO SLOT TOP CARD
                MoveSelectedCardsBack();
                SoundManager.instance.PlaySFX(SoundManager.SFX.MoveWrong);
                Player.Instance.fromSlot = null;
                Player.Instance.toSlot = null;
                return;
            }
            foreach (var c in Player.Instance.fromSlot._selectedCard)
            {
                Player.Instance.fromSlot._cards.Remove(c);
            }
            boxCol.enabled = false;
            float d = Player.Instance.duration;
            float count = Player.Instance.fromSlot._selectedCard.Count();
            CardOffset = _cards.Count == 0 ? toSlot.GetPos().y + 0.1f : _cards.Last().transform.position.y + Player.Instance.cardPositionOffsetY;
            Player.Instance.isAnimPlaying = false;

            if (isDealer)
            {
                dealer.fillImg.color = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(Player.Instance.fromSlot._selectedCard.Peek().cardColor).Color;
            }
            float delay = 0;
            float z = _cards.Count == 0 ? toSlot.GetPos().z + 0.025f : _cards.Last().transform.position.z + Player.Instance.cardPositionOffsetZ; ;

            // Sent card to slot
            int completedAnimations = 0;
            var fromSlot=  Player.Instance.fromSlot;
            for (int i = 0; i < count; i++)
            {
                if (isDealer) dealer.SetRewardActive(false);
                Card lastCard = Player.Instance.fromSlot._selectedCard.Pop();
                Player.Instance.fromSlot._cards.Remove(lastCard);
                lastCard.PlayAnimation(this, d, Player.Instance.height,
                        Player.Instance.ease, CardOffset, z, delay)
                    .OnComplete(() =>
                    {
                        completedAnimations++;
                        if (isDealer)
                        {
                            dealer.fillImg.fillAmount += 0.1f;
                        }
                        if (completedAnimations == count)
                        {
                            // All animations are complete
                            Player.Instance.isAnimPlaying = true;
                            OnAllAnimationsComplete(fromSlot);
                            //OnAllAnimationsComplete(Player.Instance.toSlot);
                        }
                    });
                _cards.Add(lastCard);
                delay += Player.Instance.delay;
                CardOffset += Player.Instance.cardPositionOffsetY;
                z += Player.Instance.cardPositionOffsetZ;
                //IsOnMagnet = true;
                SetColliderSize(1);
                Player.Instance.fromSlot.SetColliderSize(-1);
            }
            Player.Instance.fromSlot = null;

            Player.Instance.toSlot = null;

            //isDealBtnTarget = true;
            Invoke(nameof(UpdateSlotState), d + delay);
        }
        else if (Player.Instance.fromSlot is not null && Player.Instance.fromSlot == this)
        {
            MoveSelectedCardsBack();
            Player.Instance.fromSlot = null;
            Player.Instance.toSlot = null;
            Player.Instance.isAnimPlaying = false;
            return;

        }
       
    }
    void OnAllAnimationsComplete(Slot slot )
    {
        slot.FixCardsHeigh();
    }
    // Define this method to handle what happens when all animations are done

    void FindSameColorCards()
    {
        List<Vector3> lastPos = new();

        int c = _cards.Count();
        List<Card> temp = new(_cards);
        Stack<Card> temStackSelected = new();

        temp.Reverse();
        bool changed = false;
        for (int i = 0; i < c; i++)
        {
            if (temp[i].cardColor == _topCardColor)
            {
                Card tCard = temp[i];
                temStackSelected.Push(tCard);
                tCard.transform.DOMoveY(tCard.transform.position.y + 0.1f, 0.2f);
                lastPos.Add(tCard.transform.position);
            }
            else
            {
                _topCardColor = temp[i].cardColor;
                changed = true;
                break;
            }
        }
        int stackCount = temStackSelected.Count;
        for (int i = 0; i < stackCount; i++)
        {
            _selectedCard.Push(temStackSelected.Pop());
        }
        if (!changed)
        {
            _topCardColor = CardColorPallet.Empty;
        }
    }

    public void MoveSelectedCardsBack()
    {
        foreach (var c in Player.Instance.fromSlot._selectedCard)
        {
                float y = c.transform.position.y;
                c.transform.DOMoveY(y - 0.1f, 0.2f);
        }
        Player.Instance.fromSlot._selectedCard.Clear();
        Player.Instance.fromSlot.UpdateSlotState();
        Player.Instance.fromSlot.CenterCollider();
    }

    public void UpdateSlotState()
    {
        if (status != SlotStatus.Active) return;
        // Enable box collider
        boxCol.enabled = true;
        // Update top card color
        var lastCard = _cards.LastOrDefault();
        _topCardColor = lastCard?.cardColor ?? CardColorPallet.Empty;

        // Handle deal button target
        if (isDealBtnTarget)
        {
            Debug.LogError(" update slot state is deal button target");
            int excessCards = _cards.Count - 20;
            if (excessCards >=0)
            {
                for (int i = 0; i < excessCards; i++)
                {
                    CardPool.Instance.pool.DeSpawnNonGravity(_cards[i]);
                    SetColliderSize(-1);
                }
                _cards.RemoveRange(0, excessCards);
                float whY = transform.position.y;
                foreach (var card in _cards)
                {
                    //Debug.LogWarning("Execcing card" + id + isOnMagnet);
                    card.transform.DOMoveY(whY, 0.05f);
                    whY += 0.05f;
                }
                CenterCollider();
            }
            isDealBtnTarget = false;
            //Debug.LogWarning("Is deal button target true");
            Player.Instance.isAnimPlaying = false;
            Player.Instance.isDealBtnActive = false;

        }
        if (!isDealer || !gameObject.activeSelf) return;
        DealerStateCoroutine();
    }

    //s?a ?? cao c?a card
    public void FixCardsHeigh()
    {

        int excessCards = _cards.Count - 20;
        Debug.Log("Execcing card height" + excessCards);
        if (excessCards > 0 )
        {
            float whY = transform.position.y + Player.Instance.cardPositionOffsetY;
            foreach (var card in _cards)
            {
                card.transform.DOMoveY(whY, 0.05f);
                whY += 0.05f;
            }
        }
        else
        {
            UpdateCardPositionY();
        }
        Player.Instance.isAnimPlaying = false;
    }
    public void DealerStateCoroutine()
    {
        List<Card> clearList = _cards;
        // Update deal table
        int cardCount = _cards.Count;
        _cardCounter = cardCount;
        if (_cards.Count < sCounter) return;
        boxCol.enabled = false;
        Player.Instance.totalGold += dealer.RewardGold;
        Player.Instance.totalGem += dealer.RewardGem;

        float delay = 0.05f;
        float totalDelay = delay * cardCount;
        dealer.isPlashCard = true;
        for (int i = 0; i < cardCount; i++)
        {
            Invoke(nameof(SplashAndDisableCard), delay);
            delay += Player.Instance.timeDisableCard;
            exp++;
        }
        goldCollected.Invoke(dealer.RewardGold);
        gemCollected.Invoke(dealer.RewardGem);
        InvokeAfterDelay(() => AfterSplasingCard(cardCount), delay);
    }
    void InvokeAfterDelay(Action action, float delay)
    {
        StartCoroutine(InvokeAfterDelayCoroutine(action, delay));
    }
    IEnumerator InvokeAfterDelayCoroutine(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
    public void AfterSplasingCard(int cardCount)
    {


        expChanged.Invoke(cardCount);
        boxCol.enabled = true;
        dealer.isPlashCard = false;
        isOnMagnet = false;
        Player.Instance.isAnimPlaying = false;
    }
    public void SplashCardOnBomb(Action callback)
    {
        float t = 0.05f;
        int count = _cards.Count;
        int invokeCount = 0; // Counter to track the number of invokes

        for (int i = 0; i < count; i++)
        {
            if (i == count - 1)
            {
                Invoke(nameof(SplashAndDisableCardOnBomb), t);
                invokeCount++; // Increment the counter for each invoke
                               // Use a coroutine to wait and invoke the callback after all invokes are done
                StartCoroutine(InvokeCallbackAfterDelay(callback, t + Player.Instance.timeDisableCard));
            }
            else
            {
                Invoke(nameof(SplashAndDisableCardOnBomb), t);
                invokeCount++; // Increment the counter for each invoke
            }
            t += Player.Instance.timeDisableCard;
        }
    }
    public bool CheckSlotIsInCamera()
    {
        //SlotCamera cam = SlotCamera.Instance;
        //cam.GetCamera();
        //Debug.Log($"postion {transform.position} + left {CameraMain.instance.GetLeft()} " +
        //    $" + right {CameraMain.instance.GetRight()} + top {CameraMain.instance.GetTop()} + bot {CameraMain.instance.GetBottom()}");
        if (transform.position.x < SlotCamera.Instance.GetLeft() - 1
            || transform.position.x > SlotCamera.Instance.GetRight() + 1
                || transform.position.y > SlotCamera.Instance.GetTop() - 2f
                   || transform.position.y < SlotCamera.Instance.GetBottom() + 3f) return false;
        else return true;
    }
    public void EnableWhenInCamera()
    {
        if (CheckSlotIsInCamera()) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }
    public void SetCollideActive(bool active)
    {
        if (status == SlotStatus.Active && active) boxCol.enabled = true;
        else boxCol.enabled = false;
    }
    private IEnumerator InvokeCallbackAfterDelay(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
    private void SplashAndDisableCardOnBomb()
    {
        if (_cards.Count <= 0) return;
        Card last = _cards.Last();
        if (_cards.Remove(last))
        {
            last.gameObject.SetActive(false);
            SetColliderSize(-1);
            CenterCollider();
        }
    }
    private void SplashAndDisableCard()
    {
        //Debug.LogWarning("SplashAndDisableCard");
        if (_cards.Count <= 0) return;
        Card last = _cards.Last();
        if (last == null)
        {
            return;
        }

        if (_cards.Remove(last))
        {
            SplashVfx s = VFXPool.Instance.pool.SpawnNonGravity();
            s.SetPositionAndRotation(transform.position, Quaternion.identity);
            s.SetColorVFX(last.currentColor);
            s.PlayAndDeactivate();
            CardPool.Instance.pool.DeSpawnNonGravity(last);
            dealer.fillImg.fillAmount -= 0.1f;
            SetColliderSize(-1);
            CenterCollider();
            SoundManager.instance.PlaySFX(SoundManager.SFX.SPLASHCARD);
        }
    }

    public void SetColliderSize(float sign)
    {
        boxCol.size += new Vector3(0, 0, sign * sizePerCard);
        if (!(boxCol.size.z < 0)) return;
        var size = boxCol.size;
        size = new Vector3(size.x, size.y, 0);
        boxCol.size = size;
    }

    public void CenterCollider()
    {
        var center = boxCol.center;
        boxCol.center = new Vector3(center.x, center.y, -boxCol.size.z / 2);
        if (!(boxCol.center.z < 0)) return;
        var size = boxCol.center;
        size = new Vector3(size.x, size.y, 0);
        boxCol.center = size;
    }

    private void SlotUnlocked(bool isUnlocked)
    {
        if (isUnlocked)
        {
            SoundManager.instance.PlaySFX(SoundManager.SFX.UnlockSlotSFX);
            status = SlotStatus.Active;
            gameObject.SetActive(true);
            buyBtnRect.gameObject.SetActive(false);
            boxCol.enabled = true;
            UpdateSlotStatus(status);
            SetSprite();
            UpdateSlotData();
            //UpdateSlotConfig();
            transform.DORotate(new Vector3(0, 180, 0), 0.5f, RotateMode.FastBeyond360);
            if (isDealer)
            {
                dealer.SetUpgradeButtonActive(true);
                var data = DataAPIController.instance.GetDealerData(dealer.Id);
                dealer.Status = status = data.status = SlotStatus.Active;
                Init();
                dealer.Init();
                dealer.SetRender();
                dealer.SetRewardActive(true);
                dealer.SetFillActive(true);
                dealer.SetDealerLvelActive(true);

                dealer.UpdateFillPostion();

                gameObject.SetActive(true);
                //dealer.gold_reward.enabled = false;

                DataAPIController.instance.SetDealerToDictByID(dealer.Id, data, null);
            }
            UpdateSlotState();
        }
        else return;
    }
    public void UpdateSlotData()
    {
        SlotData data = new();
        data.status = SlotStatus.Active;
        CardType type = IngameController.instance.CurrentCardType;
        DataAPIController.instance.SaveSlotData(id, data, type, (isDone) =>
        {
            if (!isDone) return;
            if (IsBackBoneSlot())
            {
                //Debug.Log("Post new SLot data this " + Y);

                Vector3 camPos = SlotCamera.Instance.GetCam().transform.position;
                SlotCamera.Instance.targetPoint = new Vector3(0, camPos.y + 1.5f, camPos.z);
                IngameController.instance.SwitchNearbyCanUnlock(this);
                SlotCamera.Instance.onScalingCamera.Invoke(true);
            }
            else
            {
                IngameController.instance.SwitchNearbyCanUnlock(this);
            }
        });
    }
    public bool IsBackBoneSlot()
    {
        Vector3 temp = transform.position;
        if (temp.x == 0 && !isDealer && fibIndex < 8)
        {
            return true;
        }
        return false;
    }
    internal void UpdateSlotConfig()
    {
        var configrecord = ConfigFileManager.Instance.SlotConfig.GetRecordByKeySearch(id);
        if (configrecord != null)
        {
            //configrecord.Status = status;
            SlotData newData = new();
            newData.status = status;
            newData.stack = new();
            CardType type = IngameController.instance.CurrentCardType;
            DataAPIController.instance.SaveSlotData(id, newData, type, (isDone) =>
            {
                //if (isDone) //Debug.Log("Add new slot data");
                //else Debug.Log("Save DataFail");
            });

        }
    }
    private void IsSlotUnlocking(bool isUnlocking)
    {
        if (!isUnlocking)
        {
            //Debug.Log($"IS UNLOCKING {isUnlocking.ToString().ToUpper()}");
            return;
        };

        int gold = DataAPIController.instance.GetGold();
        int gem = DataAPIController.instance.GetGem();

        if (buyType == Currency.Gold && unlockCost <= gold)
        {
            DataAPIController.instance.MinusGoldWallet(unlockCost, (isDone) =>
            {
                //Debug.Log("MINUS GOLD DONE");
                if (isDone) slotUnlocked.Invoke(isDone);
            });
        }
        else if (buyType == Currency.Gem && unlockCost <= gem)
        {
            //TODO: change minus gold -> minus gem
            DataAPIController.instance.MinusGemWallet(unlockCost, (isDone) =>
            {
                //Debug.Log("MINUS GEM  DONE");
                slotUnlocked.Invoke(isDone);
                return;
            });

        }

    }
    private void HandleCameraScaling(bool isScaling)
    {
        ReloadSlotButton();
    }
    public void SetSlotPrice(int id, int cost, Currency type)
    {
        if (this.id != id) return;
        //Debug.Log($"Set Slot Price {id} cost {cost}");
        unlockCost = cost;
        buyType = type;
        if (status == SlotStatus.Locked)
        {
            buyBtn.InitButton(unlockCost, buyType);
        }
    }

    void FillAnim(float f)
    {
        if (f <= 0) return;
    }
    public Stack<Card> GetSelectedCards()
    {
        return _selectedCard;
    }
    bool CheckDataEmty()
    {
        //foreach slot have in data, check slot List<Card>
        return true;
    }
    public void UpdateSlotStatus(SlotStatus status)
    {
        this.status = status;
    }
    public bool CheckNeighborSlot(Slot checkSlot)
    {
        if (Pos.y == checkSlot.Y && (Pos.x + 2 == checkSlot.X || Pos.x - 2 == checkSlot.X))
        {
            return true;
        }
        else if ((Pos.y - 3 == checkSlot.Y || Pos.y + 3 == checkSlot.Y) && Pos.x == checkSlot.Pos.x)
        {
            //Debug.Log("Pos.y + 3 == checkSlot.Y && Pos.x == checkSlot.Pos.x true");
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SaveCardListToData(CardType cardType)
    {
        Debug.Log("CardCoun Dealer" + _cards.Count + " IsDealer " + isDealer);
        if (_cards.Count == 0 || GameManager.instance.IsNewPlayer) return;
        Stack<CardColorPallet> stackColorData = new();
        for (int i = 0; i < _cards.Count; i++)
        {
            Card card = _cards[i];
            stackColorData.Push(card.cardColor);
        }

        //int idData = isDealer == true ? id : id + 4;
        DataAPIController.instance.SaveStackCard(id, cardType, stackColorData);
    }

    public int CompareTo(Slot other)
    {
        if (other == null) return 1;
        return this.X.CompareTo(other.X);
    }
    internal void ReorderCards()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            float y = i * sizePerCard;
            _cards[i].transform.localPosition = new Vector3(0, y, 0);
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if (!GameManager.instance.IsNewPlayer) SaveCardListToData(IngameController.instance.CurrentCardType);

    }

    private void Reset()
    {
        status = SlotStatus.InActive;
        //SetSlotPrice(0, 0, Currency.Gold);
        _cards.Clear();
    }
    public void CheckOnTouchEvent()
    {
        if (onToucheHandle != null && onToucheHandle.GetPersistentEventCount() > 0)
        {
            //Debug.Log("The onToucheHandle event has listeners and is enabled.");
            // Invoke the event with a sample boolean value
            onToucheHandle.Invoke(true);
        }
        else
        {
            //Debug.Log("The onToucheHandle event has no listeners or is not enabled.");
            onToucheHandle.RemoveAllListeners();
            onToucheHandle.AddListener(TapHandler);
        }
    }
}
