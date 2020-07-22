using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Ghost : Player {


    [SyncVar]
    private bool superAttack = false;
    [SyncVar(hook = "ChangeState")]
    private bool isInvisible = false;
    private float maxInvisibleTime = 10f;
    private float invisibleTime = 0;

    // used for demo
    private Color colorInvisible = new Color(0.515f, 0.515f, 0.515f, 0.766f);

	//Sound Effect
	public AudioClip shoot1;
	public AudioClip invisible;


    protected override void Start()
    {
        
        maxHealth = 150;
        health = maxHealth;
        speed = 10;
        damage = 10;
        bullet.GetComponent<Bullet>().damage = damage;
        attackSpeed = 0.15f;
        lastAttackTime = Time.time + attackSpeed;
        rotateSpeed = 500f;
        bulletSpeed = 30f;
        skillCoolDownTime = 6f;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(isInvisible)
        {
            if(invisibleTime < maxInvisibleTime)
                invisibleTime += Time.deltaTime;
            else
            {
                isInvisible = false;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override GameObject[] Attack()
    {
        if (superAttack)
        {
            
            if (isInvisible)
                isInvisible = false;
            GameObject[] bulletList = new GameObject[20];
            for (int i = 0; i < 10; i++)
            {
                Quaternion q = Quaternion.AngleAxis(10 * (4.5f - i), Vector3.up);
                bulletList[2 * i] = Shoot(shootPoint, q);
                bulletList[2 * i + 1] = Shoot(shootPoint2, q);
            }
            superAttack = false;
            return bulletList;
        }
        return base.Attack();
    }

    private GameObject Shoot(GameObject shootPoint, Quaternion q)
    {
        GameObject newBullet = Instantiate(bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
        Rigidbody rigidBody = newBullet.GetComponent<Rigidbody>();
        Vector3 direction = q * shootPoint.transform.forward;
        rigidBody.velocity = direction * bulletSpeed;
        newBullet.GetComponent<Bullet>().shooterNetId = this.netId;
        //SoundManager.instance.PlaySingle(shoot1);			//Sound effect
	
        return newBullet;
    }

    protected override void UseSkill()
    {
        base.UseSkill();
        isInvisible = true;
		//SoundManager.instance.PlaySingle(invisible);        //Sound effect

    }

    private void ChangeState(bool isInvisible)
    {
        if(isInvisible)
        {
            GoInvisible();
        }
        else
        {
            Appear();
        }
    }

    private void GoInvisible()
    {
        superAttack = true;
        //Debug.Log("color:" + transform.GetChild(0).GetComponent<Renderer>().material.color);
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorInvisible;
        SetToLayer(gameObject.transform, 2);
    }

    private void SetToLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            SetToLayer(child, layer);
    }

    private void Appear()
    {
        invisibleTime = 0;
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorNormal;
        SetToLayer(gameObject.transform, 0);
    }

    public override void OnCollisionEnter(Collision collision)
    {
        
        base.OnCollisionEnter(collision);

        if(collision.collider.tag != "wall" && isInvisible)
        {
            isInvisible = false;
        }
        
    }

}
