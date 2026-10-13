
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    private List<Edge> edges = new List<Edge>();
    private List<Node> nodes = new List<Node>();

    public List<Node> pathList = new List<Node>();

    [Header("Pathfinding A*")]
    public bool randomizeExploration = true;
    public bool showExplorationDebug = true;

    [Header("Variacao dos percursos")]
    [Range(0f, 5f)]
    public float randomness = 2f;

    [Range(0f, 5f)]
    public float memoryPenalty = 2f;

    [Header("Memoria dos caminhos")]
    [Min(1)]
    public int memoryLength = 8;

    private Dictionary<Edge, float> randomCosts =
        new Dictionary<Edge, float>();

    private Queue<string> recentConnections =
        new Queue<string>();

    // REGISTAR WAYPOINTS
    public void AddNode(GameObject id)
    {
        if (id == null || FindNode(id) != null)
            return;

        nodes.Add(new Node(id));
    }

    // REGISTAR LIGACOES
    // Mantemos o nome AddEdger para compatibilidade
    // com o WPManager.cs.
    public void AddEdger(GameObject fromNode, GameObject toNode)
    {
        Node from = FindNode(fromNode);
        Node to = FindNode(toNode);

        if (from == null || to == null || from == to)
            return;

        // Evitar ligacoes duplicadas.
        foreach (Edge existing in from.edgeList)
        {
            if (existing.toNode == to)
                return;
        }

        Edge edge = new Edge(from, to);

        edges.Add(edge);
        from.edgeList.Add(edge);
    }

    // PROCURAR UM NODE
    private Node FindNode(GameObject id)
    {
        foreach (Node node in nodes)
        {
            if (node.getId() == id)
                return node;
        }

        return null;
    }

    // OBTER WAYPOINTS DIRETAMENTE LIGADOS
    // Este metodo e utilizado pelo FolloWP.cs.
    public List<GameObject> GetConnectedWaypoints(GameObject waypoint)
    {
        List<GameObject> connected = new List<GameObject>();

        if (waypoint == null)
            return connected;

        Node node = FindNode(waypoint);

        if (node == null)
            return connected;

        foreach (Edge edge in node.edgeList)
        {
            if (edge.toNode == null)
                continue;

            GameObject next = edge.toNode.getId();

            if (next != null && !connected.Contains(next))
            {
                connected.Add(next);
            }
        }

        return connected;
    }

    // IDENTIFICAR UMA LIGACAO NOS DOIS SENTIDOS
    private string ConnectionKey(Node a, Node b)
    {
        int idA = a.getId().GetInstanceID();
        int idB = b.getId().GetInstanceID();

        if (idA < idB)
            return idA + "_" + idB;

        return idB + "_" + idA;
    }

    // REGISTAR LIGACOES PERCORRIDAS
    public void RegisterTraversal(GameObject fromID, GameObject toID)
    {
        Node from = FindNode(fromID);
        Node to = FindNode(toID);

        if (from == null || to == null || from == to)
            return;

        string key = ConnectionKey(from, to);

        recentConnections.Enqueue(key);

        while (recentConnections.Count > Mathf.Max(1, memoryLength))
        {
            recentConnections.Dequeue();
        }

        if (showExplorationDebug)
        {
            Debug.Log(
                "[MEMORIA] Percorrido: " +
                fromID.name + " -> " + toID.name
            );
        }
    }

    // VERIFICAR SE UMA LIGACAO FOI UTILIZADA
    private bool WasRecentlyUsed(Node a, Node b)
    {
        return recentConnections.Contains(ConnectionKey(a, b));
    }

    // BARALHAR A ORDEM DE EXPLORACAO
    private List<Edge> ShuffleEdges(List<Edge> original)
    {
        List<Edge> shuffled = new List<Edge>(original);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Edge temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        return shuffled;
    }

    // GERAR CUSTOS PSEUDOALEATORIOS
    private void GenerateRandomCosts()
    {
        randomCosts.Clear();

        foreach (Edge edge in edges)
        {
            randomCosts[edge] =
                Random.Range(1f, 1f + randomness);
        }
    }

    // ALGORITMO A*
    public bool AStar(GameObject startID, GameObject endID)
    {
        pathList.Clear();

        Node start = FindNode(startID);
        Node end = FindNode(endID);

        if (start == null || end == null)
        {
            Debug.LogError("[A*] Origem ou destino invalido!");
            return false;
        }

        // Reiniciar os valores dos Nodes.
        foreach (Node node in nodes)
        {
            node.g = float.PositiveInfinity;
            node.h = 0f;
            node.f = float.PositiveInfinity;
            node.cameFrom = null;
        }

        GenerateRandomCosts();

        List<Node> open = new List<Node>();
        HashSet<Node> closed = new HashSet<Node>();

        start.g = 0f;
        start.h = Distance(start, end);
        start.f = start.h;

        open.Add(start);

        Debug.Log(
            "[A*] Nova pesquisa: " +
            startID.name + " -> " + endID.name
        );

        while (open.Count > 0)
        {
            int index = LowestF(open);
            Node current = open[index];

            open.RemoveAt(index);

            if (showExplorationDebug)
            {
                Debug.Log(
                    "[A*] A explorar: " +
                    current.getId().name
                );
            }

            // Destino encontrado.
            if (current == end)
            {
                ReconstructPatth(start, end);

                Debug.Log(
                    "[A*] Percurso encontrado com " +
                    pathList.Count + " Waypoints."
                );

                return pathList.Count > 0;
            }

            closed.Add(current);

            List<Edge> neighbours =
                randomizeExploration
                    ? ShuffleEdges(current.edgeList)
                    : current.edgeList;

            foreach (Edge edge in neighbours)
            {
                Node neighbour = edge.toNode;

                if (neighbour == null ||
                    closed.Contains(neighbour))
                    continue;

                float baseDistance =
                    Distance(current, neighbour);

                float randomFactor =
                    randomCosts[edge];

                float penalty =
                    WasRecentlyUsed(current, neighbour)
                        ? memoryPenalty
                        : 0f;

                // O custo nunca e inferior a distancia real.
                float edgeCost =
                    baseDistance * (randomFactor + penalty);

                float tentativeG =
                    current.g + edgeCost;

                if (showExplorationDebug)
                {
                    Debug.Log(
                        "[A*] Avaliar " +
                        current.getId().name + " -> " +
                        neighbour.getId().name +
                        " | Custo: " +
                        edgeCost.ToString("F2")
                    );
                }

                if (tentativeG < neighbour.g)
                {
                    neighbour.cameFrom = current;
                    neighbour.g = tentativeG;
                    neighbour.h = Distance(neighbour, end);
                    neighbour.f = neighbour.g + neighbour.h;

                    if (!open.Contains(neighbour))
                    {
                        open.Add(neighbour);
                    }
                }
            }
        }

        Debug.LogWarning(
            "[A*] Sem percurso entre " +
            startID.name + " e " + endID.name
        );

        return false;
    }

    // RECONSTRUIR O PERCURSO ENCONTRADO
    // Mantemos o nome ReconstructPatth para
    // compatibilidade com o projeto.
    public void ReconstructPatth(Node startID, Node endID)
    {
        pathList.Clear();

        Node current = endID;

        while (current != null)
        {
            pathList.Insert(0, current);

            if (current == startID)
                return;

            current = current.cameFrom;
        }

        pathList.Clear();
    }

    // DISTANCIA ENTRE DOIS NODES
    private float Distance(Node a, Node b)
    {
        return Vector3.Distance(
            a.getId().transform.position,
            b.getId().transform.position
        );
    }

    // ESCOLHER O NODE COM MENOR F
    private int LowestF(List<Node> list)
    {
        float lowest = float.PositiveInfinity;
        List<int> candidates = new List<int>();

        const float epsilon = 0.0001f;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].f < lowest - epsilon)
            {
                lowest = list[i].f;
                candidates.Clear();
                candidates.Add(i);
            }
            else if (Mathf.Abs(list[i].f - lowest) <= epsilon)
            {
                candidates.Add(i);
            }
        }

        if (!randomizeExploration || candidates.Count == 1)
        {
            return candidates[0];
        }

        return candidates[Random.Range(0, candidates.Count)];
    }
}
