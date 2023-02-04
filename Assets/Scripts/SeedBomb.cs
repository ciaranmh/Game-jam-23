using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBomb : MonoBehaviour
{
    public GameObject treePrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;
        
        if (collision.gameObject.CompareTag("Terrain"))
            Instantiate(treePrefab, transform.position + Vector3.up * 0.8f, Quaternion.identity);
        Destroy(gameObject);
    }
}