
using UnityEngine;

public class FollowP : MonoBehaviour
{
    [Header("Waypoints")]
    public GameObject[] waypoints;

    [Header("Movimento")]
    public float speed = 10f;
    public float rotSpeed = 120f;
    public float stoppingDistance = 4f;

    private int currentWP = 0;
    private bool isLeader;
    private Transform tankToFollow;

    void Start()
    {
        // O tanque verde lidera o circuito
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

        // Cada tanque segue o anterior
        string leaderName = "";

        if (gameObject.name == "RedTank")
            leaderName = "GreenTank";
        else if (gameObject.name == "BlueTank")
            leaderName = "RedTank";
        else if (gameObject.name == "YellowTank")
            leaderName = "BlueTank";
        else
        {
            Debug.LogWarning("Tanque nao configurado: " + gameObject.name);
            enabled = false;
            return;
        }

        GameObject leader = GameObject.Find(leaderName);

        if (leader == null)
        {
            Debug.LogError("Nao foi encontrado: " + leaderName);
            enabled = false;
            return;
        }

        tankToFollow = leader.transform;
    }

    void Update()
    {
        if (isLeader)
            MoveLeader();
        else
            FollowTank();
    }

    void MoveLeader()
    {
        Vector3 direction =
            waypoints[currentWP].transform.position - transform.position;

        direction.y = 0f;

        // Passar ao proximo Waypoint
        if (direction.magnitude < 3f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            return;
        }

        // Rodar suavemente para o Waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Avancar para a frente, fazendo curvas em arco
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void FollowTank()
    {
        if (tankToFollow == null)
            return;

        Vector3 direction =
            tankToFollow.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        // Manter distancia do tanque da frente
        if (distance <= stoppingDistance)
            return;

        // Rodar suavemente para o tanque da frente
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotSpeed * Time.deltaTime
        );

        // Avancar sem ultrapassar a distancia minima
        float movement = Mathf.Min(
            speed * Time.deltaTime,
            distance - stoppingDistance
        );

        transform.position += transform.forward * movement;
    }
}
