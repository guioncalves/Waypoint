
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

public class WPManager : MonoBehaviour
{
    public GameObject[] waypoints;
    public Link[] links;

    public Graph graph;

    void Start()
    {
        if (graph == null)
        {
            graph = GetComponent<Graph>();
        }

        if (graph == null)
        {
            Debug.LogError("Graph nao encontrado!");
            return;
        }

        if (waypoints.Length > 0)
        {
            foreach (GameObject wp in waypoints)
            {
                graph.AddNode(wp);
            }

            foreach (Link l in links)
            {
                graph.AddEdger(l.node1, l.node2);

                if (l.dir == Link.direction.BI)
                {
                    graph.AddEdger(l.node2, l.node1);
                }
            }
        }
    }

    void Update()
    {

    }
}
