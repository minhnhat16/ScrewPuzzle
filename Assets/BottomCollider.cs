using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCollider : MonoBehaviour
{
    public Collider2D bottomCollider;
    // Start is called before the first frame update
    void Start()
    {
        bottomCollider = GetComponentInChildren<BoxCollider2D>();
        bottomCollider.isTrigger = true;
    }
    
    // Update is called once per frame
    void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        var gameObject = other.GetComponent<GameObject>();
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }
}
