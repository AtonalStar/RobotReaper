using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FollowCamera : MonoBehaviour {

    public GameObject player;
    public float cameraOffset = 25f;


    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (player == null) return;
        transform.position = player.transform.position + new Vector3(0, cameraOffset, 0);
    }
}
