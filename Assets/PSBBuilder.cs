#if UNITY_EDITOR

using Ingame;
using Ingame.Board;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PSB
{
    [RequireComponent(typeof(LayerManager)), RequireComponent(typeof(BaseLevelObject)),
        RequireComponent(typeof(LayerVisibilityController))]
    public class PSBBuilder : MonoBehaviour
    {

        [Header("PSB Import Settings")]
        public int levelIdToImport = 1;
        public ScrewManager screwManager;
        public LayerManager layerManager;
        public BaseLevelObject levelObj;
        private void Awake()
        {
            layerManager = GetComponent<LayerManager>();
            levelObj = GetComponent<BaseLevelObject>();


        }
        public void InitComponent()
        {
            gameObject.AddComponent<BaseLevelObject>();

        }
        public void SpawnScrewManager()
        {
            var screwManagerPrefab = Resources.Load("Prefabs/ScrewManager");
            var screwManagerObj = Instantiate(screwManagerPrefab, Vector3.zero, Quaternion.identity, this.transform);
            screwManager = screwManagerObj.GetComponent<ScrewManager>();
            levelObj.ScrewManager = screwManager;
        }


        public void AddLayer(Transform level)
        {
            int i = 0;
            string name;
            foreach (Transform child in level)
            {
                name = $"Layer {++i}";
                // Chỉ add nếu chưa có
                if (!child.TryGetComponent<BaseLayer>(out _))
                {
                    child.gameObject.AddComponent<BaseLayer>();
                    var layer = child.GetComponent<BaseLayer>();

                    if (layer == null) continue;

                    //layer.layer(name);
                    layer.name = name;
                    layerManager.Layers.Add(layer);
                }

            }
            foreach (Transform child in level)
            {
                AddParts(child);
            }
        }


        public void AddParts(Transform layerTransform)
        {
            var layerComp = layerTransform.GetComponent<BaseLayer>();
            foreach (Transform child in layerTransform)
            {
                child.TryGetComponent<PartLevelMaker>(out var part);
                Debug.Log($"Part in layer {layerTransform} is null {part}");
                if (!part)
                {
                    child.gameObject.AddComponent<PartLevelMaker>();
                    var childPart = child.GetComponent<PartLevelMaker>();
                    layerManager.Parts.Add(childPart);
                    layerComp.parts.Add(childPart);
                    child.gameObject.tag = "Part";
                }
            }
        }

        internal void ImportPSB()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            AddLayer(transform.GetChild(1));

            layerManager = GetComponent<LayerManager>();
            levelObj = GetComponent<BaseLevelObject>();

            GameObjectToLevelConverter.ins.levelObject = this.gameObject;
            GameObjectToLevelConverter.ins.lmanager = this.layerManager;
            LevelMaker.instance.layerDropdown.OnValueChange((v) =>
            {
                this.layerManager.ActivateSingleLayer(v);
            });
            SpawnScrewManager();
        }

        public void AddScrewManagerPrefab()
        {
        }
    }

    [CustomEditor(typeof(PSBBuilder))]
    public class PSBEditorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            PSBBuilder psbEditor = (PSBBuilder)target;

            EditorGUILayout.LabelField("PSB Import Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            // 🔢 Int Input Field
            psbEditor.levelIdToImport = EditorGUILayout.IntField(
                "Level ID",
                psbEditor.levelIdToImport
            );

            EditorGUILayout.Space(10);

            // 🔘 Button
            if (GUILayout.Button("Import PSB"))
            {
                psbEditor.ImportPSB();
            }

            // Mark dirty để save giá trị
            if (GUI.changed)
            {
                EditorUtility.SetDirty(psbEditor);
            }
        }
    }

}
#endif
