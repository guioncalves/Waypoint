using System.Collections;
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

    public Graph graph;

    void Awake()
    {
        graph = GetComponent<Graph>();

        Debug.Log("Graph encontrado? " + (graph != null));
    }

    void Start()
    {
        if (graph == null)
        {
            Debug.LogError("Graph nao encontrado!");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("WPManager sem Waypoints!");
            return;
        }

        foreach (GameObject wp in waypoints)
        {
            if (wp != null)
            {
                graph.AddNode(wp);
            }
        }

        foreach (Link l in links)
        {
            if (l.node1 == null || l.node2 == null)
                continue;

            graph.AddEdger(l.node1, l.node2);

            if (l.dir == Link.direction.BI)
            {
                graph.AddEdger(l.node2, l.node1);
            }
        }
    }
}
