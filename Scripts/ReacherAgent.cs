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
    float steps = 0f;
    public float decreasingReward = 1f;
    float decreasing = -0.001f;
    float moveSpeed = 0f;
    float movePenalty = -0.0001f;
    float StimulusTime = 200f;
    float RestTime = 50f;
    float goalVisible;
    float Total_movement = 0f;
    float Total_movement_per_ep = 0f;
    float mean_m;
    float samp;
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

    float ISI_distance;
    float ISI_x;
    float ISI_y;
    float ISI_z;

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
            RandomPos = ChooseRandomPosition(transform.position, 0f, 360f, 0f, 9f);
            sensor.AddObservation(lastGoalPos);
            goalVisible = 0.0f;
        }

        sensor.AddObservation(hand.transform.localPosition);
        sensor.AddObservation(goalVisible);

        //Debug.Log(steps);
        //if (steps == -1)
        //{
        //    Debug.Log(Vector3.Distance(new Vector3(9f, 0f, 0f), hand.transform.position - transform.position));
        //    m_recorder.Add("DTB before onset", Vector3.Distance(new Vector3(9f, 0f, 0f), hand.transform.position - transform.position));
        //    m_recorder.Add("HIST before onset", Vector3.Distance(new Vector3(9f, 0f, 0f), hand.transform.position - transform.position), StatAggregationMethod.Histogram);
        //    //m_recorder.Add("Coordinate hand before onset", hand.transform.position);
        //}

        m_recorder.Add("total movement", Total_movement);
        

        moveSpeed = Vector3.Distance(hand.transform.position, prevHandPos);
        Total_movement += moveSpeed;
        Total_movement_per_ep += moveSpeed;
        prevHandPos = hand.transform.position;

        if (steps < -0.00001f)
        {
            mean_m = Total_movement_per_ep / (steps * -1);
            m_recorder.Add("Movement rest", mean_m);
        }

        if (steps > 0.00001f)
        {
            mean_m = Total_movement_per_ep / steps;
            m_recorder.Add("Movement stimulus", mean_m);
        }



        //Debug.Log(Total_movement_per_ep / steps);

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
            Total_movement_per_ep = 0;
            m_recorder.Add("Average DTB", ISI_distance / RestTime);
            m_recorder.Add("Average/X", ISI_x / RestTime);
            m_recorder.Add("Average/Y", ISI_y / RestTime);
            m_recorder.Add("Average/Z", ISI_z / RestTime);
        }

        if (steps > 0)
        {
            if (goal == null)
            {
                steps = -RestTime;
                ISI_distance = 0;
                ISI_x = 0;
                ISI_y = 0;
                ISI_z = 0;
                Total_movement_per_ep = 0;
            }
            else 
            {

                if (steps == StimulusTime)
                {
                    Destroy(goal);
                    steps = -RestTime;
                    ISI_distance = 0;
                    ISI_x = 0;
                    ISI_y = 0;
                    ISI_z = 0;
                    Total_movement_per_ep = 0;
                }


            }

        }

        //if (steps == StimulusTime)
        //{
            
        //    if (goal != null)
        //    {
        //        Destroy(goal);
        //    }

        //    ISI_distance = 0;
        //    ISI_x = 0;
        //    ISI_y = 0;
        //    ISI_z = 0;
        //    steps = - RestTime;
        //    Total_movement_per_ep = 0;
        //}

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


        //var torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f) * 150f;
        //var torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * 150f;
        //m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        //torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) * 150f;
        //torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f) * 150f;
        //m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));




        var torqueX = actionBuffers.ContinuousActions[0] * 150f;
        var torqueZ = actionBuffers.ContinuousActions[1] * 150f;
        m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = actionBuffers.ContinuousActions[2] * 150f;
        torqueZ = actionBuffers.ContinuousActions[3] * 150f;
        m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));





        decreasing = Academy.Instance.EnvironmentParameters.GetWithDefault("discount", decreasing);
        decreasingReward += decreasing;

        movePenalty = Academy.Instance.EnvironmentParameters.GetWithDefault("Punishment", movePenalty);
        float penaltyToApply = movePenalty * moveSpeed;
        GetComponent<ReacherAgent>().AddReward(penaltyToApply); //was 0.00001

        ISI_distance += Vector3.Distance(new Vector3(0f, 9f, 0f), hand.transform.position - transform.position);
        ISI_x += (hand.transform.position - transform.position)[0];
        ISI_y += (hand.transform.position - transform.position)[1];
        ISI_z += (hand.transform.position - transform.position)[2];

        if (steps == -1)
        {
            //Debug.Log(Vector3.Distance(new Vector3(9f, 0f, 0f), hand.transform.position - transform.position));
            m_recorder.Add("DTB before onset", Vector3.Distance(new Vector3(0f, 9f, 0f), hand.transform.position - transform.position));
            m_recorder.Add("HIST before onset", Vector3.Distance(new Vector3(0f, 9f, 0f), hand.transform.position - transform.position), StatAggregationMethod.Histogram);
            m_recorder.Add("Coordinate/X", (hand.transform.position - transform.position)[0]);
            m_recorder.Add("Coordinate/Y", (hand.transform.position - transform.position)[1]);
            m_recorder.Add("Coordinate/Z", (hand.transform.position - transform.position)[2]);
            //Debug.Log((hand.transform.position - transform.position)[0]);
        }


    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    ///


    void UpdateGoalPosition()
    {

        Stimulus_Distance = Academy.Instance.EnvironmentParameters.GetWithDefault("Stimulus_Distance", Stimulus_Distance);
        var random = new System.Random();
        //var list = new List<float> { Stimulus_Distance, -Stimulus_Distance };
        //int Xpos = random.Next(list.Count);
        ////int Xpos = random.Next(list.Count);
        //int Zpos = random.Next(list.Count);
        //-6f

        float Zpos;

        var rnd = System.Convert.ToDouble(random.Next(1, 100));
        samp = (float)(rnd / 100);

        if (samp < 0.8d)
        {
            Zpos = Stimulus_Distance;
        }

        else
        {
            Zpos = -Stimulus_Distance;
        }

        float Xpos;

        var rnd2 = System.Convert.ToDouble(random.Next(1, 100));
        samp = (float)(rnd2 / 100);

        if (samp < 0.5d)
        {
            Xpos = Stimulus_Distance;
        }

        else
        {
            Xpos = -Stimulus_Distance;
        }

        //Debug.Log(samp.ToString());


        //goal.transform.position = new Vector3(list[Ypos], 0f, list[Xpos]) + transform.position;
        //goal.transform.localPosition = new Vector3(list[Xpos], 9, list[Zpos]);
        goal.transform.localPosition = new Vector3(Xpos, 9, Zpos);

        //Debug.Log("Xpos = ");
        //Debug.Log(list[Xpos]);
        //Debug.Log("Zos = ");
        //Debug.Log(Zpos);
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

        m_recorder = Academy.Instance.StatsRecorder;

        decreasingReward = 1f;
        steps = 0;
        Total_movement_per_ep = 0;
        Total_movement = 0;
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
