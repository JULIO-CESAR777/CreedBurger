using UnityEngine;

public interface ITrap
{
    public void Use(Transform position);
}

public enum TrapType
{
    Cerbatana,
    Bomb
}
