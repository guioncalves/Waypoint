using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowP : MonoBehaviour
{
    public GameObject[] waypoints;
    int currentWP = 0;
    public float speed = 10.0f;
    public float rotSpeed = 10.0f;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (Vector3.Distance(this.transform.position, waypoints[currentWP].transform.position) < 3)
            currentWP++;

        if (currentWP >= waypoints.Length)
            currentWP = 0;

        Quaternion lookatWP = Quaternion.LookRotation(
            waypoints[currentWP].transform.position - this.transform.position
        );

        this.transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            lookatWP,
            rotSpeed * Time.deltaTime
        );

        this.transform.Translate(0, 0, speed * Time.deltaTime);
    }
}