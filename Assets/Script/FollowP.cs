
using UnityEngine;
using System.Collections.Generic;

public class FolloWP : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 12f;
    public float accuracy = 1.5f;
    public float rotSpeed = 180f;

    [Header("Waypoints")]
    public GameObject wpManager;

    [Header("Exploracao pseudoaleatoria")]
    [Min(0)]
    public int minExplorationSteps = 2;

    [Min(0)]
    public int maxExplorationSteps = 5;

    [Range(0f, 1f)]
    public float destinationChance = 0.25f;

    private GameObject[] wps;
    private GameObject currentNode;
    private GameObject previousNode;
    private GameObject destination;

    private Graph g;

    private List<Node> currentPath = new List<Node>();

    private int currentWP = 0;
    private int explorationSteps = 0;
    private int explorationTarget = 0;

    private bool isMoving = false;
    private bool goingToFinalDestination = false;

    void Start()
    {
        Debug.Log("FOLLOWWP ATIVO NO OBJETO: " + gameObject.name);

        if (wpManager == null)
        {
            Debug.LogError("WPManager nao atribuido!");
            enabled = false;
            return;
        }

        WPManager manager = wpManager.GetComponent<WPManager>();

        if (manager == null)
        {
            Debug.LogError("Componente WPManager nao encontrado!");
            enabled = false;
            return;
        }

        wps = manager.waypoints;
        g = manager.graph;

        if (g == null || wps == null || wps.Length == 0)
        {
            Debug.LogError("Graph ou Waypoints invalidos!");
            enabled = false;
            return;
        }

        currentNode = wps[0];

        if (currentNode == null)
        {
            Debug.LogError("Waypoint inicial invalido!");
            enabled = false;
            return;
        }

        isMoving = false;

        Debug.Log("[TANQUE] Parado. A aguardar ordem.");
    }

    // Mantive os indices do teu codigo original.
    // Confirma se correspondem aos destinos no Inspector.

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
        if (g == null || isMoving)
            return;

        if (index < 0 || index >= wps.Length || wps[index] == null)
        {
            Debug.LogWarning("Destino invalido!");
            return;
        }

        destination = wps[index];

        // Verificar se existe caminho ate ao destino.
        if (!g.AStar(currentNode, destination))
        {
            Debug.LogWarning(
                "Nao existe caminho para " + destination.name
            );

            destination = null;
            return;
        }

        int minimum = Mathf.Max(0, minExplorationSteps);
        int maximum = Mathf.Max(minimum, maxExplorationSteps);

        explorationTarget = Random.Range(minimum, maximum + 1);
        explorationSteps = 0;

        previousNode = null;
        goingToFinalDestination = false;
        isMoving = true;

        Debug.Log(
            "[EXPLORACAO] Destino: " + destination.name +
            " | Passos sorteados: " + explorationTarget
        );

        ChooseNextWaypoint();
    }

    void ChooseNextWaypoint()
    {
        if (!isMoving)
            return;

        if (currentNode == destination)
        {
            StopTank();
            return;
        }

        if (explorationSteps >= explorationTarget)
        {
            PlanFinalPath();
            return;
        }

        if (explorationSteps >= minExplorationSteps &&
            Random.value < destinationChance)
        {
            PlanFinalPath();
            return;
        }

        List<GameObject> neighbours =
            g.GetConnectedWaypoints(currentNode);

        // Evitar regressar imediatamente, quando possivel.
        if (neighbours.Count > 1 && previousNode != null)
        {
            neighbours.Remove(previousNode);
        }

        if (neighbours.Count == 0)
        {
            PlanFinalPath();
            return;
        }

        // Escolha pseudoaleatoria de um Waypoint vizinho.
        GameObject next =
            neighbours[Random.Range(0, neighbours.Count)];

        Debug.Log(
            "[EXPLORACAO] " +
            currentNode.name + " -> " + next.name
        );

        if (!g.AStar(currentNode, next))
        {
            PlanFinalPath();
            return;
        }

        SetCurrentPath();
    }

    void PlanFinalPath()
    {
        goingToFinalDestination = true;

        Debug.Log(
            "[A*] Caminho final para " + destination.name
        );

        if (!g.AStar(currentNode, destination))
        {
            Debug.LogWarning(
                "[A*] Nao existe caminho ate ao destino!"
            );

            StopTank();
            return;
        }

        SetCurrentPath();
    }

    void SetCurrentPath()
    {
        currentPath = new List<Node>(g.pathList);
        currentWP = 0;

        if (currentPath.Count == 0)
        {
            StopTank();
            return;
        }

        GameObject first = currentPath[0].getId();

        Vector3 firstPosition = first.transform.position;
        firstPosition.y = transform.position.y;

        if (Vector3.Distance(
            transform.position,
            firstPosition
        ) <= accuracy)
        {
            currentWP = 1;
        }

        if (currentWP >= currentPath.Count)
        {
            FinishCurrentPath();
        }
    }

    void Update()
    {
        if (!isMoving)
            return;

        if (currentWP >= currentPath.Count)
        {
            FinishCurrentPath();
            return;
        }

        GameObject waypoint = currentPath[currentWP].getId();

        if (waypoint == null)
        {
            Debug.LogError("Waypoint invalido!");
            StopTank();
            return;
        }

        Vector3 target = waypoint.transform.position;
        target.y = transform.position.y;

        Vector3 direction = target - transform.position;

        // Verificar chegada ao Waypoint.
        if (direction.magnitude <= accuracy)
        {
            GameObject oldNode = currentNode;

            if (oldNode != null && oldNode != waypoint)
            {
                g.RegisterTraversal(oldNode, waypoint);
            }

            previousNode = oldNode;
            currentNode = waypoint;

            Debug.Log("[CHEGADA] " + waypoint.name);

            currentWP++;

            if (currentWP >= currentPath.Count)
            {
                FinishCurrentPath();
            }

            return;
        }

        // Rodar gradualmente na direcao do Waypoint.
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Manter o comportamento de movimento original.
        if (Vector3.Angle(transform.forward, direction) < 45f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
        }
    }

    void FinishCurrentPath()
    {
        if (goingToFinalDestination)
        {
            StopTank();
            return;
        }

        explorationSteps++;

        Debug.Log(
            "[EXPLORACAO] Passo " +
            explorationSteps + "/" + explorationTarget
        );

        ChooseNextWaypoint();
    }

    void StopTank()
    {
        isMoving = false;
        goingToFinalDestination = false;

        currentWP = 0;
        currentPath.Clear();

        Debug.Log(
            "[TANQUE PARADO] Posicao: " +
            (currentNode != null
                ? currentNode.name
                : "desconhecida")
        );

        destination = null;

        Debug.Log("[TANQUE] A aguardar outro botao.");
    }
}

