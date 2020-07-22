using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Virus : Enemy
{
    private float triggeredRange = 3f;
    private float explosionRange = 5f;

    private NavMeshAgent agent;

	//Sound effect
	public AudioClip boom;

    protected override void Start()
    {
        base.Start();
        maxHealth = 50;
        health = maxHealth;
        speed = 15;
        damage = 50;
        rotateSpeed = 500f;
        detectRangeRadius = 20;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = this.speed;
        agent.angularSpeed = rotateSpeed;

    }

    private void SelfDestruct()
    {
        Debug.Log("Boom Attack");
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
        HashSet<Player> hitPlayers = new HashSet<Player>();
        foreach (Collider hit in colliders)
        {
            if(hit.tag == "Player")
            {
                if (hit.transform.GetComponent<Player>() != null)
                    hitPlayers.Add(hit.transform.GetComponent<Player>());
                else
                    hitPlayers.Add(hit.transform.parent.GetComponent<Player>());
            }
        }
        foreach(Player player in hitPlayers)
        {
            player.TakeDamage(damage);
        }
        Destroy(gameObject, 1f);
        if(isServer)
         RpcDie();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (lockedPlayer != null)
        {
            // Demo: easy chase
            Vector3 direction = lockedPlayer.transform.position - transform.position;
            Movement(direction);
            Aim(direction);
            

            // TODO: navMesh to chase Player

            

            if (Vector3.Distance(lockedPlayer.transform.position, transform.position) < triggeredRange)
                SelfDestruct();
            //Movement(lockedPlayer.transform.position - transform.position);
            //Aim(lockedPlayer.transform.position - transform.position);
        }
    }

}