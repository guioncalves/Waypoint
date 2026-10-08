
using UnityEngine;

public class FollowP : MonoBehaviour
{
    [Header("Waypoints do tanque verde")]
    public GameObject[] waypoints;

    [Header("Movimento")]
    public float speed = 10f;
    public float rotSpeed = 120f;
    public float stoppingDistance = 3f;

    private int currentWP = 0;
    private Transform greenTank;
    private Vector3 formationOffset;
    private bool isLeader;

    void Start()
    {
        // Identificar o tanque verde
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

        // Os outros tanques procuram automaticamente o verde
        GameObject leader = GameObject.Find("GreenTank");

        if (leader == null)
        {
            Debug.LogError("GreenTank nao encontrado!");
            enabled = false;
            return;
        }

        greenTank = leader.transform;

        // Manter a posicao inicial relativa ao verde
        formationOffset =
            greenTank.InverseTransformPoint(transform.position);

        formationOffset.y = 0f;
    }

    void Update()
    {
        if (isLeader)
        {
            MoveLeader();
        }
        else
        {
            FollowLeader();
        }
    }

    void MoveLeader()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Vector3 target = waypoints[currentWP].transform.position;
        target.y = transform.position.y;

        Vector3 direction = target - transform.position;

        // Passar ao proximo Waypoint quando estiver perto
        if (direction.magnitude < 3f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            return;
        }

        // Rodar suavemente na direcao do Waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Avancar para o Waypoint sem o ultrapassar
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    void FollowLeader()
    {
        if (greenTank == null)
            return;

        // Cada tanque tem uma posicao diferente na formacao
        Vector3 destination =
            greenTank.TransformPoint(formationOffset);

        destination.y = transform.position.y;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance < 0.3f)
            return;

        MoveTowardsDirection(
            direction,
            Mathf.Min(speed, distance * 2f)
        );
    }

    void MoveTowardsDirection(Vector3 direction, float moveSpeed)
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

        // Avancar para a frente, criando curvas em arco
        transform.position +=
            transform.forward * moveSpeed * Time.deltaTime;
    }
}
