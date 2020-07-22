using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlackGun : Player {

    private float flashDistance = 7f;
    [SyncVar(hook = "Flash")]
    private bool flashTrigger = false ;

	//Sound Effect
	public AudioClip shoot3;
	public AudioClip flash;

    // Use this for initialization
    protected override void Start () {
        maxHealth = 200;
        health = maxHealth;
        speed = 11;
        damage = 50;
        bullet.GetComponent<Bullet>().damage = damage;
        attackSpeed = 0.3f;
        lastAttackTime = Time.time + attackSpeed;
        rotateSpeed = 200f;
        bulletSpeed = 50f;
        skillCoolDownTime = 1f;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate () {
        base.FixedUpdate();
	}

    protected override GameObject[] Attack()
    {
        if (!AttackTimeCheck())
        {
            return null;
        }
        GameObject[] bulletList = new GameObject[1];
        bulletList[0] = Shoot(shootPoint);
        //GetComponent<AudioSource> ().Play ();

        PlaySound(shoot3);		//Sound effect

        return bulletList;
    }


    protected override void UseSkill()
    {
        base.UseSkill();
        flashTrigger = true;
        PlaySound(flash);        //Sound effect
    }

    private void Flash(bool flag)
    {
        if (!flag)
            return;
        Debug.Log("Wieeeeeeeeee");
        Vector3 direction = MovementInput();
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;
        Debug.DrawRay(transform.position, direction);

        Physics.Raycast(ray, out hit, 7f, 1 << 12);
        if(hit.transform != null)
        {
            Debug.Log("Flash Hit Edge: " + hit.point);
            transform.position = hit.point - direction * 1f;
        }
        else
            transform.position += MovementInput() * flashDistance;
        flashTrigger = false;
    }

}
