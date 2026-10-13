
using UnityEngine;

public class FollowWP : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 12f;
    public float accuracy = 1f;
    public float rotSpeed = 180f;

    [Header("Waypoints")]
    public GameObject wpManager;

    private GameObject[] wps;
    private Transform destination;

    private bool isMoving = false;

    void Start()
    {
        if (wpManager == null)
        {
            Debug.LogError("[TANQUE] WPManager nao atribuido!");
            enabled = false;
            return;
        }

        WPManager manager = wpManager.GetComponent<WPManager>();

        if (manager == null)
        {
            Debug.LogError("[TANQUE] Componente WPManager nao encontrado!");
            enabled = false;
            return;
        }

        wps = manager.waypoints;

        if (wps == null || wps.Length < 5)
        {
            Debug.LogError("[TANQUE] Waypoints invalidos!");
            enabled = false;
            return;
        }

        isMoving = false;

        Debug.Log("[TANQUE] Parado. A aguardar um botao.");
    }


    public void GoToHeli()
    {
        MoveToWaypoint(2); // WP003
    }

    public void GoToRuin()
    {
        MoveToWaypoint(9); // WP010
    }

    public void GoToFactory()
    {
        MoveToWaypoint(3); // WP004
    }

    void MoveToWaypoint(int index)
    {
        if (isMoving)
        {
            Debug.Log("[TANQUE] Ainda esta em movimento.");
            return;
        }

        if (index < 0 || index >= wps.Length ||
            wps[index] == null)
        {
            Debug.LogError("[TANQUE] Waypoint invalido!");
            return;
        }

        // Escolher diretamente o Waypoint do botao
        destination = wps[index].transform;

        isMoving = true;

        Debug.Log("[BOTAO] Destino escolhido: " +
                  destination.name);
    }

    void Update()
    {
        // Sem clique, o tanque fica parado
        if (!isMoving)
            return;

        if (destination == null)
        {
            StopTank();
            return;
        }

        Vector3 targetPosition = destination.position;

        // Manter o movimento no plano horizontal
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        float distance = direction.magnitude;

        // Chegou ao destino escolhido
        if (distance <= accuracy)
        {
            StopTank();
            return;
        }

        // Rodar na direcao do destino
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Avancar apenas quando estiver orientado
        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        if (angle <= 10f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );
        }
    }

    void StopTank()
    {
        isMoving = false;

        Debug.Log("[TANQUE PARADO] Chegou a " +
                  (destination != null ?
                   destination.name : "destino desconhecido"));

        destination = null;

        Debug.Log("[TANQUE] A aguardar outro botao.");
    }
}
