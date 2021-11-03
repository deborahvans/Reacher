using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;


public class ReacherAgent : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject hand;
    public GameObject goal;
    public float Stimulus_Distance = 9f;
    Vector3 lastGoalPos;
    Vector3 RandomPos;
    float m_GoalDegree;
    public float steps = 0;
    public float decreasingReward = 1f;
    float decreasing = -0.001f;
    float moveSpeed = 0f;
    float movePenalty = -0.0001f;
    float StimulusTime = 200f;
    float RestTime = 50f;
    float goalVisible;
    float Total_movement = 0f;
    float Total_movement_per_ep = 0f;
    //float distance;
    Rigidbody m_RbA;
    Rigidbody m_RbB;
    // speed of the goal zone around the arm (in radians)
    float m_GoalSpeed;
    // radius of the goal zone
    float m_GoalSize;
    float m_GoalDuration;
    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
    float m_Deviation;
    // Frequency of the cosine deviation of the goal along the vertical dimension
    float m_DeviationFreq;
    public GameObject backup;
    Vector3 prevHandPos;

    StatsRecorder m_recorder;


    EnvironmentParameters m_ResetParams;
    

    /// <summary>
    /// Collect the rigidbodies of the reacher in order to resue them for
    /// observations and actions.
    /// </summary>
    public override void Initialize()
    {
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    /// <summary>
    /// We collect the normalized rotations, angularal velocities, and velocities of both
    /// limbs of the reacher as well as the relative position of the target and hand.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        //var empty_vector = new Vector3(null, null, null);
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.angularVelocity);
        sensor.AddObservation(m_RbA.velocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.angularVelocity);
        sensor.AddObservation(m_RbB.velocity);

        if (goal != null)
        {
            sensor.AddObservation(goal.transform.localPosition);
            lastGoalPos = goal.transform.localPosition;
            goalVisible = 1.0f;
        }
        else
        {
            m_recorder.Add("distance to base", Vector3.Distance(new Vector3(9f, 0f, 0f), hand.transform.position - transform.position));
            RandomPos = ChooseRandomPosition(transform.position, 0f, 360f, 0f, 9f);
            sensor.AddObservation(lastGoalPos);
            goalVisible = 0.0f;
        }

        sensor.AddObservation(hand.transform.localPosition);
        sensor.AddObservation(goalVisible);

        m_recorder.Add("total movement", Total_movement);
        

        moveSpeed = Vector3.Distance(hand.transform.position, prevHandPos);
        Total_movement += moveSpeed;
        Total_movement_per_ep += moveSpeed;
        prevHandPos = hand.transform.position;


        m_recorder.Add("mean movement per step", Total_movement_per_ep / steps);
        

        //Debug.Log(moveSpeed);

    }

    /// <summary>
    /// The agent's four actions correspond to torques on each of the two joints.
    /// </summary>
    /// 
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        m_GoalDegree += m_GoalSpeed;
        //var appear = new List<float> { 500, 1000, 1500, 2000, 2500, 3000, 3500 };
        //var disappear = new List<float> { 400, 900, 1400, 1900, 2400, 2900, 3400 };

        //var appear = new List<float> { 250, 500, 750, 1000, 1250, 1500, 1750, 2000, 2250, 2500, 2750, 3000, 3250, 3500 };
        //var disappear = new List<float> { 200, 450, 700, 950, 1200, 1450, 1700, 1950, 2200, 2450, 2700, 2950, 3200, 3450 };

        steps++;

        //if (steps % 500 == 0)
        //{
        //    UpdateGoalPosition();
        //}

        if (steps == 0)
        {
            decreasingReward = 1f;
            //UpdateGoalPosition();
            goal = Instantiate<GameObject>(backup.gameObject);
            goal.transform.parent = transform;
            //goal.transform.localPosition = transform.localPosition;
            UpdateGoalPosition();
        }


        if (steps == StimulusTime)
        {
            
            if (goal != null)
            {
                Destroy(goal);
            }
            steps = - RestTime;
        }

        //if (appear.Contains(steps))
        //{
        //    decreasingReward = 1f;
        //    //UpdateGoalPosition();
        //    goal = Instantiate<GameObject>(backup.gameObject);
        //    goal.transform.parent = transform;
        //    //goal.transform.localPosition = transform.localPosition;
        //    UpdateGoalPosition();
        //}


        //if (goal != null && disappear.Contains(steps))
        //{
        //    Destroy(goal);
        //}


        var torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f) * 150f;
        var torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * 150f;
        m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) * 150f;
        torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f) * 150f;
        m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));


        decreasing = Academy.Instance.EnvironmentParameters.GetWithDefault("discount", decreasing);
        decreasingReward += decreasing;

        movePenalty = Academy.Instance.EnvironmentParameters.GetWithDefault("Punishment", movePenalty);
        float penaltyToApply = movePenalty * moveSpeed;
        GetComponent<ReacherAgent>().AddReward(penaltyToApply); //was 0.00001

    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    void UpdateGoalPosition()
    {

        Stimulus_Distance = Academy.Instance.EnvironmentParameters.GetWithDefault("Stimulus_Distance", Stimulus_Distance);
        var random = new System.Random();
        var list = new List<float> { Stimulus_Distance, -Stimulus_Distance };
        int Ypos = random.Next(list.Count);
        //int Xpos = random.Next(list.Count);
        int Zpos = random.Next(list.Count);
        //-6f

        //goal.transform.position = new Vector3(list[Ypos], 0f, list[Xpos]) + transform.position;
        goal.transform.localPosition = new Vector3(9, list[Ypos], list[Zpos]);
        //var radians = m_GoalDegree * Mathf.PI / 180f;
        //var goalX = 8f * Mathf.Cos(radians);
        //var goalY = 8f * Mathf.Sin(radians);
        //var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
        //goal.transform.position = new Vector3(goalY, goalZ, goalX) + transform.position;
    }

    /// <summary>
    /// Resets the position and velocity of the agent and the goal.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        decreasingReward = 1f;
        steps = 0;
        Total_movement_per_ep = 0;
        prevHandPos = hand.transform.position;

        pendulumA.transform.position = new Vector3(0f, -4f, 0f) + transform.position;
        pendulumA.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbA.velocity = Vector3.zero;
        m_RbA.angularVelocity = Vector3.zero;

        pendulumB.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
        pendulumB.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbB.velocity = Vector3.zero;
        m_RbB.angularVelocity = Vector3.zero;

        m_GoalDegree = Random.Range(0, 360);
        //Debug.Log("AAA");

        

        SetResetParameters();

        if (goal == null)
        {
            goal = Instantiate<GameObject>(backup.gameObject);
            goal.transform.parent = transform;
            
        }

        m_recorder = Academy.Instance.StatsRecorder;

        goal.transform.localScale = new Vector3(m_GoalSize, m_GoalSize, m_GoalSize);
        UpdateGoalPosition();

    }



    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = minRadius;

        if (maxRadius > minRadius)
        {
            radius = UnityEngine.Random.Range(minRadius, maxRadius);
        }

        return center + Quaternion.Euler(0f, UnityEngine.Random.Range(minAngle, maxAngle), 0f) * Vector3.forward * radius;
    }



    public void SetResetParameters()
    {
        m_GoalSize = m_ResetParams.GetWithDefault("goal_size", 5);
        m_GoalSpeed = Random.Range(-1f, 1f) * m_ResetParams.GetWithDefault("goal_speed", Random.Range(1f, 6f));
        m_Deviation = m_ResetParams.GetWithDefault("deviation", 0);
        m_DeviationFreq = m_ResetParams.GetWithDefault("deviation_freq", 0);
        //m_GoalDuration = 200;
    }
}
