using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent Agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Agent.SetDestination(Target.position);
    }
}
