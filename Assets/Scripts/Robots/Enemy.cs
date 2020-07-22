using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Robot {

    protected int detectRangeRadius = 30;
    protected Player lockedPlayer = null;
    public GameObject ScanPoint;

    /*
    public override void OnStartServer()
    {
        players = FindObjectsOfType<Player>();
        Debug.Log("Number of player = " + players.Length);
    }
    */
    //[Command]
    

    protected override void Start()
    {
        base.Start();
        
    }

    protected virtual void Update()
    {
        if(GameState.singleton.players == null)
        {
            return;
        }
        if (lockedPlayer != null)
        {
            CheckLockedPlayer();
            return;
        }
        DetectPlayer();
    }

    // will lock the first detected player
    private void DetectPlayer()
    {
        foreach(Player playerRobot in GameState.singleton.players)
        {
            if (playerRobot == null)
            {
                GameState.singleton.SetPlayers();
                return;
            }
            if (Vector3.Distance(transform.position, playerRobot.transform.position)
                < detectRangeRadius)
            {
                Ray ray = new Ray(ScanPoint.transform.position, playerRobot.transform.position - ScanPoint.transform.position);
                Debug.DrawRay(ScanPoint.transform.position, playerRobot.transform.position - ScanPoint.transform.position);
                
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 20f, ~(1 << 2));
                if (hit.transform != null && hit.transform.tag == "Player")
                {
                    lockedPlayer = playerRobot;
                    break;
                }
            }
        }
    }

    private void CheckLockedPlayer()
    {
        Ray ray = new Ray(ScanPoint.transform.position, lockedPlayer.transform.position - ScanPoint.transform.position);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 20f, ~(1 << 2));
        if (hit.transform != null && hit.transform.tag == "Player")
        {
            return;
        }
        
        lockedPlayer = null;
        Debug.Log("locked lost");////////////////////////////
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
