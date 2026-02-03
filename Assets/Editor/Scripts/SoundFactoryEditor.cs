using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(SoundFactory))]
public class SoundFactoryEditor : Editor
{
    private string search = "";
    private SoundManager.SFX newSfx = SoundManager.SFX.NULL;

    public override void OnInspectorGUI()
    {
        var factory = (SoundFactory)target;

        EditorGUILayout.LabelField("SFX List", EditorStyles.boldLabel);

        // =========================
        // SEARCH
        // =========================
        search = EditorGUILayout.TextField("Search", search);
        EditorGUILayout.Space(6);

        // =========================
        // ADD NEW SFX
        // =========================
        EditorGUILayout.BeginHorizontal();
        newSfx = (SoundManager.SFX)EditorGUILayout.EnumPopup("Add SFX", newSfx);

        using (new EditorGUI.DisabledScope(newSfx == SoundManager.SFX.NULL))
        {
            if (GUILayout.Button("Add", GUILayout.Width(80)))
            {
                if (!factory.sfxList.Any(s => s.sfx == newSfx))
                {
                    Undo.RecordObject(factory, "Add SFX");

                    factory.sfxList.Add(new SoundFactory.Sound_SFX
                    {
                        sfx = newSfx,
                        timer = 0f,
                        timeToDespawn = 1f,
                        audioClip = null
                    });

                    EditorUtility.SetDirty(factory);
                }
                else
                {
                    Debug.LogWarning($"[SoundFactory] SFX {newSfx} already exists.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // =========================
        // DRAW LIST
        // =========================
        for (int i = 0; i < factory.sfxList.Count; i++)
        {
            var entry = factory.sfxList[i];
            if (entry == null) continue;

            // search filter
            if (!string.IsNullOrEmpty(search) &&
                !entry.sfx.ToString().ToLower().Contains(search.ToLower()))
                continue;

            EditorGUILayout.BeginVertical("box");

            // -------- HEADER --------
            EditorGUILayout.BeginHorizontal();

            SoundManager.SFX newValue =
                (SoundManager.SFX)EditorGUILayout.EnumPopup(entry.sfx);

            // validate enum change
            if (newValue != entry.sfx)
            {
                bool duplicate = factory.sfxList.Any(
                    x => x != entry && x.sfx == newValue
                );

                if (!duplicate)
                {
                    Undo.RecordObject(factory, "Change SFX Enum");
                    entry.sfx = newValue;
                    EditorUtility.SetDirty(factory);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"SFX {newValue} already exists!",
                        MessageType.Warning
                    );
                }
            }

            GUILayout.FlexibleSpace();

            // remove button
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                Undo.RecordObject(factory, "Remove SFX");
                factory.sfxList.RemoveAt(i);
                EditorUtility.SetDirty(factory);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // -------- FIELDS --------
            Undo.RecordObject(factory, "Edit SFX Entry");

            entry.audioClip =
                (AudioClip)EditorGUILayout.ObjectField(
                    "Audio Clip",
                    entry.audioClip,
                    typeof(AudioClip),
                    false
                );

            entry.timer =
                EditorGUILayout.FloatField("Cooldown", entry.timer);

            entry.timeToDespawn =
                EditorGUILayout.FloatField("Despawn Time", entry.timeToDespawn);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
    }
}
