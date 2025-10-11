using UnityEngine;

public class SmoothCameraFollowAuto : MonoBehaviour
{
    [Header("Opciones de búsqueda")]
    public string playerTag = "Player"; // usa el tag del player

    [Header("Ajustes de cámara")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    [Range(0.01f, 1f)] public float smoothSpeed = 0.15f;

    [Header("Restricciones opcionales")]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

    private Transform target;

    void Start()
    {
        // intenta encontrar al player al iniciar
        TryFindPlayer();
    }

    void LateUpdate()
    {
        // si aún no existe el player, vuelve a buscarlo (por si spawnea después)
        if (target == null)
        {
            TryFindPlayer();
            return;
        }

        // posición deseada
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // bloqueos opcionales
        if (lockX) smoothedPosition.x = transform.position.x;
        if (lockY) smoothedPosition.y = transform.position.y;
        if (lockZ) smoothedPosition.z = transform.position.z;

        transform.position = smoothedPosition;
        // opcional: seguir mirando al jugador
        // transform.LookAt(target);
    }

    void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }
}