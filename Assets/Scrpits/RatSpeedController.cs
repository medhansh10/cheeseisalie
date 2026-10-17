using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RatSpeedController : MonoBehaviour
{
    [Header("Speed")]
    [Min(0f)] public float farCheeseSpeed = 2.5f;
    [Min(0f)] public float closeCheeseSpeed = 6f;
    [Min(0f)] public float maximumPathDistance = 20f;

    [Header("Movement")]
    [Min(0f)] public float acceleration = 18f;
    [Min(0f)] public float angularSpeed = 720f;
    [Min(0f)] public float stoppingDistance = 0.35f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
        ResetSpeed();
    }

    public void SetSpeedForPath(float pathDistance)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        float normalizedDistance = maximumPathDistance > 0f
            ? Mathf.Clamp01(pathDistance / maximumPathDistance)
            : 1f;
        float closeness = 1f - normalizedDistance;

        agent.speed = Mathf.Lerp(farCheeseSpeed, closeCheeseSpeed, closeness);
    }

    public void ResetSpeed()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = farCheeseSpeed;
    }

    private void ConfigureAgent()
    {
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }
}
