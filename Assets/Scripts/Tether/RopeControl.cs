﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Rope
{
    public float climbSpeed;
    public float boostSpeed;

    public float minLength;
    public float maxLength;

    public float initialDistancePortion;

    public int segmentCount;
    public float maxRopeSagDistance;
}

public class RopeControl : MonoBehaviour {
    private GameObject player;
    private Rigidbody2D playerBody;
    private SpriteRenderer playerRenderer;

    public HookshotControl hookshot;
    public GameObject hook;

    public Rope ropeProperties;
    public Vector2 anchorOffset;

    private LineRenderer line;
    private DistanceJoint2D rope;
    private SpringJoint2D spring;

    private WallSensor leftWallSensor;
    private WallSensor rightWallSensor;
    private float moveForce;

    private bool boostEnabled;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerBody = player.GetComponent<Rigidbody2D>();
        playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        FindWallSensors();
        EstablishConstants();
        line = GetComponent<LineRenderer>();
        boostEnabled = false;
    }

    void FindWallSensors() 
    {
        WallSensor[] sensors = player.GetComponentsInChildren<WallSensor>();
        foreach(WallSensor sensor in sensors) {
            if (sensor.name == "WallSensorL")
                leftWallSensor = sensor;
            else if (sensor.name == "WallSensorR")
                rightWallSensor = sensor;
        }
    }

    void EstablishConstants()
    {
        LateralMovement movement = player.GetComponent<LateralMovement>();
        moveForce = movement.moveForce;
    }

    void Update() {
        if (hookshot.IsHooked()) {
            boostEnabled = Input.GetButton("Fire2");
        } else if (Vector3.Distance(hookshot.transform.position, hook.transform.position) > ropeProperties.maxLength) {
            hookshot.RetractRope();
        }
    }

    void FixedUpdate()
    {
        if (hookshot.IsHooked()) {
            ControlRope();
            RotateObjectTowardsRope();
        }
        DrawRope();
    }

    void ControlRope()
    {
        // When player is touching a wall, make the player walk up the wall.
        if (isTouchingWall())
        {
            MoveAlongWall();
        }
        else
        {
            float vertical = boostEnabled ? ropeProperties.boostSpeed : Input.GetAxis("Vertical");
            float distance = vertical * ropeProperties.climbSpeed * Time.fixedDeltaTime;
            rope.distance = Mathf.Clamp(rope.distance - distance,
                                        ropeProperties.minLength,
                                        ropeProperties.maxLength);
        }
    }

    private bool isTouchingWall()
    {
        return leftWallSensor.IsWallCollide() || rightWallSensor.IsWallCollide();
    }

    private void FaceWall()
    {
        playerRenderer.transform.rotation = Quaternion.Euler(0, 
            leftWallSensor.IsWallCollide() ? 180f : 0f, 90f);
    }

    private void MoveAlongWall()
    {
        float vertical = Input.GetAxis("Vertical");
        if (vertical > 0) { 
        Vector2 lateralForce = new Vector2(0, vertical * moveForce);
        if (Mathf.Abs(playerBody.velocity.y) < ropeProperties.climbSpeed)
            playerBody.AddForce(lateralForce);

            rope.distance = Mathf.Clamp(PhysicalRopeLength(),
                                        ropeProperties.minLength,
                                        ropeProperties.maxLength);
        }
        else
        {
            float distance = vertical * ropeProperties.climbSpeed * Time.fixedDeltaTime;
            rope.distance = Mathf.Clamp(rope.distance - distance,
                                        ropeProperties.minLength,
                                        ropeProperties.maxLength);
        }
    }

    private float PhysicalRopeLength()
    {
        Vector2 connPos = rope.connectedBody.transform.position;
        Vector2 startPos = rope.transform.position + rope.transform.rotation * rope.anchor;
        return Vector2.Distance(connPos, startPos);
    }

    void RotateObjectTowardsRope()
    {

        Transform spriteTransform = playerRenderer.gameObject.transform;

        Vector2 jointDirection = hook.transform.position - spriteTransform.position;
        spriteTransform.rotation = Quaternion.FromToRotation(Vector2.right, jointDirection);

        rope.anchor = playerRenderer.transform.localPosition + spriteTransform.rotation * anchorOffset;

        if(isTouchingWall())
            FaceWall();
    }


    public void AttachRope()
    {
        MakeRope();
        DrawRope();
    }

    private void MakeRope()
    {
        float initialDistance = Vector2.Distance(player.transform.position, hook.transform.position);
        initialDistance *= ropeProperties.initialDistancePortion;

        rope = player.AddComponent<DistanceJoint2D>();
        rope.connectedBody = hook.GetComponent<Rigidbody2D>();
        rope.distance = Mathf.Clamp(initialDistance, ropeProperties.minLength, ropeProperties.maxLength);
        rope.maxDistanceOnly = true;

        line.SetVertexCount(ropeProperties.segmentCount);

        RotateObjectTowardsRope();
    }

    public void DetachRope()
    {
        if (hookshot.IsHooked())
        {
            playerRenderer.gameObject.transform.rotation = Quaternion.identity;
            DestroyObject(rope);
        }
    }

    void DrawRope()
    {
        if (hookshot.IsHooked())
        {
            DrawCurvyLine();
        }
        else
        {
            line.SetPosition(0, GetPlayerOffset());
            line.SetPosition(1, hook.transform.position);
        }
    }

    private Vector2 GetPlayerOffset()
    {
        return player.transform.position
              + playerRenderer.transform.localPosition
              + playerRenderer.transform.rotation * anchorOffset;
    }

    private void DrawCurvyLine()
    {
        Vector2 P0 = GetPlayerOffset();
        Vector2 Pn = hook.transform.position;

        line.SetPosition(0, P0);
        for (int i = 1; i < ropeProperties.segmentCount - 1; i++)
            DrawVertex(i, P0, Pn);
        line.SetPosition(ropeProperties.segmentCount - 1, Pn);
    }

    private void DrawVertex(int n, Vector2 p0, Vector2 pn)
    {
        Vector2 jointDir = (pn - p0).normalized;
        Vector2 jointPos = p0 + jointDir * Vector2.Distance(pn, p0) * ((float)n / ropeProperties.segmentCount);
        Vector2 orthogonal = Vector3.Cross(Vector3.forward, jointDir).normalized;

        Vector2 midpoint = (pn + p0) / 2.0f;
        float midDist = Vector2.Distance(midpoint, p0);

        Vector2 offsetDir = Vector3.Project(playerBody.velocity, orthogonal).normalized;
        float offsetMag = ropeProperties.maxRopeSagDistance * Mathf.Sin(3.14f / 2.0f * (1.0f - Vector2.Distance(jointPos, midpoint) / midDist));
        Vector2 offset = offsetDir * offsetMag;

        line.SetPosition(n, jointPos + offset);
    }
}
