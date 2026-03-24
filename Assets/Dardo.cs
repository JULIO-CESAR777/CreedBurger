using System;
using UnityEngine;

public class Dardo : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        print("Choco con algo");
        if (other.gameObject.tag == "Cliente")
        {
            
        }
        //Destroy(gameObject);
    }
}
