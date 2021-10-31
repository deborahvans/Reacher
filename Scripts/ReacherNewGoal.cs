//using UnityEngine;
//using ReacherAgent;

//public class ReacherGoal : MonoBehaviour
//{
//    public GameObject agent;
//    public GameObject hand;
//    public GameObject goalOn;
//    public GameObject goal;
//    public ReacherAgent agent = new ReacherAgent();

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject == hand)
//        {
//            goalOn.transform.localScale = new Vector3(1f, 1f, 1f);
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject == hand)
//        {
//            goalOn.transform.localScale = new Vector3(0f, 0f, 0f);
//        }
//    }

//    void OnTriggerStay(Collider other)
//    {
//        if (other.gameObject == hand)
//        {
//            agent.GetComponent<ReacherAgent>().AddReward(0.01f);
//        }
//    }


//    private void OnCollisionEnter(Collision collision)
//    {
//        if (other.gameObject == hand)
//        {
//            agent.GetComponent<ReacherAgent>().AddReward(0.01f);
//        }

//    }

//    public void MoveGoal()
//    {
//        var random = new System.Random();
//        var list = new List<int> { 14, -14 };
//        int index = random.Next(list.Count);
//        int index2 = random.Next(list.Count);

//        gameObject.transform.localPosition =
//            new Vector3(list[index], 1.5f, list[index2]);
//    }

//}


