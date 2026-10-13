
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWP : MonoBehaviour
{
    Transform goal;

    public float speed = 5.0f;
    public float accuracy = 5.0f;
    public float rotSpeed = 2.0f;

    public GameObject wpManager;

    GameObject[] wps;
    GameObject currentNode;

    int currentWP = 0;
    Graph g;

    void Start()
    {
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

        if (g == null || wps == null || wps.Length < 2)
        {
            Debug.LogError("Graph ou Waypoints invalidos!");
            enabled = false;
            return;
        }

        currentNode = wps[0];

        Invoke(nameof(GoToRuin), 2f);
    }

    public void GoToHeli()
    {
        if (g.AStar(currentNode, wps[0]))
            currentWP = 0;
    }

    public void GoToRuin()
    {
        if (g.AStar(currentNode, wps[1]))
            currentWP = 0;
    }

    void LateUpdate()
    {
        if (g == null ||
            g.pathList.Count == 0 ||
            currentWP >= g.pathList.Count)
        {
            return;
        }

        Vector3 targetPosition =
            g.pathList[currentWP].getId().transform.position;

        targetPosition.y = transform.position.y;

        if (Vector3.Distance(transform.position, targetPosition) < accuracy)
        {
            currentNode = g.pathList[currentWP].getId();
            currentWP++;
        }

        if (currentWP >= g.pathList.Count)
            return;

        goal = g.pathList[currentWP].getId().transform;

        Vector3 lookAtGoal = new Vector3(
            goal.position.x,
            transform.position.y,
            goal.position.z
        );

        Vector3 direction = lookAtGoal - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotSpeed
        );

        // Evita avancar enquanto estiver demasiado desalinhado
        float angle = Vector3.Angle(transform.forward, direction);

        if (angle < 45f)
        {
            transform.Translate(
                0,
                0,
                speed * Time.deltaTime
            );
        }
    }
}
