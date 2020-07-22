using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    [SyncVar] 
    [HideInInspector] public int damage;
    [SyncVar]
    [HideInInspector] public NetworkInstanceId shooterNetId;

    void OnTriggerEnter(Collider other)
    {

#if true
        //Debug.Log(shooterNetId + "bullet hit:" + other.transform.name);
        Robot robot = other.gameObject.GetComponentInParent<Robot>();
        if (robot != null)
        {
            if (robot.netId != shooterNetId)
                robot.TakeDamage(this);
            else
                return;
        }
        Boom();
        return;

#else
        
        if(other.tag == "wall")
        {
            Boom();
        }
        if(other.tag == "robot" || other.tag == "player")
        {
            other.GetComponent<Robot>().hit(damage);
            Boom();
        }
#endif
    }

    void Boom()
    {
        //TODO: sound and boom effects
        NetworkServer.Destroy(gameObject);
    }
}
