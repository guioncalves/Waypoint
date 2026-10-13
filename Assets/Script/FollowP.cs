
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

    private GameObject[] wps;
    private GameObject currentNode;
    private Graph g;

    private List<Node> currentPath = new List<Node>();
    private int currentWP = 0;
    private bool isMoving = false;

    void Start()
    {
        Debug.Log("FOLLOWWP ATIVO NO OBJETO: " + gameObject.name);
        WPManager manager = wpManager.GetComponent<WPManager>();

        wps = manager.waypoints;
        g = manager.graph;

        currentNode = wps[0];

        // O tanque comeca parado
        isMoving = false;
    }
    void LateUpdate()
    {
        if (!isMoving)
            return;

        if (g == null || g.pathList.Count == 0)
        {
            isMoving = false;
            return;
        }
    }

    // Restante código do movimento...

    public void GoToHeli()
    {
        Debug.Log("BOTAO HELI PREMIDO");
        MoveToWaypoint(0);
    }

    public void GoToRuin()
    {
        Debug.Log("BOTAO RUIN PREMIDO");
        MoveToWaypoint(1);
    }

    public void GoToFactory()
    {
        Debug.Log("BOTAO FACTORY PREMIDO");
        MoveToWaypoint(4);
    }
    void MoveToWaypoint(int index)
    {
        if (g == null || isMoving)
            return;

        if (index < 0 || index >= wps.Length || wps[index] == null)
            return;

        if (g.AStar(currentNode, wps[index]))
        {
            currentPath = new List<Node>(g.pathList);
            currentWP = 0;
            isMoving = true;

            Debug.Log("Botao selecionou: " + wps[index].name);
            Debug.Log("Percurso com " + currentPath.Count + " nos.");
        }
        else
        {
            Debug.LogWarning("Nao existe caminho para " + wps[index].name);
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

        GameObject waypoint = currentPath[currentWP].getId();

        Vector3 target = waypoint.transform.position;
        target.y = transform.position.y;

        Vector3 direction = target - transform.position;

            Debug.Log(
        "Waypoint atual: " + waypoint.name +
        " | Distancia: " + direction.magnitude +
        " | Waypoint numero: " + currentWP +
        " / " + currentPath.Count
            );

        // Verificar se chegou ao waypoint
        if (direction.magnitude <= accuracy)
        {
            currentNode = waypoint;
            currentWP++;

            // Ultimo waypoint alcancado
            if (currentWP >= currentPath.Count)
            {
                StopTank();
            }

            return;
        }

        // Rodar na direcao do waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Avancar apenas quando estiver orientado
        if (Vector3.Angle(transform.forward, direction) < 45f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
        }
    }

    void StopTank()
    {
        isMoving = false;
        currentWP = 0;

        Debug.Log("Tanque parado no destino: " + currentNode.name);
    }
}
