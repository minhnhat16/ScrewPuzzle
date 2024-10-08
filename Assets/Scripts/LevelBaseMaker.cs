using System.Collections;
using System.Collections.Generic;
using Ingame;
using UnityEngine;

public class LevelBaseMaker : BaseLevelObject
{

    [SerializeField] private ScrewManager screwManager;

    public ScrewManager ScrewManager
    {
        get => screwManager;
        set => screwManager = value;
    }


    // Start is called before the first frame update
    void Start()
    {
        ScrewManager = GetComponentInChildren<ScrewManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
