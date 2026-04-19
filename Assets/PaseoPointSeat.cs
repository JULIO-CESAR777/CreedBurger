using UnityEngine;

public class PaseoPointSeat : MonoBehaviour
{
    private int reservedBy = int.MinValue;

    public bool TryReserve(int clientId)
    {
        if (reservedBy == int.MinValue || reservedBy == clientId)
        {
            reservedBy = clientId;
            return true;
        }

        return false;
    }

    public void Release(int clientId)
    {
        if (reservedBy == clientId)
            reservedBy = int.MinValue;
    }

    public bool IsReserved => reservedBy != int.MinValue;
}