
using UnityEngine;

public class FollowP : MonoBehaviour
{
    [Header("Waypoints")]
    public GameObject[] waypoints;

    [Header("Movimento")]
    public float speed = 10f;
    public float rotSpeed = 120f;
    public float stoppingDistance = 3f;

    [Header("Evitar Colisoes")]
    public float separationDistance = 4f;
    public float separationStrength = 2f;

    private int currentWP = 0;
    private bool isLeader;

    private Transform greenTank;
    private Vector3 formationOffset;

    void Start()
    {
        isLeader = gameObject.name == "GreenTank";

        if (isLeader)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("GreenTank sem Waypoints!");
                enabled = false;
            }

            return;
        }

        GameObject leader = GameObject.Find("GreenTank");

        if (leader == null)
        {
            Debug.LogError("GreenTank nao encontrado!");
            enabled = false;
            return;
        }

        greenTank = leader.transform;

        // Guardar a posicao inicial na formacao
        formationOffset =
            greenTank.InverseTransformPoint(transform.position);

        formationOffset.y = 0f;
    }

    void Update()
    {
        if (isLeader)
            MoveLeader();
        else
            FollowLeader();
    }

    void MoveLeader()
    {
        Vector3 direction =
            waypoints[currentWP].transform.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude < 3f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            return;
        }

        MoveTank(direction, speed);
    }

    void FollowLeader()
    {
        if (greenTank == null)
            return;

        // Posicao que o tanque deve ocupar na formacao
        Vector3 destination =
            greenTank.TransformPoint(formationOffset);

        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        // Calcular afastamento dos outros tanques
        Vector3 separation = Vector3.zero;

        FollowP[] tanks = FindObjectsOfType<FollowP>();

        foreach (FollowP tank in tanks)
        {
            if (tank == this)
                continue;

            Vector3 away = transform.position - tank.transform.position;
            away.y = 0f;

            float distance = away.magnitude;

            if (distance > 0.001f &&
                distance < separationDistance)
            {
                float strength =
                    (separationDistance - distance) / separationDistance;

                separation += away.normalized * strength;
            }
        }

        // Combinar formacao e separacao
        Vector3 desiredDirection =
            direction + separation * separationStrength;

        if (desiredDirection.sqrMagnitude < 0.001f)
            return;

        float distanceToFormation = direction.magnitude;

        float currentSpeed = Mathf.Min(
            speed,
            distanceToFormation * 2f
        );

        MoveTank(desiredDirection, currentSpeed);
    }

    void MoveTank(Vector3 direction, float moveSpeed)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        float angle =
            Vector3.Angle(transform.forward, direction);

        // Evitar avancar enquanto estiver virado
        // numa direcao muito diferente
        if (angle > 60f)
            return;

        transform.position +=
            transform.forward * moveSpeed * Time.deltaTime;
    }
}
