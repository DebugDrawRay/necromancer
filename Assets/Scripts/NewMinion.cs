using UnityEngine;
using System.Collections;

public class NewMinion : MonoBehaviour
{
    public Transform commander;
    public float followDistance;
    public Vector3 followPoint
    {
        get
        {
            Vector3 inv = commander.GetComponent<Rigidbody>().velocity * -1f;
            Vector3 pos = inv.normalized * followDistance;
            return commander.transform.position + pos;
        }
    }



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
