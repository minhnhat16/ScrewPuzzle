using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DealButton : MonoBehaviour
{
    public RectTransform spawnPoint;
    public RectTransform dealCounter;
    public const int spawnSize = 5;
    [SerializeField] private bool isCountingTime;
    [SerializeField] private int seconds;
    [SerializeField] private int minutes;
    [SerializeField] private int targetMinutes;
    [SerializeField] private int targetSeconds;
    [SerializeField] private int currentCardCounter;
    [SerializeField] private int maxCardCounter;
    [SerializeField] private float floatTimeCounter;
    [SerializeField] private float delayBtwCards = 0.075f;
    [SerializeField] private float delayBtwSlots = 0.25f;
    [SerializeField] private Text lb_cardCounter;
    [SerializeField] private Text lb_timeCounter;
    [SerializeField] private Button tapBtn;
    [SerializeField] private Vector3 spawnVect;
    [SerializeField] private string timeCounter;
    [SerializeField] private string lastTimeData;
    [SerializeField] private DateTime targetTime;
    [SerializeField] private Image fill_CardCoutn;
    [HideInInspector] public UnityEvent<bool> onCardPoolEmty = new();
    [HideInInspector] public UnityEvent<bool> onCardRechage = new();
    private bool isLastSlot;

    public Button TapBtn { get => tapBtn; set => tapBtn = value; }
    public int CurrentCardCounter { get => currentCardCounter; set => currentCardCounter = value; }

    private void OnEnable()
    {
        tapBtn.onClick.AddListener(HandleTap);
        onCardRechage.AddListener(DoTimeCounter);
        onCardPoolEmty.AddListener(DoCardCharge);
        DataTrigger.RegisterValueChange(DataPath.CURRENTCARDPOOL, (data) =>
        {
            if (data == null) return;
            NewCouterData((int)data, 30);
        });
    }

    private void OnDisable()
    {
        tapBtn.onClick.RemoveAllListeners();
        onCardRechage.RemoveListener(DoTimeCounter);
        onCardPoolEmty.RemoveListener(DoCardCharge);
    }
    public IEnumerator Init()
    {
        spawnPoint = gameObject.GetComponent<RectTransform>();
        yield return new WaitUntil(() => DataAPIController.instance.isInitDone == true);
        lastTimeData = DataAPIController.instance.GetLastTimeData();
        timeCounter = DataAPIController.instance.GetLastTimeData();
        currentCardCounter = DataAPIController.instance.CurrentCardPool();
        maxCardCounter = DataAPIController.instance.MaxCardPool();
        FillCardCounter(currentCardCounter, maxCardCounter);
        targetTime = DateTime.Parse(lastTimeData);
        onCardRechage.Invoke(targetTime > DateTime.Now);
        InvokeRepeating(nameof(UpdateCounter), 1, 1f);
    }

    private void UpdateCounter()
    {
        if (gameObject.activeInHierarchy)
        {
            CardCounterTextUpdate(currentCardCounter, maxCardCounter);
        }
    }
    public void DoTimeCounter(bool isTimeCounter)
    {
        isCountingTime = isTimeCounter;
        StartCoroutine(UpdateTime());
    }
    IEnumerator UpdateTime()
    {
        //Debug.Log("UPDATE TIME");
        while (true)
        {
            // Tính toán thời gian còn lại
            //targetTime = DataAPIController.instance.gettar
            TimeSpan timeRemaining = targetTime - DateTime.Now;

            // Nếu thời gian đã hết, dừng bộ đếm
            if (timeRemaining.TotalSeconds <= 0)
            {
                lb_timeCounter.text = "Card is full now";

                // Thực hiện các hành động khác tại đây nếu cần
                isCountingTime = false;
                currentCardCounter = maxCardCounter;
                //DataAPIController.instance.SetCurrrentCardPool(currentCardCounter, null);
                FillCardCounter(currentCardCounter, maxCardCounter);
                yield break;
            }

            //lb_timeCounter.text = $"500 cards in {minutes}:{seconds}";
            lb_timeCounter.text = "500 cards in " + string.Format("{0:00}:{1:00}", timeRemaining.Minutes, timeRemaining.Seconds);
            yield return null;
        }
    }
    public void FillCardCounter(int current, int max)
    {
        if (!gameObject.activeSelf) return;
        float targetPercent = (float)current / (float)max;
        Debug.LogWarning("target percent " + current + " "+ max);
        if (gameObject.activeInHierarchy) StartCoroutine(FillCounterOverTime(targetPercent, 1f)); // 1f là thời gian chuyển đổi
    }
    private IEnumerator FillCounterOverTime(float targetPercent, float duration)
    {
        float startPercent = fill_CardCoutn.fillAmount;
        float timeElapsed = 0f;

        while (timeElapsed <= duration)
        {
            timeElapsed += Time.deltaTime;
            fill_CardCoutn.fillAmount = Mathf.Lerp(startPercent, targetPercent, timeElapsed / duration);
            yield return null;
        }
        //Debug.LogWarning("target percent after update " + targetPercent);
        fill_CardCoutn.fillAmount = targetPercent;
    }
    public void CardCounterTextUpdate(int currentCard, int maxCard)
    {
        FillCardCounter(currentCard, maxCard);
        if (currentCard < 0) lb_cardCounter.text = $"{0}/{maxCard}";
        else
        {
            lb_cardCounter.text = $"{currentCard}/{maxCard}";

        }
    }
    public void HandleTap()
    {
        if (currentCardCounter <= 0)
        {
            onCardPoolEmty?.Invoke(true);
            tapBtn.interactable = false;
            Player.Instance.isDealBtnActive = false;
            return;
        }
        //Debug.Log("Handel tap dealbutton");
        if (Player.Instance.isAnimPlaying) return;
        if (!Player.Instance.isDealBtnActive)
        {
            //Debug.LogWarning("dealbutton active false");
            Player.Instance.isDealBtnActive = true;
            tapBtn.interactable = false;
        }
        if (Player.Instance.fromSlot is not null)
        {
            float tempY;
            foreach (var card in Player.Instance.fromSlot.GetSelectedCards())
            {
                tempY = card.transform.position.y;
                card.transform.DOMoveY(tempY - 0.1f, 0.2f);
                if (currentCardCounter <= 0)
                {
                    onCardPoolEmty?.Invoke(true);
                    return;
                }
            }

            Player.Instance.fromSlot.GetSelectedCards().Clear();
            //Player.Instance.isDealBtnActive = true;
            //Player.Instance.isAnimPlaying = true;
            Player.Instance.fromSlot.UpdateSlotState();
            Player.Instance.fromSlot = null;
            Player.Instance.toSlot = null;
        }
        Tween t = transform.DOScaleZ(0.5f, 0.24f).OnComplete(() =>
        {
            t = transform.DOScaleZ(1.25f, 1f);
            t.OnPlay(() => { Player.Instance.isAnimPlaying = true; });
            t.OnComplete(() =>
            {
                t.Kill();
            });

        });

        float timer = 0.25f;
        var listSlot = IngameController.instance.GetListActiveSortByCardCount();
        int totalSlotCanDealWithCurrentCard = currentCardCounter / 5;
        totalSlotCanDealWithCurrentCard = listSlot.Count > totalSlotCanDealWithCurrentCard ? totalSlotCanDealWithCurrentCard : listSlot.Count;
        for (int i = 0; i < totalSlotCanDealWithCurrentCard; i++)
        {
            Slot s = listSlot[i];
            s.SetTargetToDealCard(true);
            StartCoroutine(SendingCard(s, timer));
            timer += delayBtwSlots;

        }
        StartCoroutine(WaitForSendCardDone(timer + Player.Instance.timeDisableCard));
        DataAPIController.instance.SetCurrrentCardPool(currentCardCounter, () =>
        {
        });
        FillCardCounter(currentCardCounter, maxCardCounter);

    }
    public IEnumerator WaitForSendCardDone(float time)
    {
        yield return new WaitForSeconds(time);
        tapBtn.interactable = true;
        Player.Instance.isAnimPlaying = false;
    }
    public void NewCouterData(int data, double target)
    {
        int CardCounterNewData = (int)data;
        if (CardCounterNewData < maxCardCounter && !isCountingTime)
        {
            targetTime = DateTime.Now.AddMinutes(target);
            timeCounter = targetTime.ToString();
            DataAPIController.instance.SaveTargetTime(timeCounter, () =>
            {
                onCardRechage?.Invoke(true);
            });
        }
        FillCardCounter(currentCardCounter, maxCardCounter);
    }

    IEnumerator SendingCard(Slot s, float timer)
    {
        yield return new WaitForSeconds(timer);
        SendCardTo(s);

        tapBtn.interactable = false;
    }
    public void SetOnNewPlayer(bool isNewPlayer)
    {
        dealCounter.gameObject.SetActive(!isNewPlayer);
    }
    private void SendCardTo(Slot destination)
    {
        float d = Player.Instance.duration;
        float offset = destination._cards.Count == 0 ? destination.transform.position.y + 0.1f : destination._cards.Last().transform.position.y + Player.Instance.cardPositionOffsetY;
        float z = destination._cards.Count == 0 ? Player.Instance.cardPositionOffsetZ : destination._cards.Last().transform.position.z + Player.Instance.cardPositionOffsetZ;

        //destination.SetCollisionEnable(false);

        Player.Instance.isAnimPlaying = true;

        CardColorPallet targetColor = destination.TopColor();

        List<CardColorPallet> option = new(GameManager.instance.listCurrentCardColor);

        option.Remove(targetColor);


        //Debug.Log($"random index {randomIndex}, option count {option.Count}");
        CardType currentType = IngameController.instance.CurrentCardType;
        float delay = 0;
        Vector3 newSpawnPoint;
        Vector3 woldPoint;
        Card c;
        Vector3 newvect = new(0, 0, 10);
        Stack<CardColorPallet> stackColor = new();
        CardColorPallet previousColor = CardColorPallet.Empty;
        for (int i = 0; i < 5; i++)
        {
            CardColorPallet selectedColor;

            // Nếu đây là lần lặp thứ 2, ta sử dụng lại màu trước đó để tạo 2 màu giống nhau liền kề
            if (i % 2 == 1 && previousColor != CardColorPallet.Empty)
            {
                selectedColor = previousColor;
            }
            else
            {
                // Lựa chọn màu ngẫu nhiên từ danh sách
                int randomIndex = UnityEngine.Random.Range(0, option.Count);
                selectedColor = option[randomIndex];

                // Lưu lại màu hiện tại để sử dụng cho lần lặp tiếp theo
                previousColor = selectedColor;
            }

            stackColor.Push(selectedColor);
        }

        for (int i = 0; i < spawnSize; i++)
        {
            var spawnColor = stackColor.Pop();
            ColorConfigRecord colorRecord = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(spawnColor);
            c = CardPool.Instance.pool.SpawnNonGravity();
            c.ColorSetBy(colorRecord.Name, currentType, colorRecord.Color);

            newSpawnPoint = spawnPoint.position;
            woldPoint = ScreenToWorld.Instance.PreverseConvertPosition(newSpawnPoint);
            c.transform.SetLocalPositionAndRotation(woldPoint + spawnVect - newvect, Quaternion.identity);
            //Debug.Log($"Spawn Point {newSpawnPoint} destination {destination.Pos}");
            c.PlayAnimation(destination, d, Player.Instance.height, Player.Instance.ease, offset, z, delay);
            destination._cards.Add(c);
            delay += delayBtwCards;
            offset += Player.Instance.cardPositionOffsetY;
            z += Player.Instance.cardPositionOffsetZ;
            destination.SetColliderSize(1);
        }
        currentCardCounter -= spawnSize;
        destination.CenterCollider();
        StartCoroutine(UpdateSlotType(destination, delay + d));

    }
    private void DoCardCharge(bool onCardEmty)
    {

        bool isVideoReady = ZenSDK.instance.IsVideoRewardReady();
        Debug.Log("DO carrd charge ");
        OutOffCardParam param = new();
        param.targetTime = targetTime;
        DialogManager.Instance.ShowDialog(DialogIndex.OutOffCardDialog, param);
        //}

    }

    IEnumerator UpdateSlotType(Slot destination, float v)
    {
        yield return new WaitForSeconds(v);
        destination.UpdateSlotState();
        //Player.Instance.isAnimPlaying = false;
    }
    private void OnApplicationPause(bool pause)
    {
        DataAPIController.instance.SaveTargetTime(timeCounter, null);
        DataAPIController.instance.SetCurrrentCardPool(currentCardCounter, () =>
        {
        });

    }
    private void OnApplicationQuit()
    {
        DataAPIController.instance.SaveTargetTime(timeCounter, null);
        DataAPIController.instance.SetCurrrentCardPool(currentCardCounter, () =>
        {
        });
    }
}
