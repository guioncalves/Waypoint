
using UnityEngine;

public class FollowP : MonoBehaviour
{
    public GameObject[] waypoints;
    private int currentWP = 0;

    [Header("Tracker")]
    public float trackerSpeed = 10f;

    [Header("Orbita do Tanque")]
    public float orbitRadius = 10f;
    public float orbitSpeed = 1000f;
    public float rotSpeed = 10f;

    private float orbitAngle;
    private GameObject tracker;
    private float tankHeight;

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Nao existem Waypoints configurados!");
            enabled = false;
            return;
        }

        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tracker.name = "Tracker";

        Destroy(tracker.GetComponent<Collider>());

        tracker.transform.position = transform.position;

        tankHeight = transform.position.y;

        // Comecar com o tanque na sua posicao atual
        Vector3 offset = transform.position - tracker.transform.position;

        if (offset.sqrMagnitude > 0.001f)
        {
            orbitAngle = Mathf.Atan2(offset.z, offset.x)
                         * Mathf.Rad2Deg;
        }
        else
        {
            orbitAngle = 0f;
        }
    }

    void ProgressTracker()
    {
        Vector3 target = waypoints[currentWP].transform.position;
        target.y = tracker.transform.position.y;

        tracker.transform.position = Vector3.MoveTowards(
            tracker.transform.position,
            target,
            trackerSpeed * Time.deltaTime
        );

        if (Vector3.Distance(tracker.transform.position, target) < 0.1f)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
        }
    }

    void Update()
    {
        ProgressTracker();

        // Angulo aumenta continuamente
        orbitAngle += orbitSpeed * Time.deltaTime;

        float angleRad = orbitAngle * Mathf.Deg2Rad;

        // Posicao circular relativa ao Tracker
        Vector3 offset = new Vector3(
            Mathf.Cos(angleRad) * orbitRadius,
            0f,
            Mathf.Sin(angleRad) * orbitRadius
        );

        Vector3 newPosition = tracker.transform.position + offset;
        newPosition.y = tankHeight;

        // Calcular direcao do movimento
        Vector3 movement = newPosition - transform.position;

        if (movement.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotSpeed * Time.deltaTime
            );
        }

        transform.position = newPosition;
    }
}
