using System;
using UnityEngine;

public class Dardo : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        
        if (other.gameObject.tag == "Cliente")
        {
            MoveClient cliente = other.gameObject.GetComponentInParent<MoveClient>();

            if (cliente != null)
            {
                cliente.Aturdir(2f); 
            }
        }
        Destroy(gameObject);
    }
}
