using ConfigFile;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
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
                CheckDailyBlockReset();
                isInitDone = true;
                callback();
            });
            Debug.Log("==========> BOOT DATA DONE <==========");
        }

        public bool IsNewPlayer()
        {
            return dataModel.ReadData<bool>(DataPath.NEWPLAYER);
        }
        public void SetPlayerNewAtFalse(Action callback)
        {
            dataModel.UpdateData(DataPath.NEWPLAYER, false, () => callback?.Invoke());
        }


        public void AddOneCurrent()
        {
            var current = dataModel.ReadData<int>(DataPath.CURRENTPLAYERLEVEL);
            current += 1;
            SavePlayerLevel(current);
        }
        public int GetPlayerLevel()
        {
            return dataModel.ReadData<int>(DataPath.CURRENTPLAYERLEVEL);
        }
        #region CURRRENCY
        public CurrencyWallet GetWalletByType(Currency currency)
        {
            if (currency == Currency.Gold)
            {
                var wallet = GetGoldWallet();
                return wallet;
            }
            else if (currency == Currency.Ticket)
            {
                var wallet = GetTicketWallet();
                return wallet;
            }
            return null;
        }

        public void MinusGoldWallet(long minus, Action<bool> callback)
        {
            var goldWallet = GetGoldWallet();
            goldWallet.amount -= minus;
            SaveGold(goldWallet.amount, callback);
        }



        public void SetCurrentScrewData(ScrewSkinData newSkin, Action callback = null)
        {
            dataModel.UpdateData(DataPath.CRSCREWCOLOR, newSkin, callback);
        }
        public void SetCurrentBoardData(BoardColorData newBoardColor, Action callback = null)
        {
            dataModel.UpdateData(DataPath.CRBOARDCOLOR, newBoardColor, callback);
        }

        public void SetCurrentBackGroundData(BackGroundData newBackground, Action callback = null)
        {
            dataModel.UpdateData(DataPath.CRBACKGROUND, newBackground, callback);
        }
        public ScrewSkinData GetCurrentScrewData()
        {
            var screwSkinData = dataModel.ReadData<ScrewSkinData>(DataPath.CRSCREWCOLOR);
            Debug.LogWarning("Screw Skin data" + screwSkinData.name);
            return screwSkinData;
        }
        public BackGroundData GetCurrentBackGroundData()
        {
            var backGroundData = dataModel.ReadData<BackGroundData>(DataPath.CRBACKGROUND);
            Debug.LogWarning("Back Ground data" + backGroundData.name);
            return backGroundData;
        }
        public BoardColorData GetCurrentBoardData()
        {
            var boardColorData = dataModel.ReadData<BoardColorData>(DataPath.CRBOARDCOLOR);
            Debug.LogWarning("Board Color data" + boardColorData.name);
            return boardColorData;
        }
        public Dictionary<string, ScrewSkinData> GetAllScrewSkinData()
        {
            var screwSkinData = dataModel.ReadData<Dictionary<string, ScrewSkinData>>(DataPath.SCREWCOLOR);
            return screwSkinData;
        }

        public Dictionary<string, BackGroundData> GetAllBackGroundData()
        {
            var backGroundData = dataModel.ReadData<Dictionary<string, BackGroundData>>(DataPath.BACKGROUND);
            return backGroundData;
        }
        public Dictionary<string, BoardColorData> GetAllScrewBoardColorData()
        {
            var boardColorData = dataModel.ReadData<Dictionary<string, BoardColorData>>(DataPath.BOARDCOLOR);
            return boardColorData;
        }

        public void UpdateScrewSkinData(ScrewSkinData newData, Action callback = null)
        {
            dataModel.UpdateData(DataPath.SCREWCOLOR, newData, callback);
        }
        public void UpdateBoardColorData(BoardColorData newData, Action callback = null)
        {
            dataModel.UpdateData(DataPath.BOARDCOLOR, newData, callback);
        }
        public void UpdateBackGroundData(BackGroundData newData, Action callback = null)
        {
            dataModel.UpdateData(DataPath.BACKGROUND, newData, callback);
        }

        internal object GetBoardData()
        {
            throw new NotImplementedException();
        }

        /* public void MinusGemWallet(int minus, Action<bool> callback)
{
    var gemWallet = GetGemWallet();
    gemWallet.amount -= minus;
    SaveGem(gemWallet, callback);
}*/
        /* public void MinusWalletByType(int minus, Currency currency, Action<bool> callback)
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
         }*/

        internal void SavePlayerLevel(int currentLevel)
        {
            dataModel.UpdateData(DataPath.CURRENTPLAYERLEVEL, currentLevel);
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
        public CurrencyWallet GetTicketWallet()
        {
            return dataModel.ReadData<CurrencyWallet>(DataPath.TICKET);
        }
        public long GetGold()
        {
            CurrencyWallet goldWallet = dataModel.ReadData<CurrencyWallet>(DataPath.GOLDINVENT);
            return goldWallet.amount;
        }
        public long GetTicket()
        {
            CurrencyWallet ticketWallet = dataModel.ReadData<CurrencyWallet>(DataPath.TICKET);
            return ticketWallet.amount;
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


        public void AddGold(long gold, Action<bool> callback = null)
        {
            CurrencyWallet goldWallet = GetGoldWallet();
            goldWallet.amount += gold;
            SaveGold(gold, callback);
        }
        public void SaveGold(long gold, Action<bool> callback = null)
        {
            var goldWallet = GetGoldWallet();
            goldWallet.amount = gold;
            dataModel.UpdateData(DataPath.GOLDINVENT, goldWallet, () =>
            {
                callback?.Invoke(true);
                return;
            });

        }
        public void SaveTicket(long amount, Action<bool> callback = null)
        {
            var ticketWallet = GetTicketWallet();
            ticketWallet.amount = amount;
            dataModel.UpdateData(DataPath.TICKET, ticketWallet, () =>
            {
                callback?.Invoke(true);
                DataTrigger.TriggerValueChange(DataPath.TICKET, ticketWallet);
                return;
            });
            callback?.Invoke(false);

        }
        #endregion

        #region daytimedata

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
            var itemData = dataModel.ReadDictionary<ItemData>(DataPath.ITEMDICT, type.ToString());
            return itemData ?? null;
        }
        public int GetItemDataTotal(ItemType type)
        {
            var itemData = dataModel.ReadDictionary<int>(DataPath.ITEMDICT, type.ToString());
            return itemData;
        }
        public int GetItemTotal(ItemType type)
        {
            //Debug.Log("GetItemTotal");
            ItemData itemData = GetItemData(type);

            if (itemData == null) return 0;
            int total = itemData.total;
            if (total < 0) total = 0;
            //Debug.Log($"TOTAL ITEM{itemData.id} {total}");
            return total;
        }
        public void AddItemTotal(ItemType type, int inTotal)
        {
            int total = GetItemTotal(type) + inTotal;

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
            dataModel.UpdateDataDictionary<ItemData>(DataPath.ITEMDICT, type.ToString(), itemData, () =>
            {
                return;
            });

        }

        private static string SubPathForItem(ItemType type)
        {
            return DataPath.ITEMDICT + $"/{char.ToLower(type.ToString()[0]) + type.ToString().Substring(1)}";
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
                dailyData.currentType = DailyType.Unavailable;
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
        public void SetDailyData(int day, DailyType type)
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
            var listLevelData = dataModel.ReadData<List<LevelData>>(DataPath.ALLLEVEL);
            return listLevelData ?? new List<LevelData>();
        }
        public void SaveNewLevelData(LevelData data, Action callback = null)
        {
            var levelsData = GetAllLevelData();
            levelsData.Add(data);
            dataModel.UpdateData(DataPath.ALLLEVEL, levelsData, callback);
        }

        internal void AddItemByConfig(List<ShopItemRecord> items)
        {
            ItemType type;
            foreach (ShopItemRecord item in items)
            {
                type = item.Id;
                int quantity = item.Quantity;
                if (type == ItemType.Ticket)
                {
                    long tickets = GetTicket() + quantity;
                    SaveTicket(tickets);
                }
                else if (type == ItemType.Gold)
                {
                    long golds = GetGold() + quantity;
                    SaveGold(golds);
                }
                else AddItemTotal(type, quantity);

            }
        }

        internal void MinusItemByOne(ItemType itemType)
        {
            var itemData = GetItemData(itemType);
            if (itemData == null) return;

            itemData.total--;
            SetItemTotal(itemType, itemData.total);
        }


        #region Mission Data

        public MissionProgress GetMissionProgress(int missionId)
        {
            // Đọc toàn bộ dictionary
            var missions = dataModel.ReadData<Dictionary<string, MissionProgress>>(DataPath.MISSION_PROGRESS);

            // Nếu root null → tạo mới
            if (missions == null)
            {
                missions = new Dictionary<string, MissionProgress>();
                dataModel.UpdateData(DataPath.MISSION_PROGRESS, missions);
            }

            // Đọc mission theo key
            var mission = dataModel.ReadDictionary<MissionProgress>(DataPath.MISSION_PROGRESS, missionId.ToKey());

            // Nếu chưa có → tạo nhiệm vụ mới
            if (mission == null)
            {
                mission = new MissionProgress
                {
                    missionId = missionId,
                    current = 0,
                    state = MissionState.InProgress,   // ✔ đúng trạng thái cho new mission
                };

                missions.Add(missionId.ToKey(), mission);
                dataModel.UpdateData(DataPath.MISSION_PROGRESS, missions);
            }

            return mission;
        }

        public void UpdateMissionProgress(MissionProgress missionProgress, Action callback = null)
        {

            Debug.Log(" update mission progress " + missionProgress.missionId);
            dataModel.UpdateDataDictionary(DataPath.MISSION_PROGRESS, missionProgress.missionId.ToKey(), missionProgress, callback);
        }
        public void AddMissionProgress(int missionId, int amount = 1, Action callback = null)
        {
            var mission = dataModel.ReadDictionary<MissionProgress>(DataPath.MISSION_PROGRESS, missionId.ToKey());

            if (mission == null) return;

            mission.current += amount;

            dataModel.UpdateDataDictionary(DataPath.MISSION_PROGRESS, missionId.ToKey(), mission, callback);
        }

        public void CompleteMission(int missionId, Action callback = null)
        {


            var mission = dataModel.ReadDictionary<MissionProgress>(DataPath.MISSION_PROGRESS, missionId.ToKey());
            if (mission == null) return;

            mission.state = MissionState.Completed;

            dataModel.UpdateDataDictionary(DataPath.MISSION_PROGRESS, missionId.ToKey(), mission, callback);

        }

        internal bool IsChestClaimed(int id)
        {
            return false;
        }

        internal bool CheckStageUnlocked(int id)
        {
            var stage = dataModel.ReadDictionary<StageProgress>(DataPath.STAGEPATH, id.ToString());


            Debug.Log($"Check stage unlocked {id}: stage {stage} is unlocked {stage.isUnlocked}");
            if (stage == null) return false;
            return stage.isUnlocked;
        }

        internal void SetCurrentStage(int v)
        {

        }

        internal void ResetDailyMissionProgress()
        {
            // Lấy tất cả mission từ config
            var missionConfig = ConfigFileManager.Instance.GetConfig<MissionConfig>();
            if (missionConfig == null) return;

            var allMissions = missionConfig.GetAllRecord();
            if (allMissions == null) return;

            foreach (var mission in allMissions)
            {
                MissionProgress progress = new MissionProgress()
                {
                    missionId = mission.Id,
                    current = 0,
                    state = MissionState.InProgress
                };

                UpdateMissionProgress(progress);
            }

            Debug.Log("[Daily Reset] All mission progress reset.");
        }

        internal void ResetChestStates()

        {
            var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>();
            if (chestConfig == null)
            {
                Debug.LogWarning("[DataAPI] ChestConfig not found!");
                return;
            }

            var allChests = chestConfig.GetAllRecord();
            if (allChests == null)
                return;

            foreach (var chest in allChests)
            {
                ChestStageData newState = new ChestStageData
                {
                    chestId = chest.Id,
                    isClaimed = false,
                    isUnlocked = chest.Id == 0,   // Unlock chest 0 (stage đầu)
                    progress = 0f
                };

                SaveChestState(newState);
            }

            Debug.Log("[Daily Reset] All chest states reset.");
        }


        private void SaveChestState(ChestStageData state)
        {
            dataModel.UpdateDataDictionary(DataPath.CHESTSTAGE, state.chestId.ToSafeString(), state);
        }

        public void AddStage(StageProgress progress, Action<bool> isDone = null)
        {
            var stageDict = dataModel.ReadData<Dictionary<int, StageProgress>>(DataPath.STAGEPATH);
            var hasInDict = stageDict.ContainsValue(progress);
            if (hasInDict)
            {
                isDone?.Invoke(false);
                return;
            }
            stageDict.Add(progress.stageId, progress);
        }
        public ChestStageData GetChestState(int chestId)
        {
            var chestData = dataModel.ReadDictionary<ChestStageData>(DataPath.CHESTSTAGE, chestId.ToString());

            if (chestData == null)
            {
                chestData = new ChestStageData
                {
                    chestId = chestId,
                    isUnlocked = chestId == 0,
                    isClaimed = false,
                    progress = 0
                };
                AddNewChestStage(chestData);
            }

            return chestData;
        }

        public void AddNewChestStage(ChestStageData data, Action<bool> isDone = null)
        {
            var chestDict = dataModel.ReadData<Dictionary<int, ChestStageData>>(DataPath.CHESTSTAGE);

            chestDict.Add(data.chestId, data);
            dataModel.UpdateData(DataPath.CHESTSTAGE, data);
            isDone?.Invoke(true);
        }
        public StageProgress GetStageProgress(int stageId)
        {
            var progressData = dataModel.ReadDictionary<StageProgress>(DataPath.STAGEPATH, stageId.ToSafeString());
            if (progressData == null)
            {
                progressData = new StageProgress
                {
                    stageId = stageId,
                    isUnlocked = stageId == 0,   // stage 0 unlock mặc định
                    isCompleted = false,
                    rewardClaimed = false,
                    chestProgress = 0
                };
                AddStage(progressData);
            }

            return progressData;
        }

        public void UpdateStageProgress(StageProgress stage)
        {
            if (stage == null)
            {
                Debug.LogError("[DataAPI] UpdateStageProgress FAILED: stage = null");
                return;
            }


            // Lưu xuống DataModel (ổn định, dùng key stageId)
            dataModel.UpdateDataDictionary(
                DataPath.STAGEPATH,
                stage.stageId.ToString(),
                stage
            );

#if UNITY_EDITOR
            Debug.Log($"[Stage] Updated stage {stage.stageId}: unlocked={stage.isUnlocked}, completed={stage.isCompleted}");
#endif
        }

        public void UnlockStage(int stageId)
        {
            var stage = GetStageProgress(stageId);
            stage.isUnlocked = true;
            UpdateStageProgress(stage);
        }

        public void CompleteStage(int stageId)
        {
            var stage = GetStageProgress(stageId);
            stage.isCompleted = true;
            UpdateStageProgress(stage);

            // When a stage is completed, unlock the chest associated with that stage.
            UnlockChestForStage(stageId);

            Debug.Log($"[DataAPI] Stage {stageId} completed and chest unlocked if existed.");
        }

        public void UnlockNextStage(int currentStage)
        {
            // Keep existing behaviour: unlock the provided stage id
            UnlockStage(currentStage);
        }

        /// <summary>
        /// Ensures the chest state exists for given stageId and marks it unlocked.
        /// Uses SaveChestState(...) to persist the change.
        /// </summary>
        public void UnlockChestForStage(int stageId)
        {
            // GetChestState will create a default chest state if missing
            var chest = GetChestState(stageId);


            Debug.Log("Chest stage null " + chest);
            if (chest == null)
            {
                Debug.LogError($"[DataAPI] UnlockChestForStage: failed to obtain chest state for id={stageId}");
                return;
            }

            if (!chest.isUnlocked)
            {
                chest.isUnlocked = true;
                SaveChestState(chest);
                Debug.Log($"[DataAPI] Chest {stageId} unlocked.");
            }
            else
            {
                Debug.Log($"[DataAPI] Chest {stageId} was already unlocked.");
            }
        }

        //public void UnlockNextStage(int currentStage)
        //{
        //    UnlockStage(currentStage);
        //}

        internal int GetCurrentStage()
        {
            // Default
            var currentStage = 0;

            // Read stored stages (safe null-handling)
            var stages = dataModel.ReadData<Dictionary<int, StageProgress>>(DataPath.STAGEPATH) ?? new Dictionary<int, StageProgress>();

            // 1) Prefer the first unlocked stage that is NOT completed (the active stage player should play)
            var active = stages.Values
                .OrderBy(s => s.stageId)
                .LastOrDefault(s => s.isUnlocked && !s.isCompleted);

            if (active != null)
                return active.stageId;

            // 2) Fallback: return the highest unlocked stage (player progressed furthest)
            var lastUnlocked = stages.Values
                .Where(s => s.isUnlocked)
                .OrderByDescending(s => s.stageId)
                .LastOrDefault();

            if (lastUnlocked != null)
                return lastUnlocked.stageId;

            // 3) Final fallback: stage 0
            return currentStage;
        }

        internal void UpdateChestState(ChestStageData chestState)
        {
            string chestID = chestState.chestId.ToString();
            dataModel.UpdateDataDictionary(DataPath.CHESTSTAGE, chestID, chestState);
        }

        internal List<BlockParam> GetBlocksData()
        {
            var blocksData =
                dataModel.ReadData<Dictionary<int, BlockData>>(DataPath.BLOCKSDATA);

            if (blocksData == null || blocksData.Count == 0)
                return new List<BlockParam>();

            List<BlockParam> result = new List<BlockParam>();



            foreach (var item in blocksData)
            {
                var id = item.Key;
                var block = item.Value;
                BlockParam param = new BlockParam
                {
                    blockId = id,
                    screwRequired = block.screwRequired,
                    unlocked = block.unlocked,

                    // CLONE dictionary để tránh reference bug
                    removedCells = block.removedCells != null
                       ? new Dictionary<int, bool>(block.removedCells)
                       : new Dictionary<int, bool>()
                };

                result.Add(param);
            }


            return result;
        }
        public void RefreshBlockData()
        {
            var blockData = dataModel.ReadData<Dictionary<int, BlockData>>(DataPath.BLOCKSDATA);
            if (blockData.Count == 0)
            {
                blockData = new Dictionary<int, BlockData>(25);
            }
            ;
        }

        public void CheckDailyBlockReset()
        {
            var meta = dataModel.ReadData<TimeSaveMeta>(DataPath.TIMESAVEMETA);

            long now = DateTime.UtcNow.Ticks;

            if (meta == null)
            {
                meta = new TimeSaveMeta { lastResetUtcTicks = now };
                dataModel.UpdateData(DataPath.TIMESAVEMETA, meta);
                return;
            }

            TimeSpan delta = new TimeSpan(now - meta.lastResetUtcTicks);

            if (delta.TotalHours >= 24)
            {
                meta.lastResetUtcTicks = now;
                dataModel.UpdateData(DataPath.TIMESAVEMETA, meta);
                int puzzleId = GetRandomPuzzleId();
                SetCurrentPuzzle(puzzleId);
                RefreshBlockData();
                Debug.Log("[BlockData] Daily reset executed");
            }
        }

        // Returns a random puzzle id from PuzzleConfig (or -1 if none found)
        public int GetRandomPuzzleId()
        {
            var puzzleConfig = ConfigFileManager.Instance.GetConfig<PuzzleConfig>();
            if (puzzleConfig == null)
            {
                Debug.LogWarning("[DataAPI] GetRandomPuzzleId: PuzzleConfig not found.");
                return -1;
            }

            var puzzles = puzzleConfig.GetAllRecord();
            if (puzzles == null || puzzles.Count == 0)
            {
                Debug.LogWarning("[DataAPI] GetRandomPuzzleId: no puzzles available.");
                return -1;
            }

            // filter out null entries
            var list = puzzles.Where(p => p != null).ToList();
            if (list.Count == 0) return -1;

            var chosen = list[UnityEngine.Random.Range(0, list.Count)];

            // try common property/field names for id
            var idProp = chosen.GetType().GetProperty("Id") ?? chosen.GetType().GetProperty("ID");
            if (idProp != null)
            {
                var val = idProp.GetValue(chosen);
                return System.Convert.ToInt32(val);
            }

            var idField = chosen.GetType().GetField("id") ?? chosen.GetType().GetField("ID") ?? chosen.GetType().GetField("blockId");
            if (idField != null)
            {
                var val = idField.GetValue(chosen);
                return System.Convert.ToInt32(val);
            }

            Debug.LogWarning("[DataAPI] GetRandomPuzzleId: unable to locate Id field/property on puzzle record.");
            return -1;
        }
        internal void UpdateBlockCell(int blockid, int idCell, bool v)
        {
            var block = dataModel.ReadDictionary<BlockData>(DataPath.BLOCKSDATA, blockid.ToString());
            block.removedCells[idCell] = v;
            dataModel.UpdateDataDictionary(DataPath.BLOCKSDATA, blockid.ToString(), block);
        }



        public int CurrentPuzzle()
        {
            return dataModel.ReadData<int>(DataPath.CURPUZZLEID);
        }
        public void SetCurrentPuzzle(int id, Action callback = null)
        {
            dataModel.UpdateData(DataPath.CURPUZZLEID, id, () =>
            {
                callback?.Invoke();
            });
        }

        internal int GetToolScrew()
        {
            return 1000;
        }
        #endregion
    }
}
