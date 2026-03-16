using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class DataTrigger
{
    /// <summary>
    /// Custom Extension method convert path to list path
    /// </summary>
    /// <param spriteName="path"></param>
    /// <returns></returns>
    public static List<string> ConvertToListPath(this string path)
    {
        string[] s = path.Split('/');
        List<string> paths = new List<string>();
        paths.AddRange(s);
        return paths;
    }

    private static Dictionary<string, UnityEvent<object>> dicvalueChange = new Dictionary<string, UnityEvent<object>>();

    public static void RegisterValueChange(string path, UnityAction<object> delegateDataChange)
    {
        if (!dicvalueChange.ContainsKey(path))
        {
            dicvalueChange[path] = new UnityEvent<object>();
        }
        dicvalueChange[path].AddListener(delegateDataChange);
    }

    public static void UnRegisterValueChange(string path, UnityAction<object> delegateDataChange)
    {
        if (dicvalueChange.ContainsKey(path))
        {
            dicvalueChange[path].RemoveListener(delegateDataChange);
        }
    }
    public static void TriggerValueChange(this string path, object data)
    {
        if (dicvalueChange.ContainsKey(path))
        {
            //Debug.Log("TRIGGER VALUE CHANGE" + path);
            dicvalueChange[path].Invoke(data);
        }
    }

    public static string ToKey(this int id)
    {
        return "K_" + id.ToString();
    }

    public static int FromKey(this string key)
    {
        string[] s = key.Split('_');
        return int.Parse(s[1]);
    }
}

public class DataModel : MonoBehaviour
{
    private UserData userData;

    public void InitData(Action callback)
    {
        if (LoadData())
        {
            MigrateData();
            Debug.Log("(BOOT) // INIT DATA DONE");
            callback?.Invoke();
        }
        else
        {
            NewDataForPlayer();
            SaveData();
            Debug.Log("(BOOT) // INIT DATA DONE");
            callback?.Invoke();
        }
    }

    // ─────────────────────────────────────────
    // Auto Migration — patch null fields cho save cũ
    // ─────────────────────────────────────────

    /// <summary>
    /// Tự động scan tất cả public field trong UserData.
    /// Nếu field là reference type và đang null → tạo instance mới bằng Activator.
    /// Nếu field là Dictionary và đang null → tạo Dictionary rỗng.
    /// 
    /// Khi thêm field mới vào UserData:
    ///   1. Khai báo field trong UserData (DataSchema.cs)
    ///   2. Init giá trị trong NewDataForPlayer() (cho new player)
    ///   3. MigrateData() tự động handle cho existing player — KHÔNG CẦN SỬA GÌ THÊM
    /// </summary>
    private void MigrateData()
    {
        if (userData == null) return;

        bool dirty = MigrateObject(userData, "UserData");

        if (dirty)
        {
            SaveData();
            Debug.Log("[DataModel] Migration complete — saved patched data.");
        }
    }

    /// <summary>
    /// Đệ quy scan tất cả public field của một object.
    /// Tạo default instance cho bất kỳ reference field nào đang null.
    /// Trả về true nếu có ít nhất 1 field được patch.
    /// </summary>
    private bool MigrateObject(object target, string parentPath)
    {
        if (target == null) return false;

        bool dirty = false;
        Type type = target.GetType();

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            Type fieldType = field.FieldType;
            string fieldPath = $"{parentPath}.{field.Name}";

            // Skip value types (int, bool, float, enum...) — chúng có default value sẵn
            if (fieldType.IsValueType) continue;

            // Skip string — null string là hợp lệ, không cần migrate
            if (fieldType == typeof(string)) continue;

            object value = field.GetValue(target);

            if (value != null)
            {
                // Field không null — nhưng nếu là nested data class (không phải collection)
                // thì đệ quy vào để check sub-fields
                if (IsUserDataClass(fieldType))
                {
                    if (MigrateObject(value, fieldPath))
                        dirty = true;
                }
                continue;
            }

            // ── Field null → cần patch ─────────────────────────────

            object newInstance = CreateDefaultInstance(fieldType);

            if (newInstance != null)
            {
                field.SetValue(target, newInstance);
                Debug.Log($"[DataModel] Migration: created default for {fieldPath} ({fieldType.Name})");
                dirty = true;
            }
            else
            {
                Debug.LogWarning($"[DataModel] Migration: cannot create default for {fieldPath} ({fieldType.FullName})");
            }
        }

        return dirty;
    }

    /// <summary>
    /// Tạo default instance cho một type.
    /// Hỗ trợ: class có parameterless constructor, Dictionary, List.
    /// </summary>
    private object CreateDefaultInstance(Type type)
    {
        try
        {
            // Dictionary<K,V> → new Dictionary<K,V>()
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return Activator.CreateInstance(type);
            }

            // List<T> → new List<T>()
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return Activator.CreateInstance(type);
            }

            // Class có parameterless constructor → new T()
            if (type.IsClass && !type.IsAbstract)
            {
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    return Activator.CreateInstance(type);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataModel] CreateDefaultInstance failed for {type.FullName}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra xem type có phải là data class trong project (không phải Unity built-in, collection, string...).
    /// Dùng để quyết định có đệ quy MigrateObject vào sub-fields không.
    /// </summary>
    private bool IsUserDataClass(Type type)
    {
        if (type == null || type.IsValueType || type == typeof(string)) return false;
        if (type.IsArray) return false;
        if (type.IsGenericType) return false; // Skip Dictionary, List
        if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine")) return false;
        if (type.Namespace != null && type.Namespace.StartsWith("System")) return false;

        // Chỉ đệ quy vào class có [Serializable] — đây là data class của project
        return type.IsClass && type.IsDefined(typeof(SerializableAttribute), false);
    }

    #region Read Normal

    public T ReadData<T>(string path)
    {
        object outData;
        // using extension method
        List<string> paths = path.ConvertToListPath();
        ReadDataByPath(paths, userData, out outData);
        return (T)outData;
    }

    private void ReadDataByPath(List<string> paths, object data, out object outData)
    {
        outData = null;
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            outData = field.GetValue(data);
        }
        else
        {
            paths.RemoveAt(0);
            ReadDataByPath(paths, field.GetValue(data), out outData);
        }
    }

    #endregion

    #region Read Dictionary

    public T ReadDictionary<T>(string path, string key)
    {
        // using extension method

        List<string> paths = path.ConvertToListPath();
        T outData;
        ReadDataDictionaryByPath(paths, userData, key, out outData);
        return outData;
    }

    private void ReadDataDictionaryByPath<T>(List<string> paths, object data, string key, out T dataOut)
    {
        dataOut = default;

        if (data == null)
        {
            Debug.LogError("[DataModel] ReadDataDictionaryByPath: data is null for path segment '" + paths[0] + "'.");
            return;
        }

        string p = paths[0];
        Type t = data.GetType();
        // Use explicit BindingFlags so private / serialized fields are found too
        FieldInfo field = t.GetField(p, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null)
        {
            Debug.LogError($"[DataModel] ReadDataDictionaryByPath: field '{p}' not found on type '{t.FullName}'. Check DataPath and UserData field names (case-sensitive).");
            return;
        }

        if (paths.Count == 1)
        {
            object dic = field.GetValue(data);

            // If the dictionary is null, return default
            if (dic == null)
            {
                dataOut = default;
                return;
            }

            // Support dictionaries keyed by string or int.
            if (dic is Dictionary<string, T> dicString)
            {
                dicString.TryGetValue(key, out dataOut);
                return;
            }
            else if (dic is Dictionary<int, T> dicInt)
            {
                if (int.TryParse(key, out int ik))
                {
                    dicInt.TryGetValue(ik, out dataOut);
                }
                else
                {
                    dataOut = default;
                }
                return;
            }
            else
            {
                // Handle unsupported key types or unexpected runtime type
                if (field.FieldType.IsGenericType)
                {
                    var keyType = field.FieldType.GetGenericArguments()[0];
                    if (keyType == typeof(string) || keyType == typeof(int))
                    {
                        dataOut = default;
                        return;
                    }
                }

                dataOut = default;
                Debug.LogError($"[DataModel] ReadDictionary: field '{p}' is not a supported dictionary type. Actual runtime type: {dic?.GetType().FullName}");
                return;
            }
        }
        else
        {
            paths.RemoveAt(0);
            var nextData = field.GetValue(data);
            ReadDataDictionaryByPath(paths, nextData, key, out dataOut);
        }
    }

    #endregion

    #region Update Normal

    public void UpdateData(string path, object newData, Action callback = null)
    {
        // using extension method
        List<string> paths = path.ConvertToListPath();
        UpdateDataByPath(paths, userData, newData, callback);
        path.TriggerValueChange(newData);
        SaveData();
    }

    private void UpdateDataByPath(List<string> paths, object data, object newData, Action callback)
    {
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            field.SetValue(data, newData);
            callback?.Invoke();
        }
        else
        {
            paths.RemoveAt(0);
            UpdateDataByPath(paths, field.GetValue(data), newData, callback);
        }
    }

    #endregion

    #region Update Dictionary

    public void UpdateDataDictionary<T>(string path, string key, T newData, Action callback = null)
    {
        List<string> paths = path.ConvertToListPath();
        object dicDataOut;
        UpdateDataDictionaryByPath<T>(paths, key, userData, newData, out dicDataOut, callback);
        (path + "/" + key).TriggerValueChange(newData);
        path.TriggerValueChange(dicDataOut);
        SaveData();
    }

    private void UpdateDataDictionaryByPath<T>(List<string> paths, string key, object data, T newData, out object dataOut, Action callback)
    {
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            object dic = field.GetValue(data);

            // If dictionary instance exists, handle by actual runtime type
            if (dic is Dictionary<string, T> dicString)
            {
                if (dicString == null)
                    dicString = new Dictionary<string, T>();
                dicString[key] = newData;
                dataOut = dicString;
                field.SetValue(data, dicString);

                Debug.Log("Update dictionary string key " + key + " with value " + newData.ToString());
                callback?.Invoke();
                return;
            }
            else if (dic is Dictionary<int, T> dicInt)
            {
                if (dicInt == null)
                    dicInt = new Dictionary<int, T>();

                if (int.TryParse(key, out int ik))
                {
                    dicInt[ik] = newData;
                    dataOut = dicInt;
                    field.SetValue(data, dicInt);
                    callback?.Invoke();
                }
                else
                {
                    dataOut = dicInt;
                    Debug.LogError($"[DataModel] UpdateDataDictionary: key '{key}' is not a valid int for dictionary '{p}'.");
                }
                return;
            }
            else
            {
                // If field is null or not assigned, create appropriate dictionary based on declared field type
                if (field.FieldType.IsGenericType)
                {
                    var genericArgs = field.FieldType.GetGenericArguments();
                    var keyType = genericArgs[0];

                    if (keyType == typeof(string))
                    {
                        var newDic = new Dictionary<string, T>();
                        newDic[key] = newData;
                        dataOut = newDic;
                        field.SetValue(data, newDic);
                        callback?.Invoke();
                        return;
                    }
                    else if (keyType == typeof(int))
                    {
                        var newDic = new Dictionary<int, T>();
                        if (int.TryParse(key, out int ik))
                        {
                            newDic[ik] = newData;
                            dataOut = newDic;
                            field.SetValue(data, newDic);
                            callback?.Invoke();
                        }
                        else
                        {
                            dataOut = newDic;
                            Debug.LogError($"[DataModel] UpdateDataDictionary: key '{key}' is not a valid int for dictionary '{p}'.");
                        }
                        return;
                    }
                }

                dataOut = null;
                Debug.LogError($"[DataModel] UpdateDataDictionary: field '{p}' is not a supported dictionary type. Actual type: {dic?.GetType().FullName}");
                return;
            }
        }
        else
        {
            paths.RemoveAt(0);
            UpdateDataDictionaryByPath<T>(paths, key, field.GetValue(data), newData, out dataOut, callback);
        }
    }

    #endregion

    private void SaveData()
    {
        string json_string = JsonConvert.SerializeObject(userData);
        PlayerPrefs.SetString("DATA", json_string);
        PlayerPrefs.Save(); // ← flush xuống disk ngay lập tức
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Khi app bị minimize / switch task → đảm bảo data đã ghi
        if (pauseStatus)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private bool LoadData()
    {
        if (PlayerPrefs.HasKey("DATA"))
        {
            string json_string = PlayerPrefs.GetString("DATA");
            //Debug.Log("(DATA) // LOAD DATA: " + json_string);
            userData = JsonConvert.DeserializeObject<UserData>(json_string);
            return true;
        }
        return false;
    }
    private void NewDataForPlayer()
    {
        Debug.Log("(BOOT) // CREATE NEW DATA");
        userData = new UserData();

        // ===============================
        // USER INFO
        // ===============================
        userData.userInfo = new UserInfo
        {
            name = ZenSDK.instance.GetConfigString("userName", "player"),
            isNewPlayer = true
        };
        userData.music = true;
        userData.sfx = true;
        // ===============================
        // INVENTORY (ITEMS)
        // ===============================
        userData.itemInventory = new ItemInvent();
        userData.itemInventory.itemDict = new Dictionary<string, ItemData>();

        AddNewItem(ItemType.Magnet, ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 0));
        AddNewItem(ItemType.Breaker, ZenSDK.instance.GetConfigInt(ItemType.Breaker.ToString(), 5));
        AddNewItem(ItemType.Drill, ZenSDK.instance.GetConfigInt(ItemType.Drill.ToString(), 5));
        userData.currentPuzzleID = 1;
        // ===============================
        // LEVEL DATA
        // ===============================
        userData.levelInfo = new LevelInfo
        {
            expLevel = 0f,
            bonusCount = 0,
            currentLevel = 1,
            levelData = new List<LevelData>
        {
            new LevelData
            {
                levelID = 1,
                levelStar = 0,
                isCompleted = true
            }
        }
        };

        // ===============================
        // WALLET
        // ===============================
        userData.wallet = new Wallet();

        userData.wallet.goldWallet = new CurrencyWallet
        {
            currency = Currency.Gold,
            amount = ZenSDK.instance.GetConfigInt(Currency.Gold.ToString(), 100000)
        };

        userData.wallet.ticketWallet = new CurrencyWallet
        {
            currency = Currency.Ticket,
            amount = ZenSDK.instance.GetConfigInt(Currency.Ticket.ToString(), 1)
        };

        // ===============================
        // DAILY DATA
        // ===============================
        userData.dailyData = new DailyData
        {
            isClaimToday = false,
            timeClaimed = DateTime.MinValue.ToString(),
            dailyList = new List<DailyItemData>()
        };

        for (int i = 0; i < 7; i++)
        {
            userData.dailyData.dailyList.Add(new DailyItemData
            {
                day = i + 1,
                currentType = (i == 0 ? DailyType.Available : DailyType.Unavailable)
            });
        }

        // ===============================
        // COLLECTION DATA
        // ===============================
        userData.collectionData = new CollectionData
        {
            currentBG = new BackGroundData { isUnlocked = true, name = "07_Shape_mini board" },
            currentBoard = new BoardColorData { isUnlocked = true, name = "18_Screw_mini board" },
            currentScrew = new ScrewSkinData { isUnlocked = true, name = "19_Screw_mini board" },

            backGroundDict = new Dictionary<string, BackGroundData>(),
            boardColorDict = new Dictionary<string, BoardColorData>(),
            screwColorDict = new Dictionary<string, ScrewSkinData>()
        };

        for (int i = 0; i < 5; i++)
        {
            userData.collectionData.backGroundDict.Add($"Background_{i + 1}", new BackGroundData());
            userData.collectionData.boardColorDict.Add($"BoardColor_{i + 1}", new BoardColorData());
            userData.collectionData.screwColorDict.Add($"ScrewColor_{i + 1}", new ScrewSkinData());
        }

        // ===============================
        // SPIN DATA
        // ===============================
        userData.spinData = new SpinData
        {
            isSpin = false,
            timeSpin = DateTime.MinValue.ToString()
        };

        // ===============================
        // INIT MISSION PROGRESS
        // ===============================
        userData.missions = new Dictionary<string, MissionProgress>();

        foreach (var mission in MissionManager.ins.GetActiveMissions())
        {
            var mp = new MissionProgress
            {
                missionId = mission.Id,
                current = 0,
                target = mission.Target,
                state = MissionState.NotStarted,
                rewardClaimed = false,
                startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            userData.missions[mission.Id.ToString()] = mp;
        }

        // ===============================
        // INIT STAGE PROGRESS
        // ===============================
        userData.stageProgress = new Dictionary<int, StageProgress>();

        // Stage 0 always unlocked
        userData.stageProgress[0] = new StageProgress
        {
            stageId = 0,
            isUnlocked = true,
            isCompleted = false,
            rewardClaimed = false,
            chestProgress = 0
        };

        // Next stages locked
        int stageCount = ConfigExtensions.GetStageCount();
        for (int i = 1; i < stageCount; i++)
        {
            userData.stageProgress[i] = new StageProgress
            {
                stageId = i,
                isUnlocked = false,
                isCompleted = false,
                rewardClaimed = false,
                chestProgress = 0
            };
        }

        // ===============================
        // INIT CHEST STATE
        // ===============================
        userData.chestStates = new Dictionary<int, ChestStageData>();

        var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>().GetAllRecord();
        foreach (var chest in chestConfig)
        {
            userData.chestStates[chest.Id] = new ChestStageData
            {
                chestId = chest.Id,
                isUnlocked = false,
                isClaimed = false,
                progress = 0
            };
        }
        var puzzleBlockDatas = new Dictionary<int, BlockData>();

        for (int i = 0; i < 25; i++)
        {
            var puzzleBlockData = new BlockData
            {
                screwRequired = 0,
                unlocked = false,
                removedCells = new Dictionary<int, bool>()
            };
            puzzleBlockDatas.Add(i, puzzleBlockData);
        }
        userData.puzzleBlockData = puzzleBlockDatas;

        userData.timeMeta = new TimeSaveMeta
        {
            lastResetUtcTicks = DateTime.MinValue.Ticks
        };

        userData.specialData = new Special
        {
            currentSpecial = 0,
            targetSpecial = 15
        };

        userData.sideMissionDaily = new SideMissionDailyData
        {
            completedToday = 0,
            lastResetDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        Debug.Log("Time save meta " + userData.timeMeta.lastResetUtcTicks);
        Debug.Log("(BOOT) // NEW PLAYER DATA CREATED");
    }

    private void AddNewItem(ItemType type, int defaultAmount)
    {
        ItemData item = new ItemData
        {
            type = type,
            total = defaultAmount
        };

        userData.itemInventory.itemDict.Add(type.ToString(), item);
    }


    private void NewDataForTester()
    {
        Debug.Log("(BOOT) // CREATE NEW DATA");
        userData = new UserData();
        UserInfo inf = new UserInfo();
        inf.name = ZenSDK.instance.GetConfigString("userName", "player");
        inf.isNewPlayer = true;
        userData.userInfo = inf;

        ///item
        userData.itemInventory = new();
        Dictionary<string, ItemData> newItemDict = new();

        ItemData newAddHoldInvent = new();
        newAddHoldInvent.type = ItemType.Magnet;
        newAddHoldInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 5);
        var key = newAddHoldInvent.type.ToString();
        newItemDict.Add(key, newAddHoldInvent);

        ItemData newAddBoxInvent = new();
        newAddBoxInvent.type = ItemType.Breaker;
        newAddBoxInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Breaker.ToString(), 5);
        key = newAddBoxInvent.type.ToString();
        newItemDict.Add(key, newAddBoxInvent);

        ItemData newClearOnScrewInvent = new();
        newClearOnScrewInvent.type = ItemType.Drill;
        newClearOnScrewInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Drill.ToString(), 5);
        key = newClearOnScrewInvent.type.ToString();
        newItemDict.Add(key, newClearOnScrewInvent);

        userData.itemInventory.itemDict = new();
        userData.itemInventory.itemDict = newItemDict;

        //player level
        LevelInfo levelInf = new();
        levelInf.expLevel = 0.0f;
        LevelData dataLevelOne = new();
        dataLevelOne.levelID = 1;
        dataLevelOne.levelStar = 0;
        dataLevelOne.isCompleted = false;
        levelInf.currentLevel = 1;
        levelInf.levelData.Add(dataLevelOne);

        userData.levelInfo = levelInf;
        userData.wallet = new();
        //Add gold 
        CurrencyWallet goldWallet = new();
        goldWallet.currency = Currency.Gold;
        goldWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Gold.ToString(), 100000000);
        userData.wallet.goldWallet = goldWallet;

        //Add gem 
        CurrencyWallet gemWallet = new();
        gemWallet.currency = Currency.Ticket;
        gemWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Ticket.ToString(), 10000000);
        userData.wallet.ticketWallet = gemWallet;
        DailyData newDaily = new();
        newDaily.isClaimToday = false;
        newDaily.timeClaimed = DateTime.MinValue.ToString();
        List<DailyItemData> _dailyData = new();
        for (int i = 0; i < 7; i++)
        {
            DailyItemData dailyData = new DailyItemData();
            dailyData.day = i + 1;
            DailyType iEDailyType = i == 0 ? DailyType.Available : DailyType.Unavailable;
            dailyData.currentType = iEDailyType;
            _dailyData.Add(dailyData);
        }
        newDaily.dailyList = _dailyData;
        userData.dailyData = newDaily;
        SpinData newSpinData = new();
        newSpinData.isSpin = false;
        newSpinData.timeSpin = DateTime.MinValue.ToString();
        userData.spinData = newSpinData;
    }

    public void DeleteData(string path)
    {
        List<string> paths = path.ConvertToListPath();
        DeleteDataByPath(paths, userData);
    }
    private void DeleteDataByPath(List<string> paths, object data)
    {
        if (data == null || paths == null || paths.Count == 0)
            return;

        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (field == null)
            return;

        // ===== DELETE HERE =====
        if (paths.Count == 1)
        {
            // reference type → null
            if (!field.FieldType.IsValueType)
            {
                field.SetValue(data, null);
            }
            else
            {
                // value type → default(T)
                object defaultValue = Activator.CreateInstance(field.FieldType);
                field.SetValue(data, defaultValue);
            }

            return;
        }

        // ===== GO DEEPER =====
        object next = field.GetValue(data);
        if (next == null)
            return;

        paths.RemoveAt(0);
        DeleteDataByPath(paths, next);
    }

}