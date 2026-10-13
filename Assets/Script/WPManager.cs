
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
        // Obter o Graph associado ao WPManager
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

        // Registar todos os Waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                graph.AddNode(waypoints[i]);
            }
        }

        // Registar as ligacoes entre os Waypoints
        if (links != null)
        {
            for (int i = 0; i < links.Length; i++)
            {
                Link l = links[i];

                if (l.node1 == null || l.node2 == null)
                    continue;

                // Ligacao de Node 1 para Node 2
                graph.AddEdger(l.node1, l.node2);

                // Ligacao no sentido inverso, quando permitida
                if (bidirectionalPaths ||
                    l.dir == Link.direction.BI)
                {
                    graph.AddEdger(l.node2, l.node1);
                }
            }
        }

        Debug.Log(
            "[WPManager] Grafo construido com " +
            waypoints.Length + " Waypoints e " +
            (links != null ? links.Length : 0) +
            " Links configurados."
        );
    }
}
