using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class BearTrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
    
        if (other.CompareTag("Client"))
        {
            var cliente = other.GetComponent<MoveClient>();
            if (cliente == null) return;
            cliente.Aturdir(5);
            Destroy(gameObject);
        }

       

    }



    

}
