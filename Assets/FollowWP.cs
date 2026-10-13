
using System.Collections.Generic;
using UnityEngine;

public class FollowWP : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 12f;
    public float accuracy = 1f;
    public float rotSpeed = 150f;

    [Header("Curvas")]
    [Range(0.05f, 1f)]
    public float minTurnSpeed = 0.1f;

    [Header("Waypoints")]
    public GameObject wpManager;

    private GameObject[] wps;
    private Graph graph;

    private List<Node> currentPath = new List<Node>();

    private int currentWP = 0;

    private GameObject currentNode;
    private GameObject destination;

    private bool isMoving = false;

    void Start()
    {
        if (wpManager == null)
        {
            Debug.LogError(
                "[TANQUE] WPManager nao atribuido!"
            );

            enabled = false;
            return;
        }

        WPManager manager =
            wpManager.GetComponent<WPManager>();

        if (manager == null)
        {
            Debug.LogError(
                "[TANQUE] Componente WPManager nao encontrado!"
            );

            enabled = false;
            return;
        }

        wps = manager.waypoints;
        graph = manager.graph;

        if (graph == null ||
            wps == null ||
            wps.Length < 10)
        {
            Debug.LogError(
                "[TANQUE] Graph ou Waypoints invalidos!"
            );

            enabled = false;
            return;
        }

        // O tanque comeca associado ao WP001.
        currentNode = wps[0];

        if (currentNode == null)
        {
            Debug.LogError(
                "[TANQUE] WP001 nao atribuido!"
            );

            enabled = false;
            return;
        }

        isMoving = false;

        Debug.Log(
            "[TANQUE] Parado. A aguardar ordem."
        );
    }

    // HELIPAD - WP003
    public void GoToHeli()
    {
        MoveToWaypoint(2);
    }

    // RUINS - WP010
    public void GoToRuin()
    {
        MoveToWaypoint(9);
    }

    // FACTORY - WP004
    public void GoToFactory()
    {
        MoveToWaypoint(3);
    }

    void MoveToWaypoint(int index)
    {
        if (isMoving)
        {
            Debug.Log(
                "[TANQUE] Aguarda a chegada ao destino."
            );

            return;
        }

        if (wps == null ||
            index < 0 ||
            index >= wps.Length ||
            wps[index] == null)
        {
            Debug.LogError(
                "[TANQUE] Waypoint invalido!"
            );

            return;
        }

        destination = wps[index];

        Debug.Log(
            "[BOTAO] Destino escolhido: " +
            destination.name
        );

        // Calcular um novo percurso utilizando A*
        bool found = graph.AStar(
            currentNode,
            destination
        );

        if (!found)
        {
            Debug.LogWarning(
                "[A*] Nao existe percurso entre " +
                currentNode.name +
                " e " +
                destination.name
            );

            destination = null;
            return;
        }

        // Copiar o percurso calculado pelo A*
        currentPath = new List<Node>(graph.pathList);

        if (currentPath.Count == 0)
        {
            Debug.LogWarning(
                "[A*] O percurso esta vazio!"
            );

            destination = null;
            return;
        }

        currentWP = 0;

        // Se ja estiver perto do primeiro Waypoint,
        // avancar diretamente para o seguinte.
        GameObject firstWaypoint =
            currentPath[0].getId();

        Vector3 firstPosition =
            firstWaypoint.transform.position;

        firstPosition.y = transform.position.y;

        if (Vector3.Distance(
            transform.position,
            firstPosition
        ) <= accuracy)
        {
            currentWP = 1;
        }

        isMoving = true;

        Debug.Log(
            "[A*] Percurso escolhido com " +
            currentPath.Count +
            " Waypoints."
        );

        for (int i = 0; i < currentPath.Count; i++)
        {
            Debug.Log(
                "[A*] " +
                i +
                ": " +
                currentPath[i].getId().name
            );
        }

        // Se a origem ja for o destino
        if (currentWP >= currentPath.Count)
        {
            StopTank();
        }
    }

    void Update()
    {
        if (!isMoving)
            return;

        if (currentWP >= currentPath.Count)
        {
            StopTank();
            return;
        }

        GameObject waypoint =
            currentPath[currentWP].getId();

        if (waypoint == null)
        {
            Debug.LogError(
                "[TANQUE] Waypoint do percurso invalido!"
            );

            StopTank();
            return;
        }

        Vector3 targetPosition =
            waypoint.transform.position;

        // Movimento apenas no plano horizontal
        targetPosition.y = transform.position.y;

        Vector3 direction =
            targetPosition - transform.position;

        float distance = direction.magnitude;

        // Verificar chegada ao Waypoint atual
        if (distance <= accuracy)
        {
            // Guardar a ligacao efetivamente percorrida
            if (currentNode != null &&
                currentNode != waypoint)
            {
                graph.RegisterTraversal(
                    currentNode,
                    waypoint
                );
            }

            // Atualizar a posicao logica do tanque
            currentNode = waypoint;

            Debug.Log(
                "[CHEGADA] " +
                waypoint.name
            );

            // Avancar para o proximo Waypoint do percurso
            currentWP++;

            // Parar apenas no destino final
            if (currentWP >= currentPath.Count)
            {
                StopTank();
            }

            return;
        }

        // Rodar gradualmente para o Waypoint
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotSpeed * Time.deltaTime
            );

        // Calcular o angulo entre a frente do tanque
        // e a direcao do Waypoint
        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        // Reduzir a velocidade nas curvas apertadas
        float turnFactor = Mathf.Lerp(
            minTurnSpeed,
            1f,
            1f - angle / 180f
        );

        float currentSpeed =
            speed * turnFactor;

        // Avancar na direcao para onde o tanque aponta
        float step = Mathf.Min(
            currentSpeed * Time.deltaTime,
            distance
        );

        transform.position +=
            transform.forward * step;
    }

    void StopTank()
    {
        isMoving = false;

        currentWP = 0;
        currentPath.Clear();

        Debug.Log(
            "[TANQUE PARADO] Destino: " +
            (destination != null
                ? destination.name
                : "desconhecido")
        );

        destination = null;

        Debug.Log(
            "[TANQUE] A aguardar outro botao."
        );
    }
}
