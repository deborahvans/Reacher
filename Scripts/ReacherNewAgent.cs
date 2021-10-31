//using UnityEngine;
//using Unity.MLAgents;
//using Unity.MLAgents.Actuators;
//using Unity.MLAgents.Sensors;
//using System.Collections;
//using System.Collections.Generic;


//public class ReacherAgent : Agent
//{
//    public GameObject pendulumA;
//    public GameObject pendulumB;
//    public GameObject hand;
//    public GameObject goal;
//    float m_GoalDegree;
//    public float steps = 0;
//    Rigidbody m_RbA;
//    Rigidbody m_RbB;
//    // speed of the goal zone around the arm (in radians)
//    float m_GoalSpeed;
//    // radius of the goal zone
//    float m_GoalSize;
//    float m_GoalDuration;
//    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
//    float m_Deviation;
//    // Frequency of the cosine deviation of the goal along the vertical dimension
//    float m_DeviationFreq;
//    public GameObject backup;


//    EnvironmentParameters m_ResetParams;


//    /// <summary>
//    /// Collect the rigidbodies of the reacher in order to resue them for
//    /// observations and actions.
//    /// </summary>
//    public override void Initialize()
//    {
//        m_RbA = pendulumA.GetComponent<Rigidbody>();
//        m_RbB = pendulumB.GetComponent<Rigidbody>();

//        m_ResetParams = Academy.Instance.EnvironmentParameters;

//        SetResetParameters();
//    }

//    /// <summary>
//    /// We collect the normalized rotations, angularal velocities, and velocities of both
//    /// limbs of the reacher as well as the relative position of the target and hand.
//    /// </summary>
//    public override void CollectObservations(VectorSensor sensor)
//    {
//        //var empty_vector = new Vector3(null, null, null);
//        sensor.AddObservation(pendulumA.transform.localPosition);
//        sensor.AddObservation(pendulumA.transform.rotation);
//        sensor.AddObservation(m_RbA.angularVelocity);
//        sensor.AddObservation(m_RbA.velocity);

//        sensor.AddObservation(pendulumB.transform.localPosition);
//        sensor.AddObservation(pendulumB.transform.rotation);
//        sensor.AddObservation(m_RbB.angularVelocity);
//        sensor.AddObservation(m_RbB.velocity);

//        if (goal != null)
//        {
//            sensor.AddObservation(goal.transform.localPosition);
//        }
//        //else
//        //{
//        //    sensor.AddObservation(null);
//        //    sensor.AddObservation(null);
//        //    sensor.AddObservation(null);
//        //}

//        sensor.AddObservation(hand.transform.localPosition);

//        //sensor.AddObservation(m_GoalSpeed);
//        sensor.AddObservation(m_GoalDuration);
//    }

//    /// <summary>
//    /// The agent's four actions correspond to torques on each of the two joints.
//    /// </summary>
//    /// 

//    public override void OnActionReceived(ActionBuffers actionBuffers)
//    {
//        m_GoalDegree += m_GoalSpeed;
//        var appear = new List<float> { 500, 1000, 1500, 2000, 2500, 3000, 3500 };
//        var disappear = new List<float> { 400, 900, 1400, 1900, 2400, 2900, 3400 };

//        steps++;
//        //if (steps % 500 == 0)
//        //{
//        //    UpdateGoalPosition();
//        //}

//        if (appear.Contains(steps))
//        {
//            //UpdateGoalPosition();
//            goal = Instantiate<GameObject>(backup.gameObject);
//            goal.transform.parent = transform;
//            //goal.transform.localPosition = transform.localPosition;
//            UpdateGoalPosition();
//        }


//        if (goal != null && disappear.Contains(steps))
//        {
//            Destroy(goal);
//        }



//        var torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f) * 150f;
//        var torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * 150f;
//        m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

//        torqueX = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f) * 150f;
//        torqueZ = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f) * 150f;
//        m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));
//    }

//    /// <summary>
//    /// Used to move the position of the target goal around the agent.
//    /// </summary>
//    void UpdateGoalPosition()
//    {

//        var random = new System.Random();
//        var list = new List<int> { 9, -9 };
//        int Ypos = random.Next(list.Count);
//        int Xpos = random.Next(list.Count);

//        //goal.transform.position = new Vector3(list[Ypos], 0f, list[Xpos]) + transform.position;
//        goal.transform.localPosition = new Vector3(list[Ypos], -6f, list[Xpos]);
//        //var radians = m_GoalDegree * Mathf.PI / 180f;
//        //var goalX = 8f * Mathf.Cos(radians);
//        //var goalY = 8f * Mathf.Sin(radians);
//        //var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
//        //goal.transform.position = new Vector3(goalY, goalZ, goalX) + transform.position;
//    }

//    /// <summary>
//    /// Resets the position and velocity of the agent and the goal.
//    /// </summary>
//    public override void OnEpisodeBegin()
//    {
//        steps = 0;


//        pendulumA.transform.position = new Vector3(0f, -4f, 0f) + transform.position;
//        pendulumA.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
//        m_RbA.velocity = Vector3.zero;
//        m_RbA.angularVelocity = Vector3.zero;

//        pendulumB.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
//        pendulumB.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
//        m_RbB.velocity = Vector3.zero;
//        m_RbB.angularVelocity = Vector3.zero;

//        m_GoalDegree = Random.Range(0, 360);
//        //Debug.Log("AAA");


//        SetResetParameters();

//        if (goal == null)
//        {
//            goal = Instantiate<GameObject>(backup.gameObject);
//            goal.transform.parent = transform;

//        }

//        goal.transform.localScale = new Vector3(m_GoalSize, m_GoalSize, m_GoalSize);
//        UpdateGoalPosition();

//    }



//    public void SetResetParameters()
//    {
//        m_GoalSize = m_ResetParams.GetWithDefault("goal_size", 5);
//        m_GoalSpeed = Random.Range(-1f, 1f) * m_ResetParams.GetWithDefault("goal_speed", Random.Range(1f, 6f));
//        m_Deviation = m_ResetParams.GetWithDefault("deviation", 0);
//        m_DeviationFreq = m_ResetParams.GetWithDefault("deviation_freq", 0);
//        m_GoalDuration = 400;
//    }
//}
