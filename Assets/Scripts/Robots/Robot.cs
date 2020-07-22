using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class Robot : NetworkBehaviour {

    protected int maxHealth;
    [SerializeField]
    [SyncVar]
    protected int health;
    protected int speed;
    protected int damage;
    protected float attackSpeed; // attacks cool down time
    protected float lastAttackTime;
    protected float rotateSpeed;
    protected float bulletSpeed;
    public GameObject bullet;
    public GameObject shootPoint;
    public GameObject shootPoint2;
    public Image healthBar;

    //Demo
    protected Color colorNormal;
    public Color colorDead;

    protected virtual void Start()
    {
        colorNormal = transform.GetChild(0).GetComponent<Renderer>().material.color;
    }

    protected virtual void FixedUpdate()
    {
        //transform.rotation = new Quaternion().SetAxisAngle(Vector3.up,transform.rotation.ToAxisAngle());
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }

    public virtual void TakeDamage (Bullet bullet)
    {
        //TODO: add sound and effect when get hit
        TakeDamage(bullet.damage);
        healthBar.rectTransform.sizeDelta = new Vector2(200 * ((float)health / maxHealth), healthBar.rectTransform.sizeDelta.y);
    }

    public virtual void TakeDamage (int damage)
    {
        if (!isServer)
            return;
        //TODO: add sound and effect when get hit
        health -= damage;
        if (health <= 0)
        {
            RpcDie();
        }
        Debug.Log(transform.name + " take damage, HP:" + health);
    }

    [ClientRpc]
    public void RpcDie ()
    {
        //TODO: behaviour before dead (show explosion effects)
        //transform.GetChild(0).GetComponent<Renderer>().material.color = colorDead;
        //this.enabled = false;
        Destroy(gameObject);
    }

    protected bool AttackTimeCheck()
    {
        if (Time.time - lastAttackTime < attackSpeed)
        {
            return false;
        }
        lastAttackTime = Time.time;
        return true;
    }

    protected virtual GameObject[] Attack()
    {
        if (!AttackTimeCheck())
        { 
            return null;
        }
        GameObject[] bulletList = new GameObject[2];
        bulletList[0] = Shoot(shootPoint);
        bulletList[1] = Shoot(shootPoint2);
        return bulletList;
        //NetworkServer.Spawn(newBullet);
    }

    protected virtual GameObject Shoot(GameObject shootPoint)
    {
        GameObject newBullet = Instantiate(bullet, shootPoint.transform.position, shootPoint.transform.rotation) as GameObject;
        Rigidbody rigidBody = newBullet.GetComponent<Rigidbody>();
        rigidBody.velocity = shootPoint.transform.forward * bulletSpeed;
        newBullet.GetComponent<Bullet>().shooterNetId = this.netId;
        return newBullet;
    }

    protected void Movement(Vector3 direction)
    {
        direction = direction.normalized;
        /*
        Vector3 offset = new Vector3(-direction.y, direction.x, 0);
        Ray ray1 = new Ray(transform.position + offset, direction);
        RaycastHit hit1;
        Physics.Raycast(ray1, out hit1, 1.5f);
        Ray ray2 = new Ray(transform.position - offset, direction);
        RaycastHit hit2;
        Physics.Raycast(ray2, out hit2, 1.5f);

        if ((hit1.transform != null && hit1.transform.tag == "wall")
          ||(hit2.transform != null && hit2.transform.tag == "wall"))
            return;
            */
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
    }

    protected bool Aim(Vector3 direction)
    {
        Vector3 rotateDirection = Vector3.Cross(direction, transform.forward);
        float angle = Vector3.Angle(direction, transform.forward);
        if (angle > rotateSpeed * Time.deltaTime)
        {
            if (rotateDirection.y > 0)
            {
                transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
            }
            else if (rotateDirection.y < 0)
            {
                transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            }
        }
        else
        {
            transform.forward = direction;
            return true;
        }
        return false;
    }

    protected void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.volume = SoundManager.instance.SEVolume;
        audioSource.clip = clip;
        audioSource.Play();
    }

}
