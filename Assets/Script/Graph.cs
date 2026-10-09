
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    List<Edge> edges = new List<Edge>();
    List<Node> nodes = new List<Node>();
    List<Node> pathList = new List<Node>();

    public void AddNode(GameObject id)
    {
        Node node = new Node(id);
        nodes.Add(node);
    }

    public void AddEdger(GameObject fromNode, GameObject toNode)
    {
        Node from = FindNode(fromNode);
        Node to = FindNode(toNode);

        if (from != null && to != null)
        {
            Edge e = new Edge(from, to);
            edges.Add(e);
            from.edgeList.Add(e);
        }
    }

    Node FindNode(GameObject id)
    {
        foreach (Node n in nodes)
        {
            if (n.getId() == id)
                return n;
        }

        return null;
    }

    public bool AStar(GameObject startID, GameObject endID)
    {
        Node start = FindNode(startID);
        Node end = FindNode(endID);

        if (start == null || end == null)
            return false;

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        // Reiniciar os valores dos nos
        foreach (Node node in nodes)
        {
            node.g = float.PositiveInfinity;
            node.h = 0f;
            node.f = float.PositiveInfinity;
            node.cameFrom = null;
        }

        start.g = 0f;
        start.h = distance(start, end);
        start.f = start.g + start.h;

        open.Add(start);

        while (open.Count > 0)
        {
            int i = lowestF(open);
            Node thisNode = open[i];

            // Encontramos o destino
            if (thisNode == end)
            {
                pathList.Clear();

                Node current = end;

                while (current != null)
                {
                    pathList.Insert(0, current);
                    current = current.cameFrom;
                }

                return true;
            }

            open.RemoveAt(i);
            closed.Add(thisNode);

            foreach (Edge e in thisNode.edgeList)
            {
                Node neighbour = e.toNode;

                if (closed.Contains(neighbour))
                    continue;

                float tentative_g_score =
                    thisNode.g + distance(thisNode, neighbour);

                if (tentative_g_score < neighbour.g)
                {
                    neighbour.cameFrom = thisNode;
                    neighbour.g = tentative_g_score;
                    neighbour.h = distance(neighbour, end);
                    neighbour.f = neighbour.g + neighbour.h;

                    if (!open.Contains(neighbour))
                        open.Add(neighbour);
                }
            }
        }

        pathList.Clear();
        return false;
    }

    public void ReconstructPatth(Node startID, Node endID)
    {
        pathList.Clear();
        pathList.Add(endID);

        var p = endID.cameFrom;
        while(p != startID && p != null)
        {
            pathList.Insert(0, p);
            p = p.cameFrom;

        }
        pathList.Insert(0, startID);
    }

    float distance(Node a, Node b)
    {
        return Vector3.Distance(
            a.getId().transform.position,
            b.getId().transform.position
        );
    }

    int lowestF(List<Node> list)
    {
        int lowestIndex = 0;

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].f < list[lowestIndex].f)
            {
                lowestIndex = i;
            }
        }

        return lowestIndex;
    }
}
