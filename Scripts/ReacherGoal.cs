using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReacherGoal : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;
    public GameObject goal;

    //(other.gameObject == hand)

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hand_tag"))
        {
            //Debug.Log("BBB");
            goalOn.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Hand_tag"))
        {
            goalOn.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Hand_tag"))
    //    {
    //        agent.GetComponent<ReacherAgent>().AddReward(0.01f);

    //        //UpdateGoalPosition();
    //    }
    //}

    void UpdateGoalPosition()
    {

        var random = new System.Random();
        var list = new List<int> { 10, -10 };
        int Ypos = random.Next(list.Count);
        int Xpos = random.Next(list.Count);

        goal.transform.position = new Vector3(list[Ypos], 0f, list[Xpos]) + transform.position;
        //var radians = m_GoalDegree * Mathf.PI / 180f;
        //var goalX = 8f * Mathf.Cos(radians);
        //var goalY = 8f * Mathf.Sin(radians);
        //var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
        //goal.transform.position = new Vector3(goalY, goalZ, goalX) + transform.position;
    }

}
