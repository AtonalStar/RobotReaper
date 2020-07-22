using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : Enemy {

	//Sound effect
	public AudioClip shoot4;
    [SerializeField]
    private Vector3 movePoint;
    private bool isMoving = false;

    protected override void Start()
    {
        base.Start();
        maxHealth = 200;
        health = maxHealth;
        speed = 5;
        damage = 5;
        bullet.GetComponent<Bullet>().damage = damage;
        attackSpeed = 0.2f;
        lastAttackTime = Time.time + attackSpeed;
        rotateSpeed = 200f;
        bulletSpeed = 30f;
        detectRangeRadius = 20;
        RandomMovePoint();
    }

    
    protected override void Update()
    {
        base.Update();
        
        if (lockedPlayer != null)
        {
            // TODO: implement AI behaviour. The lockedPlayer is the detected player
        }
        else
        {
            //Debug.Log("random move distance = " + Vector3.Distance(movePoint, transform.position));
            //Debug.Log("random move point:" + movePoint + "my postion:" + transform.position);
            if (movePoint == null || Vector3.Distance(movePoint, transform.position) < 2f)
            {
                
                RandomMovePoint();
                Debug.Log(this.gameObject + " new Random Move Point:" + movePoint);
            }
        }
    }

    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (lockedPlayer == null)
        {
            Vector3 direction = movePoint - transform.position;
            isMoving = false;
            if (Aim(direction))
            {
                isMoving = true;
                Movement(direction);
            }
        }
        else
        {
            Vector3 enemyDirection = lockedPlayer.transform.position - transform.position;
            if (Aim(enemyDirection))
            {
                float distance = Vector3.Distance(lockedPlayer.transform.position, transform.position);
                if (distance > 10f)
                    Movement(enemyDirection);
                if(distance < 5f)
                    Movement(-enemyDirection);
                Attack();
            }
        }

    }

    private void RandomMovePoint()
    {
        float radian = Random.Range(0, 2 * Mathf.PI);
        //Vector2 direction2D = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        Vector3 direction = new Vector3(Mathf.Cos(radian), 0.5f, Mathf.Sin(radian));
        float distance;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 10f);
        if (hit.transform != null)
            distance = Random.Range(0, hit.distance);
        else
            distance = Random.Range(0, 10f);
        movePoint = new Vector3(transform.position.x + direction.x * distance,
                               0.5f , transform.position.y + direction.y * distance);
    }

    public void OnCollisionStay(Collision collision)
    {
        Debug.Log(this.gameObject + "hit something");
        if (isMoving)
            RandomMovePoint();
    }
}
