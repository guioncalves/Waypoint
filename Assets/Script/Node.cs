
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private GameObject id;

    public List<Edge> edgeList = new List<Edge>();
    // Valores utilizados pelo algoritmo A*
    public float g;
    public float h;
    public float f;

    // No anterior no caminho
    public Node cameFrom;

    public Node(GameObject id)
    {
        this.id = id;
    }

    public GameObject getId()
    {
        return id;
    }
}
