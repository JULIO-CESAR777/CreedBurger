using UnityEngine;

public class MesaSeat : MonoBehaviour
{
    [SerializeField] private int reservedBy = 0; // InstanceID del cliente que reservó
    [SerializeField] private bool occupied = false;

    public bool IsOccupied => occupied;
    public int ReservedBy => reservedBy;

    /// <summary>
    /// Intenta reservar esta mesa para un cliente.
    /// True si queda reservada para clientId (si no lo estaba ya por otro).
    /// </summary>
    public bool TryReserve(int clientId)
    {
        if (occupied) return false;               // ya está ocupada (alguien sentado)
        if (reservedBy == 0 || reservedBy == clientId)
        {
            reservedBy = clientId;
            return true;
        }
        return false; // reservada por otro
    }

    /// <summary>
    /// Marca como sentado al cliente que la reservó.
    /// </summary>
    public bool Sit(int clientId)
    {
        if (reservedBy == clientId && !occupied)
        {
            occupied = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Libera reserva/ocupación si pertenece a ese cliente.
    /// </summary>
    public void Release(int clientId)
    {
        if (reservedBy == clientId)
        {
            occupied = false;
            reservedBy = 0;
        }
    }

    /// <summary>
    /// Fuerza liberar (por ejemplo al reiniciar nivel).
    /// </summary>
    public void ForceClear()
    {
        occupied = false;
        reservedBy = 0;
    }
}