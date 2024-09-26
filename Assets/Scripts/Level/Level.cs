using UnityEngine;
using System.Collections.Generic;
using ConfigFile;
using Enum;
using Ingame.Screw;
using UnityEngine.Serialization;

namespace Level
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level")]
    public class Level : ScriptableObject
    {
        public Vector3[] hingesPosition;  // Vị trí của các điểm hinges
        public int totalColorInLevel;
        public LayerEnum layer;
        public BoxConfig connectors ;  
        public List<ScrewScriptable> screws;        // Danh sách các Screw
        public List<BodyPartScriptable> bodyParts;  // Danh sách các Body Part
        
    }
    // Dummy class cho Screw, bạn có thể thay đổi nội dung tùy theo dự án
    [System.Serializable]
    public class ScrewScriptable
    {
        public int idScrew;
        public int  idColor;
        public string screwName;
        public Vector3 screwPosition;
        public List<Rigidbody2D> listRigidBodyConnections;
        public List<HingeJoint2D> listHingeJoint;
        
    }
    // Dummy class cho Body Part, bạn có thể thay đổi nội dung tùy theo dự án
    [System.Serializable]
    public class BodyPartScriptable
    {
        public int idBodyPart;
        public string partName;
        public Vector3 partPosition;
        public LayerEnum layer;
    }
}