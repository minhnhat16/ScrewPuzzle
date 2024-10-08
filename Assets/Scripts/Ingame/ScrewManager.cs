using System;
using System.Collections.Generic;
using System.Linq;
using ConfigFile.ConfigFile;
using Level;
using Unity.VisualScripting;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
       [SerializeField] private LayerMask layerMask;
        public LayerMask LayerMask
        {
            get { return layerMask;}
            set { layerMask = value; }
        }
        private void Awake()
        {
           
        }

        private void Start()
        {
            layerMask = LayerMask.GetMask("Screw");
            
        }

        public void AttachScrewsToBoard(BaseWoodBoard baseWoodBoard)
        {
            
        }

        public List<Screw.Screw> GetScrews()
        {
            Screw.Screw[] screwsInChildren = GetComponentsInChildren<Screw.Screw>();
            return screwsInChildren.ToList();
        }

        public void AddScrew(Screw.Screw screw)
        {
        }
        public void AddScrewToConfig(Screw.Screw screw, List<ScrewScriptable> screwScriptable)
        {
            var screwList = GetScrews();

            if (screwScriptable == null) throw new ArgumentNullException(nameof(screwScriptable));

            if (screwList.Count == 0)
            {
                Debug.LogError("Screw list is empty, nothing to save.");
                return;
            }

            // foreach (var s in screwList)
            // {
            //     // Save the screw configuration
            //     var screwConfig = SaveScrewToConfig(s);
            //
            //     // Add the config to the provided list
            //     screwScriptable.Add(screwConfig);
            // }

            Debug.Log("Screw configurations successfully added to the list.");
        }

        // public ScrewScriptable SaveScrewToConfig(Screw.Screw screw)
        // {
        //     ScrewScriptable newScrewScriptable = new ScrewScriptable();
        //     newScrewScriptable.idScrew = screw.GetInstanceID();
        //     newScrewScriptable.screwPosition = screw.Position;
        //     newScrewScriptable.idColor = Convert.ToInt32(screw.Color);
        //     newScrewScriptable.listRigidBodyConnections = screw.HingeController.BodyConnect;
        //     return newScrewScriptable;
        // }
    }
}