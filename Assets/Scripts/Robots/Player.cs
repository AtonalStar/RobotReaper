using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum RobotType
{
    BlackGun,
    Ghost,
    Monster
}

public abstract class Player : Robot {

    [SyncVar]
    public bool isDying = false;
    [SyncVar]
    public bool isFinishLevel = false;
    protected float dyingTime;
    protected float maxDyingTime = 10f;
    protected float reviveTime;
    protected float maxReviveTime = 2f;
    protected float skillCoolDownTime;
    protected float skillCoolDownRemaining;
    private float reviveDistance = 3f;
    //public PlayerInfo playerInfo;

    public Color colorDying;

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer");
        //GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    protected override void Start()
    {
        if (!hasAuthority) return;
        base.Start();
        dyingTime = maxDyingTime;
        reviveTime = maxReviveTime;
        GameState.singleton.SetPlayers();
        HUDUI.singleton.UpdateSkill(skillCoolDownTime - skillCoolDownRemaining, skillCoolDownTime);
        HUDUI.singleton.UpdateHealth(health, maxHealth);
        //Debug.Log("Start()::" + localPlayerAuthority);

    }

    protected virtual void Update()
    {
        if (isDying)
        {
            DyingCheck();
            ReviveCheck();
            return;
        }
        SkillCoolDownUpdate();
        if (!hasAuthority)
        {
            return;
        }
        if (Input.GetButton("Fire1"))
        {
            CmdAttack();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdTryToUseSkill();
        }
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isDying)
        {
            return;
        }
        if (!hasAuthority)
        {
            return;
        }
        Aim(AimInput());
        Vector3 direction = MovementInput();
        Ray ray1 = new Ray(transform.position, new Vector3(direction.x, 0, 0));
        RaycastHit hit1;
        Physics.Raycast(ray1, out hit1, 1.5f);
        Ray ray2 = new Ray(transform.position, new Vector3(0, 0, direction.z));
        RaycastHit hit2;
        Physics.Raycast(ray2, out hit2, 1.5f);
        if (hit1.transform != null && hit1.transform.tag == "wall")
            direction = new Vector3(0, 0, direction.z);
        if (hit2.transform != null && hit2.transform.tag == "wall")
            direction = new Vector3(direction.x, 0, 0);

        Movement(direction);
    }

    [ClientRpc]
    public void RpcSetFollowCamera()
    {
        if (hasAuthority)
        {
            FindObjectOfType<FollowCamera>().player = this.gameObject;
        }
    }


    [Command]
    protected void CmdAttack()
    {
        GameObject[] bulletList = Attack();
        if (bulletList == null) return;
        for (int i = 0; i < bulletList.Length; i++)
        {
            NetworkServer.Spawn(bulletList[i]);
        }
    }


    public override void TakeDamage(Bullet bullet)
    {
        if (!isServer)
            return;
        //TODO: add sound and effect when get hit
        Debug.Log(transform.name + " player take damage, HP:" + health);
        TakeDamage(bullet.damage);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (health <= 0)
        {
            health = 0;
            isDying = true;
            RpcDying();
        }
        RpcUpdateHealth(health, maxHealth);
    }

    [ClientRpc]
    private void RpcUpdateHealth(int health, int maxHealth)
    {
        if (hasAuthority)
            HUDUI.singleton.UpdateHealth(health, maxHealth);
    }

    private void SkillCoolDownUpdate()
    {
        if (skillCoolDownRemaining > 0)
        {
            skillCoolDownRemaining -= Time.deltaTime;
            HUDUI.singleton.UpdateSkill(skillCoolDownTime-skillCoolDownRemaining, skillCoolDownTime);
            // TODO: Update HUD to display skill's cool down time
        }
    }

    [Command]
    private void CmdTryToUseSkill()
    {
        if (skillCoolDownRemaining > 0)
        {
            // TODO: show player that skill is in cool down time
            Debug.Log("Skill in CD time!");
            return;
        }
        else
        {
            UseSkill();
        }
    }

    protected virtual void UseSkill()
    {
        skillCoolDownRemaining = skillCoolDownTime;
    }

    protected void DyingCheck()
    {
        if (!isServer)
            return;
        if (dyingTime < 0 || GameState.singleton.players.Length <= 1)
        {
            isDying = false;
            RpcDie();
        }
        else
        {
            dyingTime -= Time.deltaTime;
        }
    }

    [ClientRpc]
    protected void RpcDying()
    {
        // Demo: change color to show dying state
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorDying;
        
    }


    protected void ReviveCheck()
    {
        if (!isServer)
            return;
        Player[] players = FindObjectsOfType<Player>();
        foreach(Player playerRobot in players)
        {
            if (playerRobot.netId == this.netId)
                continue;
            Debug.Log(playerRobot.transform.name);
            if (Vector3.Distance(transform.position, playerRobot.transform.position)
                < reviveDistance)
            {
                if (reviveTime < 0)
                {
                    RpcRevive();
                }
                else
                {
                    reviveTime -= Time.deltaTime;
                }
            }
        }
    }

    [ClientRpc]
    protected void RpcRevive()
    {
        reviveTime = maxReviveTime;
        dyingTime = maxDyingTime;
        health = maxHealth / 2;
        isDying = false;
        HUDUI.singleton.UpdateHealth(health, maxHealth);
        //Demo
        transform.GetChild(0).GetComponent<Renderer>().material.color = colorNormal;
    }

    private void PickUpHealthPack()
    {
        health += (health + maxHealth / 2);
        if (health > maxHealth)
            health = maxHealth;
    }

    protected Vector3 AimInput()
    {
#if true
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 aimPoint = hit.point;
            aimPoint.y = 0.5f;
            Vector3 aimingDirection = hit.point - transform.position;
            aimingDirection.y = 0f;
            return aimingDirection.normalized;
        }
        else
            return new Vector3(0,0,0);
#else

        return new Vector3(0,0,0);

#endif
    }

    protected Vector3 MovementInput()
    {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "HealthPack" && health != maxHealth)
        {
            PickUpHealthPack();
            HUDUI.singleton.UpdateHealth(health, maxHealth);
            Destroy(collision.gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "FinishArea")
        {
            Debug.Log("Level Finish");
            isFinishLevel = true;
            GameState.singleton.GameFinishCheck();
            
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "FinishArea")
        {
            isFinishLevel = false;
            Debug.Log("Level not Finish");
        }
    }


}
