using System;
using UnityEngine;

public class DardTrap : MonoBehaviour, ITrap
{

    [SerializeField] private GameObject DardObj;
    public TrapType type;

    private void Start()
    {
        type = TrapType.Cerbatana;
    }

    public void Use(Transform ShootingPoint)
    {
        GameObject dard = Instantiate(DardObj, ShootingPoint.position, ShootingPoint.rotation);
        dard.GetComponent<Rigidbody>().AddForce(ShootingPoint.forward * 1000);
        print("dardo");
    }
    
}
