using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestDialog : BaseDialog
{
    public Transform missionListParent;
    public GameObject missionItemPrefab;

    private List<MissionConfigRecord> missionRecords;

    void OnEnable()
    {
        LoadMissions();
    }

    void LoadMissions()
    {
        missionRecords = MissionManager.ins.GetActiveMissions();

        foreach (Transform child in missionListParent)
            Destroy(child.gameObject);

        foreach (var mission in missionRecords)
        {
            var item = Instantiate(missionItemPrefab, missionListParent);
            int progress = MissionManager.ins.GetProgress(mission.Id);
            item.GetComponent<MissionItemUI>().Setup(mission, progress);
        }
    }
}
