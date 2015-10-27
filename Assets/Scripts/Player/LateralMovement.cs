﻿using UnityEngine;
using System.Collections;

public class LateralMovement : MonoBehaviour
{
    public float speed;
    public float force;
    public float moveForce;

    public HookshotControl hookshotControl;
    public SpriteRenderer characterSprite;
    private JumpControl jump;
    private Rigidbody2D player;
    private Vector2 contactNormal;

    public WallSensor wallSensorRight;
    public WallSensor wallSensorLeft;

    private const float AIR_STOP_TIME = 0.08f;
    private bool canMove;

    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        canMove = true;
        jump = GetComponent<JumpControl>();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        Move(horizontal);
        Orient(horizontal);
    }

    void Move(float horizontal)
    {
        int traverse = 1;

        //if (!checkWalls(horizontal))
            //traverse = 0;

        if (!hookshotControl.IsHooked())
        {
            Vector2 lateralForce = new Vector2(horizontal * moveForce * traverse, 0);

            if (Mathf.Abs(player.velocity.x) < speed && canMove)
                player.AddForce(lateralForce);

            if (player.velocity.x > 0 && horizontal < 0
             || player.velocity.x < 0 && horizontal > 0)
            {
                player.velocity = new Vector2(0, player.velocity.y);

                if (!jump.isGrounded())
                {
                    StartCoroutine(AirStopTime());
                }
            }
        }
        else
        {
            Vector2 pivotPoint = hookshotControl.HookPoint();
            if (horizontal > 0 && pivotPoint.x >= transform.position.x || horizontal < 0 && pivotPoint.x <= transform.position.x)
            {
                Vector2 lateralForce = Vector3.Cross((Vector3)pivotPoint - transform.position, Vector3.forward).normalized;                
                lateralForce *= horizontal * force * traverse/ (player.velocity.magnitude + 1f);
                player.AddForce(lateralForce);
            }
        }
    }

    void Orient(float horizontal)
    {
        if (!hookshotControl.IsHooked() && horizontal != 0)
        {
            Quaternion rot = horizontal == 1 ? Quaternion.Euler(0, 0, -5.73f) : Quaternion.Euler(0, 180, -5.73f);
            characterSprite.transform.rotation = rot;
        }
    }

    IEnumerator AirStopTime()
    {
        canMove = false;

        yield return new WaitForSeconds(AIR_STOP_TIME);

        canMove = true;
    }

    /*bool checkWalls(float d)
    {
        if (d > 0)
        {
            return wallSensorRight.emptySpace;
        }
        else if (d < 0)
        {
            return wallSensorLeft.emptySpace;
        }
        else
            return true;
    }*/
}
