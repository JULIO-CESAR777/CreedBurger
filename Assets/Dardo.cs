using System;
using UnityEngine;

public class Dardo : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        
        if (other.gameObject.tag == "Cliente")
        {
            //Comportamiento del cliente    
        }
        Destroy(gameObject);
    }
}
