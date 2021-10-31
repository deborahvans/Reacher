using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReacherHand : MonoBehaviour
{
    public GameObject agent;
    float reward;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("reacher_goal"))
        {
            reward = agent.GetComponent<ReacherAgent>().decreasingReward;
            //Debug.Log("AAA");
            Destroy(other.gameObject);
            agent.GetComponent<ReacherAgent>().AddReward(reward);
            
        }

    }

}