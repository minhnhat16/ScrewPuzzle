using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class BYDataBase : ScriptableObject
{
    public virtual void CreateBinaryFile(TextAsset csvText)
    {

    }
}
public class ConfigCompare<T> : IComparer<T> where T : class, new()
{
    private List<FieldInfo> keyInfos = new List<FieldInfo>();
    public ConfigCompare(params string[] keyInfoNames)// ConfigCompareKey("a","b","c")
    {
        for (int i = 0; i < keyInfoNames.Length; i++)
        {
            FieldInfo keyInfo = typeof(T).GetField(keyInfoNames[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            keyInfos.Add(keyInfo);
        }

    }
    public int Compare(T x, T y)
    {
        int result = 0;
        for (int i = 0; i < keyInfos.Count; i++)
        {
            object val_x = keyInfos[i].GetValue(x);
            object val_y = keyInfos[i].GetValue(y);

            // Defensive: handle nulls explicitly to avoid cast exceptions
            if (val_x == null && val_y == null)
            {
                result = 0;
            }
            else if (val_x == null)
            {
                result = -1;
            }
            else if (val_y == null)
            {
                result = 1;
            }
            else
            {
                if (val_x is IComparable cmpX)
                    result = cmpX.CompareTo(val_y);
                else
                    throw new ArgumentException($"Field '{keyInfos[i].Name}' of type '{val_x.GetType()}' does not implement IComparable.");
            }

            if (result != 0)
            {
                break;
            }

        }

        return result;
    }

    public T SetValueSearch(params object[] value)
    {
        T key = new T();
        //Debug.Log("SetValueSearch" + value);
        for (int i = 0; i < value.Length; i++)
        {
            keyInfos[i].SetValue(key, value[i]);
        }
        return key;
    }
}
public abstract class BYDataTable<T> : BYDataBase where T : class, new()
{
    protected ConfigCompare<T> configCompare;
    [SerializeField]
    protected internal List<T> records = new List<T>();
    public abstract ConfigCompare<T> DefineConfigCompare();

    private void OnEnable()
    {
        // Ensure configCompare is assigned when the ScriptableObject is enabled
        configCompare = DefineConfigCompare();
    }

    public override void CreateBinaryFile(TextAsset csvText)
    {
        // Ensure configCompare is assigned before using it
        configCompare = DefineConfigCompare();

        records.Clear();
        List<List<string>> grids = SplitCSVFile(csvText);
        Type recordType = typeof(T);
        FieldInfo[] fieldInfos = recordType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < grids.Count; i++)
        {
            List<string> dataLine = grids[i];
            string jsonText = "{";
            for (int x = 0; x < fieldInfos.Length; x++)
            {
                if (x > 0)
                    jsonText += ",";


                if (fieldInfos[x].FieldType != typeof(string))
                {
                    string dataField = "0";
                    if (x < dataLine.Count)
                    {
                        if (dataLine[x] != string.Empty)
                            dataField = dataLine[x];
                    }
                    jsonText += "\"" + fieldInfos[x].Name + "\":" + dataField;
                }
                else
                {
                    string dataField = string.Empty;
                    if (x < dataLine.Count)
                    {
                        if (dataLine[x] != string.Empty)
                            dataField = dataLine[x];
                    }
                    jsonText += "\"" + fieldInfos[x].Name + "\":\"" + dataField + "\"";
                }
            }
            jsonText += "}";
            // add data record
            Debug.Log(jsonText);
            T r = JsonUtility.FromJson<T>(jsonText);
            records.Add(r);
        }

        // Sort only if a comparer is available. Provide a clear warning otherwise.
        if (configCompare != null)
        {
            try
            {
                records.Sort(configCompare);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BYDataTable] Sorting failed: {ex.Message}");
                // leave unsorted but continue
            }
        }
        else
        {
            Debug.LogWarning("[BYDataTable] DefineConfigCompare() returned null — skipping sort.");
        }
    }
    private List<List<string>> SplitCSVFile(TextAsset csvTest)
    {
        List<List<string>> grids = new List<List<string>>();
        string[] lines = csvTest.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string s = lines[i];
            if (s.CompareTo(string.Empty) != 0)
            {
                string[] lineData = s.Split(',');
                List<string> lsLine = new List<string>();
                foreach (string e in lineData)
                {
                    string newchar = Regex.Replace(e, @"\t|\n|\r", "");
                    newchar = Regex.Replace(newchar, @"""", "\\" + "\\");
                    lsLine.Add(newchar);
                }
                grids.Add(lsLine);
            }
        }
        return grids;
    }
    public List<T> GetAllRecord()
    {
        //Debug.Log($"GET ALLRECORD");
        return records;
    }
    public T GetRecordByKeySearch(params object[] key)
    {
        if (configCompare == null)
        {
            configCompare = DefineConfigCompare();
            if (configCompare == null)
            {
                return null;
            }
        }


        T objectkey = configCompare.SetValueSearch(key);
        int index = records.BinarySearch(objectkey, configCompare);
        //Debug.Log("OBJECT KEY" + objectkey);
        if (index >= 0 && index < records.Count)
            return records[index];
        else
            return null;
    }
}
