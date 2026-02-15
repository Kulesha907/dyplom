using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent Agent;

    void Update()
    {
        Agent.SetDestination(Target.position);
    }
}
