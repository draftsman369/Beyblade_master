  using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{

    private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRadius = 10f;


    [SerializeField] private float patrolRadius;

    [SerializeField] private float wanderTimer;
    private float timer; 

    private void Awake()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        timer = wanderTimer;
    }

    private void Update()
    {

        timer += Time.deltaTime;
        if(PlayerDetected())
        {
            agent.SetDestination(player.position);
        }
        else if(timer >= wanderTimer)
        {
            Vector3 randomPoint = GenerateRandomPatrolPoint(this.transform.position,patrolRadius, NavMesh.AllAreas);
            agent.SetDestination(randomPoint);
            timer = 0;
        }


    }

    private Vector3 GenerateRandomPatrolPoint(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        if(NavMesh.SamplePosition(randomDirection, out NavMeshHit navMeshHit, dist, layerMask))
        {
            return navMeshHit.position;
        }
        else
        {
            return origin;
        }
    }


    private bool PlayerDetected()
    {
        if(Vector3.Distance(this.transform.position, player.position) < detectionRadius)
        {            
            Debug.Log("Player detected");
            return true;
        }
        Debug.Log("Player out of sight");
        return false;
    }

}
