using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class BearTrap : MonoBehaviour
{
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
