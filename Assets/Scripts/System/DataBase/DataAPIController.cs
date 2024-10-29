using System.Collections.Generic;
using UnityEngine;

namespace System.DataBase
{
    public class DataAPIController : MonoBehaviour
    {
        public static DataAPIController instance;
        public bool isInitDone;
        [SerializeField]
        private DataModel dataModel;
        private void Awake()
        {
            instance = this;
        }

        public void InitData(Action callback)
        {
            Debug.Log("(BOOT) // INIT DATA");
            isInitDone = false;
            dataModel.InitData(() =>
            {
                // CheckDailyLogin();
                isInitDone = true;
                callback();
            });
            Debug.Log("==========> BOOT PROCESS SUCCESS <==========");
        }

        #region Get Data
        /*public int GetPlayerLevel()
        {
            //Debug.Log("DATA === LEVEL");
            return dataModel.ReadData<int>(DataPath.LEVEL);
        }*/
        public bool IsNewPlayer()
        {
            return dataModel.ReadData<bool>(DataPath.NEWPLAYER);
        }
        public void SetPlayerNewAtFalse(Action callback)
        {
            dataModel.UpdateData(DataPath.NEWPLAYER, false, () => callback?.Invoke());
        }
        #region CURRRENCY
        public CurrencyWallet GetWalletByType(Currency currency)
        {
            if (currency == Currency.Gold)
            {
                var wallet = GetGoldWallet();
                return wallet;
            }
            else if (currency == Currency.Gem)
            {
                var wallet = GetGemWallet();
                return wallet;
            }
            return null;
        }

        public void MinusGoldWallet(int minus, Action<bool> callback)
        {
            var goldWallet = GetGoldWallet();
            goldWallet.amount -= minus;
            SaveGold(goldWallet, callback);
        }
        public void MinusGemWallet(int minus, Action<bool> callback)
        {
            var gemWallet = GetGemWallet();
            gemWallet.amount -= minus;
            SaveGem(gemWallet, callback);
        }
        public void MinusWalletByType(int minus, Currency currency, Action<bool> callback)
        {
            if (currency == Currency.Gold)
            {
                MinusGoldWallet(minus, callback);
            }
            else if (currency == Currency.Gem)
            {
                MinusGemWallet(minus, callback);
            }
            else { callback.Invoke(false); }
        }
        public void SaveWallet(CurrencyWallet wallet, Currency currency, Action<bool> callback)
        {
            dataModel.UpdateDataDictionary(DataPath.WALLETINVENT, currency.ToString(), wallet, () =>
            {
                callback?.Invoke(true);
                return;
            });
            callback?.Invoke(false);

        }
        public CurrencyWallet GetGoldWallet()
        {
            return dataModel.ReadData<CurrencyWallet>(DataPath.GOLDINVENT);
        }
        public CurrencyWallet GetGemWallet()
        {
            return dataModel.ReadData<CurrencyWallet>(DataPath.GEMINVENT);
        }
        public int GetGold()
        {
            CurrencyWallet goldWallet = dataModel.ReadData<CurrencyWallet>(DataPath.GOLDINVENT);
            return goldWallet.amount;
        }
        public int GetGem()
        {
            CurrencyWallet gemWallet = dataModel.ReadData<CurrencyWallet>(DataPath.GEMINVENT);
            return gemWallet.amount;
        }

        /*public void SetLevel(int playerLevel, Action callback)
        {
            int currentLevel = GetPlayerLevel();
            dataModel.UpdateData(DataPath.LEVEL, playerLevel, () =>
            {
                //Debug.Log($"Save level done at {currentLevel}");
                callback?.Invoke();
            });
        }*/

        public void AddGold(int add,Action<bool> callback)
        {
            CurrencyWallet gold = GetGoldWallet();
            gold.amount += add;
            SaveGold(gold, callback);
            //TODO : ADD TRIGGER FOR GOLD AND GEM
        }
        public void SaveGold(CurrencyWallet gold, Action<bool> callback)
        {
            dataModel.UpdateData(DataPath.GOLDINVENT, gold, () =>
            {
                //Debug.Log("gold amount" + gold.amount);
                if(callback != null) callback.Invoke(true);
                return;
            });

        }
        public void AddGem(int add)
        {
            CurrencyWallet gem = GetGemWallet();
            gem.amount += add;
            SaveGem(gem, null);
        }
        public void SaveGem(CurrencyWallet gem, Action<bool> callback)
        {
            dataModel.UpdateData(DataPath.GEMINVENT, gem, () =>
            {
                callback?.Invoke(true);
                DataTrigger.TriggerValueChange(DataPath.GEMINVENT, gem);
                return;
            });
            callback?.Invoke(false);

        }
        #endregion

        #endregion
        #region daytimedata
        public string GetDayTimeData()
        {
            string day = dataModel.ReadData<string>(DataPath.LASTSAVETIME);
            //Debug.Log($"day {day}");
            return day;
        }
        public void SetDayTimeData(string day)
        {
            if (!string.IsNullOrEmpty(day))
            {
                dataModel.UpdateData(DataPath.CURRENTTIME, day, () =>
                {
                    Debug.Log("SAVE DAYTIME DATA SUCCESSFULL");
                });
            }
        }
        #endregion
        #region Others
        public float GetCurrentExp()
        {
            return dataModel.ReadData<float>(DataPath.EXPCURRENT);
        }
        public void SetCurrentExp(float currentExp, Action callback)
        {
            dataModel.UpdateData(DataPath.EXPCURRENT, currentExp, () =>
            {
                //Debug.Log($"Save current exp to data successfull {currentExp}");
                callback?.Invoke();
            });
        }
        public ItemData GetItemData(ItemType type)
        {
            string subPath = SubPathForItem(type);
            if(subPath !=null)
            {
                ItemData itemData = dataModel.ReadData<ItemData>(subPath);
                return itemData;
            }
            return null;
        }
        public int GetItemTotal(ItemType type)
        {
            //Debug.Log("GetItemTotal");
            ItemData itemData = GetItemData(type);
            int total = itemData.total;
            if(total < 0) total = 0;
            //Debug.Log($"TOTAL ITEM{itemData.id} {total}");
            return total;
        }
        public void AddItemTotal(ItemType type, int inTotal)
        {
            int total  = GetItemTotal(type) + inTotal;
    
            SetItemTotal(type, total);
        }
        public void SetItemTotal(ItemType type, int inTotal)
        {
            if (inTotal < 0) return;
            ItemData itemData = new()
            {
                type = type,
                total = inTotal,
            };
            //Debug.Log("DATA === SAVE ITEMDATA");
            string subPath = SubPathForItem(type);
            dataModel.UpdateData(subPath, itemData, () =>
            {
                return;
            }); 
  
        }

        private static string SubPathForItem(ItemType type)
        {
            return DataPath.ITEM + $"/{char.ToLower(type.ToString()[0]) + type.ToString().Substring(1)}";
        }

        public bool GetSpinData()
        {
            return dataModel.ReadData<bool>(DataPath.ISSPIN);
        }
        public void SetSpinData(bool isSpinned, Action callback = null)
        {
            dataModel.UpdateData(DataPath.ISSPIN, isSpinned, () =>
            {
                callback?.Invoke();
            });
        }
        public void SetSpinTimeData(DateTime timeSpinned, Action callback = null)
        {
            dataModel.UpdateData(DataPath.TIMESPIN, timeSpinned.ToString(), () =>
            {
                callback?.Invoke();
            });
        }
        public DateTime GetSpinTimeData()
        {
            try
            {
                string timeString = dataModel.ReadData<string>(DataPath.TIMESPIN);
                DateTime time = DateTime.Parse(timeString);
                return time;
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log the error)
                Debug.LogError($"Error parsing date: {ex.Message}");

                // Return a default value or handle accordingly
                return DateTime.MinValue;
            }
        }
        public bool GetIsClaimTodayData()
        {
            return dataModel.ReadData<bool>(DataPath.ISDAILYCLAIM);
        }
        public void SetIsClaimTodayData(bool isClaim, Action callback = null)
        {
            dataModel.UpdateData(DataPath.ISDAILYCLAIM, isClaim, () =>
            {
                callback?.Invoke();
            });
        }
        public DateTime GetTimeClaimItem()
        {
            string stringDate = dataModel.ReadData<string>(DataPath.DAILYTIMECLAIMED);
            DateTime datetime = DateTime.Parse(stringDate);
            return datetime;
        }
        public void SetTimeClaimItem(DateTime time, Action callback = null)
        {
            string stringDate = time.ToString();
            dataModel.UpdateData(DataPath.DAILYTIMECLAIMED, stringDate, () =>
            {
                callback?.Invoke();
            });
        }

        public DailyData GetDailyData()
        {
            var dailyData = dataModel.ReadData<DailyData>(DataPath.DAILYDATA);
            return dailyData;
        }
        public List<DailyItemData> GetAllDailyData()
        {
            var dailyData = dataModel.ReadData<List<DailyItemData>>(DataPath.DAILYLIST);
            return dailyData;
        }
        public void SetNewDailyCircle()
        {
            List<DailyItemData> newData = new();
            for (int i = 0; i < 7; i++)
            {
                DailyItemData dailyData = new();
                dailyData.day = i;
                dailyData.currentType = IEDailyType.Unavailable;
                newData.Add(dailyData);
            }
            dataModel.UpdateData(DataPath.DAILYLIST, newData, () =>
            {
                Debug.Log("UPDTE NEW DAILY CIRCLE");
            });
        }
        public DailyItemData GetDailyData(int idDay)
        {
            //Debug.Log($"ID day {idDay} ");
            var _dailyData = GetAllDailyData();
            DailyItemData dailyData = _dailyData[idDay];
            return dailyData;
        }
        public void SetDailyData(int day, IEDailyType type)
        {
            //Debug.Log($"SET DAILY DATA {day} + {type}");
            List<DailyItemData> _dailyData = GetAllDailyData();
            DailyItemData dailyData = _dailyData[day];
            if (dailyData is null) Debug.LogError("Dailydatanull");
            else
            {
                dailyData.currentType = type;

                _dailyData[day] = dailyData;
                dataModel.UpdateData(DataPath.DAILYLIST, _dailyData, () =>
                {

                });
            }
        }
        #endregion

        public List<LevelData> GetAllLevelData()
        {
            var listLevelData =  dataModel.ReadData<List<LevelData>>(DataPath.ALLLEVEL);
            return listLevelData ?? new List<LevelData>();
        }
    }
}
