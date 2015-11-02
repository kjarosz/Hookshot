﻿using UnityEngine;
using System.Collections;

public class LateralMovement : MonoBehaviour
{
    public float speed;
    public float speedInWater;
    private float regularSpeed;
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
        regularSpeed = speed;
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

    void OnTriggerStay2D(Collider2D c)
    {
        if (c.CompareTag("Water"))
            speed = speedInWater;
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (c.CompareTag("Water"))
            speed = regularSpeed;
    }

    void Move(float horizontal)
    {
        if (!hookshotControl.IsHooked())
        {
            Vector2 lateralForce = new Vector2(horizontal * moveForce, 0);

            if (Mathf.Abs(player.velocity.x) < speed && canMove)
                player.AddForce(lateralForce);

            if (player.velocity.x > 0 && horizontal < 0
             || player.velocity.x < 0 && horizontal > 0)
            {
                player.velocity = new Vector2(0, player.velocity.y);

                if (!jump.isGrounded())
                {
                    StartCoroutine(AirStopTime(AIR_STOP_TIME));
                }
            }
        }
        else
        {
            Vector2 pivotPoint = hookshotControl.HookPoint();
            if (horizontal > 0 && pivotPoint.x >= transform.position.x || horizontal < 0 && pivotPoint.x <= transform.position.x)
            {
                Vector2 lateralForce = Vector3.Cross((Vector3)pivotPoint - transform.position, Vector3.forward).normalized;                
                lateralForce *= horizontal * force / (player.velocity.magnitude + 1f);
                player.AddForce(lateralForce);
            }
        }
    }

    void Orient(float horizontal)
    {
        if (!hookshotControl.IsHooked() && horizontal != 0 && jump.isGrounded())
        {
            Quaternion rot = horizontal == 1 ?
                Quaternion.Euler(0, 0, -5.73f) : 
                Quaternion.Euler(0, 180, -5.73f);
            characterSprite.transform.rotation = rot;
        }
        else if (!jump.isGrounded())
        {
            //AimAtMouse();
        }
    }

    IEnumerator AirStopTime(float t)
    {
        canMove = false;

        yield return new WaitForSeconds(t);

        canMove = true;
    }

    void AimAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 playerPos = characterSprite.transform.position;
        Vector3 direction = mousePos - playerPos;
        direction = new Vector3(direction.x, direction.y, 0);
        Debug.Log(direction);
        Vector3 angles = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles;
        float flip = direction.x < 0 ? 180f : 0f;
        angles = new Vector3(0, 0, angles.z);
        characterSprite.transform.rotation = Quaternion.Euler(angles);
    }
}
