using System.Linq;
using UnityEditor;
using UnityEngine;
using static SoundManager;

[CustomEditor(typeof(SoundFactory))]
public class SoundFactoryEditor : Editor
{
    string sfxSearch = "";
    string musicSearch = "";

    SoundManager.SFX newSfx = SoundManager.SFX.NULL;
    Music newMusic = Music.NULL;

    public override void OnInspectorGUI()
    {
        var factory = (SoundFactory)target;

        DrawSFX(factory);

        EditorGUILayout.Space(14);

        DrawMusic(factory);
    }

    // =====================================================
    // SFX
    // =====================================================
    void DrawSFX(SoundFactory factory)
    {
        EditorGUILayout.LabelField("🔊 SFX LIST", EditorStyles.boldLabel);

        sfxSearch = EditorGUILayout.TextField("Search", sfxSearch);
        EditorGUILayout.Space(6);

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

        EditorGUILayout.Space(8);

        for (int i = 0; i < factory.sfxList.Count; i++)
        {
            var entry = factory.sfxList[i];
            if (entry == null) continue;

            if (!string.IsNullOrEmpty(sfxSearch) &&
                !entry.sfx.ToString().ToLower().Contains(sfxSearch.ToLower()))
                continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            var newValue =
                (SoundManager.SFX)EditorGUILayout.EnumPopup(entry.sfx);

            if (newValue != entry.sfx &&
                !factory.sfxList.Any(x => x != entry && x.sfx == newValue))
            {
                Undo.RecordObject(factory, "Change SFX");
                entry.sfx = newValue;
                EditorUtility.SetDirty(factory);
            }

            GUILayout.FlexibleSpace();
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

            entry.audioClip = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip", entry.audioClip, typeof(AudioClip), false);

            entry.timer =
                EditorGUILayout.FloatField("Cooldown", entry.timer);

            entry.timeToDespawn =
                EditorGUILayout.FloatField("Despawn Time", entry.timeToDespawn);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
    }

    // =====================================================
    // MUSIC
    // =====================================================
    void DrawMusic(SoundFactory factory)
    {
        EditorGUILayout.LabelField("🎵 MUSIC LIST", EditorStyles.boldLabel);

        musicSearch = EditorGUILayout.TextField("Search", musicSearch);
        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();
        newMusic = (Music)EditorGUILayout.EnumPopup("Add Music", newMusic);

        using (new EditorGUI.DisabledScope(newMusic == Music.NULL))
        {
            if (GUILayout.Button("Add", GUILayout.Width(80)))
            {
                if (!factory.musicList.Any(m => m.music == newMusic))
                {
                    Undo.RecordObject(factory, "Add Music");
                    factory.musicList.Add(new SoundFactory.Music_SFX
                    {
                        music = newMusic
                    });
                    EditorUtility.SetDirty(factory);
                }
                else
                {
                    Debug.LogWarning($"[SoundFactory] Music {newMusic} already exists.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        for (int i = 0; i < factory.musicList.Count; i++)
        {
            var entry = factory.musicList[i];
            if (entry == null) continue;

            if (!string.IsNullOrEmpty(musicSearch) &&
                !entry.music.ToString().ToLower().Contains(musicSearch.ToLower()))
                continue;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            var newValue =
                (Music)EditorGUILayout.EnumPopup(entry.music);

            if (newValue != entry.music &&
                !factory.musicList.Any(x => x != entry && x.music == newValue))
            {
                Undo.RecordObject(factory, "Change Music");
                entry.music = newValue;
                EditorUtility.SetDirty(factory);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                Undo.RecordObject(factory, "Remove Music");
                factory.musicList.RemoveAt(i);
                EditorUtility.SetDirty(factory);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            entry.audioClip = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip", entry.audioClip, typeof(AudioClip), false);

            //entry.volume =
            //    EditorGUILayout.Slider("Volume", entry.volume, 0f, 1f);

            //entry.loop =
            //    EditorGUILayout.Toggle("Loop", entry.loop);

            //entry.fadeIn =
            //    EditorGUILayout.FloatField("Fade In", entry.fadeIn);

            //entry.fadeOut =
            //    EditorGUILayout.FloatField("Fade Out", entry.fadeOut);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
    }
}
