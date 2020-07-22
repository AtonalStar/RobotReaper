using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Monster : Player
{
    private float maxShieldTime = 4f;
    private float shieldTime = 0;
    private float maxReflectTime = 1f;
    private float reflectTime = 0;
    public GameObject shootPoint3;

	//Sound Effect
	public AudioClip shoot2;
	public AudioClip useShield;

    enum State
    {
        Normal,
        Reflect,
        Shield
    };
    [SyncVar(hook = "ChangeState")]
    private State state = State.Normal;
    //[SyncVar]
    //private int stateNum;

    // used for demo
    //public Color colorNormal;
    public Color colorShield;
    public Color colorReflect;
    protected override void Start()
    {
        maxHealth = 400;
        health = maxHealth;
        speed = 5;
        damage = 30;
        bullet.GetComponent<Bullet>().damage = damage;
        attackSpeed = 0.5f;
        lastAttackTime = Time.time + attackSpeed;
        rotateSpeed = 100f;
        bulletSpeed = 30f;
        skillCoolDownTime = 10f;
        base.Start();
        //transform.GetChild(0).GetComponent<Renderer>().material.color = colorNormal;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
        switch(state)
        {
            case State.Normal:
                break;
            case State.Reflect:
                Debug.Log("reflectTime ing");
                if (reflectTime < maxReflectTime)
                    reflectTime += Time.deltaTime;
                else
                    state = State.Shield;
                break;
            case State.Shield:
                if (shieldTime < maxShieldTime)
                    shieldTime += Time.deltaTime;
                else
                    state = State.Normal;
                break;
        }
    }

    private void ChangeState(State nextState)
    {
        switch(nextState)
        {
            case State.Reflect:
                EnableReflect();
                break;
            case State.Shield:
                EnableShield();
                break;
            case State.Normal:
                ShieldFinish();
                break;
        }
    }

    public override void TakeDamage(Bullet bullet)
    {
        switch (state)
        {
            case State.Normal:
                base.TakeDamage(bullet);
                break;
            case State.Reflect:
                bullet.damage /= 2;
                CmdReflect(bullet.gameObject);
                break;
            case State.Shield:
                bullet.damage /= 2;
                base.TakeDamage(bullet);
                break;
        }
    }
    /*

    public override void TakeDamage(int damage)
    {
        Debug.Log("M::TakeDamage(int)");
        if (!isServer)
            return;
        switch (state)
        {
            case State.Normal:
                base.TakeDamage(damage);
                break;
            case State.Reflect:
            case State.Shield:
                damage /= 2;
                base.TakeDamage(damage);
                break;
        }
    }
    */
    [Command]
    private void CmdReflect(GameObject otherBullet)
    {
        GameObject temp = this.bullet;
        this.bullet = otherBullet.gameObject;
        NetworkServer.Spawn(Shoot(shootPoint3));
        this.bullet = temp;
    }

    protected override void UseSkill()
    {
        base.UseSkill();
        state = State.Reflect;
		SoundManager.instance.PlaySingle (useShield);//Sound Effect
    }

    private void EnableReflect()
    {
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorReflect;

        // TODO: use animation to show the reflect state
    }


    private void EnableShield()
    {
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorShield;

    }

    private void ShieldFinish()
    {
        shieldTime = 0;
        reflectTime = 0;
        // Demo
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorNormal;
    }
}
