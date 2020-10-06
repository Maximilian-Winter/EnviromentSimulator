using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarAIMovement : MonoBehaviour
{
    private Vector3 target;

    private Transform followTarget;


    [SerializeField]
    private float m_MoveSpeed = 3f;
    [SerializeField]
    private float m_TurnSpeed = 3f;

    [SerializeField]
    private float nextWaypointDistance = 3f;
    [SerializeField]
    private float stopDistance = 1f;

    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;

    private Rigidbody rb;

    public bool ReachedEndOfPath { get; set; }

    RVO2Simulator simulator = null;
    int agentIndex = -1;
    private Vector3 lastTargetWorldPosition;
    private Transform lastFollowTarget;

    Vector3 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        lastTargetWorldPosition = Vector3.zero;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody>();
        simulator = GameObject.FindGameObjectWithTag("RVOSim").GetComponent<RVO2Simulator>();
        agentIndex = simulator.addAgentToSim(transform.position, gameObject);
        GetComponent<AnimalUnit>().IsMovmentInitialised = true;
    }

    private void CreateMoveToPath()
    {
        path = null; 
        if(seeker != null)
        if (seeker.IsDone())
            seeker.StartPath(transform.position, target, OnPathComplete);
    }

  

    private void UpdateFollowPath()
    {
        NNInfo nearestPointOnNavGraphInfo = AstarPath.active.GetNearest(followTarget.position, NNConstraint.Default);
        path = null;
        if (seeker.IsDone())
            seeker.StartPath(transform.position, nearestPointOnNavGraphInfo.position, OnPathComplete);
    }


    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void Update()
    {
        if (agentIndex != -1)
        {
            moveDir = (toUnityVector(simulator.getAgentPosition(agentIndex)) - transform.position).normalized;
            if(moveDir != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * m_TurnSpeed);
            }
            transform.position = Vector3.Lerp(transform.position, toUnityVector(simulator.getAgentPosition(agentIndex)), m_MoveSpeed* Time.deltaTime);
        }

        float distanceToTarget = Vector3.Distance(transform.position, target);

        if (distanceToTarget < stopDistance)
        {
            ReachedEndOfPath = true;
            return;
        }
        else
        {
            ReachedEndOfPath = false;
        }


        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            ReachedEndOfPath = true;
            return;
        }

        float distance = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    public void StopAgent()
    {
        CancelInvoke("UpdateFollowPath");
        path = null;
    }

    public void MoveToPosition(Vector3 position)
    {
        if (lastTargetWorldPosition != position || ReachedEndOfPath)
        {
            CancelInvoke("UpdateFollowPath");
            NNInfo nearestPointOnNavGraphInfo = AstarPath.active.GetNearest(position, NNConstraint.Default);
            this.target = nearestPointOnNavGraphInfo.position;
            CreateMoveToPath();
        }
        lastTargetWorldPosition = position;
    }

    public void FollowTarget(Transform target)
    {
        if (lastFollowTarget != target || ReachedEndOfPath)
        {
            CancelInvoke("UpdateFollowPath");
            followTarget = target;
            InvokeRepeating("UpdateFollowPath", 0.0f, 0.5f);
        }
        lastFollowTarget = target;
    }

    Vector3 PickRandomPoint(float radius)
    {
        var point = Random.insideUnitSphere * radius;
        point.y = 0;
        point += gameObject.transform.position;
        return point;
    }

    public bool IsNotMoving()
    {
        if (path == null)
        {
            return true;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            return true;
        }

        if (ReachedEndOfPath)
        {
            return true;
        }

        return false;
    }

    public bool IsReadyToMoveOn()
    {      
        if(path == null)
        {
            return false;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            return false;
        }

        if(ReachedEndOfPath)
        {
            return false;
        }

        return true;
    }

    public RVO.Vector2 GetNextWaypoint()
    {
        return toRVOVector(path.vectorPath[currentWaypoint]);
    }

    Vector3 toUnityVector(RVO.Vector2 param)
    {
        return new Vector3(param.x(), transform.position.y, param.y());
    }

    RVO.Vector2 toRVOVector(Vector3 param)
    {
        return new RVO.Vector2(param.x, param.z);
    }


    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Color gizmoColor = Color.green;
        Gizmos.color = gizmoColor;
        
        gizmoColor = Color.red;
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(transform.position, moveDir);
    }
}
