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
        bottomCollider.isTrigger = true; // Ensure the collider is set as a trigger
    }

    // This method is called when another collider enters the trigger collider attached to this game object
    void OnTriggerEnter2D(Collider2D other)
    {
        // Set the game object of the collider that entered the trigger to inactive
        StartCoroutine(WaitToSetCollider(other,0f));
    }

    IEnumerator WaitToSetCollider( Collider2D other, float time)
    {
        yield return new WaitForSeconds(time);
        other.gameObject.SetActive(false);

    }
}