
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Link
{
    public enum direction { UNI, BI }

    public GameObject node1;
    public GameObject node2;
    public direction dir;
}

[RequireComponent(typeof(Graph))]
public class WPManager : MonoBehaviour
{
    public GameObject[] waypoints;
    public Link[] links;

    [Header("Configuracao dos caminhos")]
    public bool bidirectionalPaths = true;

    public Graph graph;

    void Awake()
    {
        graph = GetComponent<Graph>();

        if (graph == null)
        {
            Debug.LogError("[WPManager] Graph nao encontrado!");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[WPManager] Sem Waypoints!");
            return;
        }

        HashSet<GameObject> registered =
            new HashSet<GameObject>();

        // Registar Waypoints sem duplicados
        for (int i = 0; i < waypoints.Length; i++)
        {
            GameObject waypoint = waypoints[i];

            if (waypoint == null)
            {
                Debug.LogWarning(
                    "[WPManager] Element " + i +
                    " nao tem Waypoint atribuido."
                );

                continue;
            }

            if (!registered.Add(waypoint))
            {
                Debug.LogWarning(
                    "[WPManager] Waypoint repetido: " +
                    waypoint.name
                );

                continue;
            }

            graph.AddNode(waypoint);
        }

        int validLinks = 0;

        // Criar ligacoes entre os Waypoints
        if (links != null)
        {
            for (int i = 0; i < links.Length; i++)
            {
                Link link = links[i];

                if (link.node1 == null || link.node2 == null)
                {
                    Debug.LogWarning(
                        "[WPManager] Link " + i +
                        " incompleto."
                    );

                    continue;
                }

                if (link.node1 == link.node2)
                {
                    Debug.LogWarning(
                        "[WPManager] Link " + i +
                        " liga um Waypoint a si proprio."
                    );

                    continue;
                }

                if (!registered.Contains(link.node1) ||
                    !registered.Contains(link.node2))
                {
                    Debug.LogWarning(
                        "[WPManager] Link " + i +
                        " utiliza Waypoints nao registados."
                    );

                    continue;
                }

                graph.AddEdger(link.node1, link.node2);

                if (bidirectionalPaths ||
                    link.dir == Link.direction.BI)
                {
                    graph.AddEdger(link.node2, link.node1);
                }

                validLinks++;
            }
        }

        Debug.Log(
            "[WPManager] Grafo construido com " +
            registered.Count +
            " Waypoints e " +
            validLinks +
            " Links validos."
        );
    }
}
