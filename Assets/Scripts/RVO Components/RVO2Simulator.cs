using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RVO;

public class RVO2Simulator : MonoBehaviour
{

    List<RVO.Vector2> agentPositions;
    List<GameObject> rvoGameObj;

    // Use this for initialization
    void Awake()
    {
        agentPositions = new List<RVO.Vector2>();
        rvoGameObj = new List<GameObject>();

        Simulator.Instance.setTimeStep(0.01f);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 0.75f, 200.0f, new RVO.Vector2(0.0f, 0.0f));
    }

    public Simulator getSimulator()
    {
        return Simulator.Instance;
    }

    Vector3 toUnityVector(RVO.Vector2 param)
    {
        return new Vector3(param.x(), transform.position.y, param.y());
    }

    RVO.Vector2 toRVOVector(Vector3 param)
    {
        return new RVO.Vector2(param.x, param.z);
    }

    public RVO.Vector2 getAgentPosition(int agentIndex)
    {
        return Simulator.Instance.getAgentPosition(agentIndex);
    }

    public RVO.Vector2 getAgentVelocity(int agentIndex)
    {
        return Simulator.Instance.getAgentVelocity(agentIndex);
    }


    public int addAgentToSim(Vector3 pos, GameObject ag)
    {
        //clear the simulation
        //Simulator.Instance.Clear();
        //re initialize the simulation
       // Simulator.Instance.setTimeStep(0.10f);
        //Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 0.5f, 2.0f, new RVO.Vector2(0.0f, 0.0f));

        //add all the previous agents
       /* int agentCount = agentPositions.Count;
        for (int i = 0; i < agentCount; i++)
        {
            Simulator.Instance.addAgent(agentPositions[i]);
        }*/

        //add the new agent
        rvoGameObj.Add(ag);
        agentPositions.Add(toRVOVector(pos));

        //return the index of the new added agent
        return Simulator.Instance.addAgent(toRVOVector(pos));
    }

    // Update is called once per frame
    void Update()
    {
        int agentNUmber = Simulator.Instance.getNumAgents();
        try
        {
            for (int i = 0; i < agentNUmber; i++)
            {
                RVO.Vector2 agentLoc = Simulator.Instance.getAgentPosition(i);
                RVO.Vector2 station = agentLoc - agentLoc;

                AstarAIMovement movementComponent = rvoGameObj[i].GetComponent<AstarAIMovement>();
                if (movementComponent.IsReadyToMoveOn())
                {
                    station = rvoGameObj[i].GetComponent<AstarAIMovement>().GetNextWaypoint() - agentLoc;
                }
                
                
                if (RVOMath.absSq(station) > 1.0f)
                {
                    station = RVOMath.normalize(station);
                }

                Simulator.Instance.setAgentPrefVelocity(i, station);
                agentPositions[i] = Simulator.Instance.getAgentPosition(i);
            }
            Simulator.Instance.doStep();
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }
}
