using UnityEngine;

public class MoveClientTutorial : MonoBehaviour
{
    // ✅ Evento para el TutorialManager
    public static event System.Action<MoveClientTutorial, int> OnClientReceivedRecipe;

    [Header("Receta / UI")]
    public IngredientPrefabDB db;
    [Tooltip("-1 = aleatorio; >=0 fuerza ese ID de la DB")]
    public int idPedidoForzado = -1;

    [Header("Cuándo pedir")]
    public bool pedirAlIniciar = true;

    // Pedido actual
    private IngredientPrefabDB.Entry pedido;
    private bool pedidoEnviado = false;
    private bool pedidoListo = false;

    private ClientOrderIndicator indicator;

    void Awake()
    {
        indicator = GetComponent<ClientOrderIndicator>();
    }

    void Start()
    {
        if (pedirAlIniciar)
            HacerPedido();
    }

    public int GetPedidoId() => pedido.id;

    public void HacerPedido()
    {
        if (db == null || db.entries == null || db.entries.Count == 0)
        {
            Debug.LogWarning("[MoveClientTutorial] DB vacía o no asignada.");
            return;
        }

        if (idPedidoForzado >= 0 && db.TryGetById(idPedidoForzado, out var forced))
            pedido = forced;
        else
            db.TryGetRandom(out pedido);

        pedidoEnviado = true;
        pedidoListo = false;

        // UI lista de pedidos (tu OrderUIController)
        if (OrderUIController.Instance != null && pedido.image != null)
            OrderUIController.Instance.AddOrder(GetInstanceID(), pedido.image);

        // Iconito sobre el cliente (opcional)
        if (indicator != null && pedido.image != null)
            indicator.Show(pedido.image);

        Debug.Log($"[MoveClientTutorial] Pedido creado ID={pedido.id} (cliente {name})");
    }

    /// <summary>
    /// Llama esto cuando el chef/Player le entregue una receta (ID).
    /// </summary>
    public void RecibirPedido(int idDelChef)
    {
        if (!pedidoEnviado || pedidoListo) return;

        if (pedido.id != idDelChef)
        {
            Debug.Log($"[MoveClientTutorial] Pedido incorrecto. Esperado={pedido.id}, entregado={idDelChef}");
            return;
        }

        // ✅ Correcto
        pedidoListo = true;

        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());

        if (indicator != null)
            indicator.Hide();

        OnClientReceivedRecipe?.Invoke(this, idDelChef);

        Debug.Log($"[MoveClientTutorial] Pedido correcto entregado ID={idDelChef} (cliente {name})");
    }

    void OnDestroy()
    {
        // Limpieza por si destruyes al cliente
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());
    }
}