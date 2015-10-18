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
}

public class RopeControl : MonoBehaviour {
    private GameObject player;
    private SpriteRenderer playerRenderer;

    public HookshotControl hookshot;
    public GameObject hook;

    public Rope ropeProperties;
    public Vector2 anchorOffset;

    public GameObject springPrefab;

    private LineRenderer line;
    private DistanceJoint2D rope;
    private SpringJoint2D spring;

    private bool attached;
    private bool boostEnabled;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        line = GetComponent<LineRenderer>();
        attached = false;
        boostEnabled = false;
	}

	void Update () {
        if (attached) {
            boostEnabled = Input.GetButton("Fire2");
        } else if (Vector3.Distance(hookshot.transform.position, hook.transform.position) > ropeProperties.maxLength) {
            hookshot.RetractRope();
        }
    }

    void FixedUpdate()
    {
        if (attached) {
            ControlRope();
            RotateObjectTowardsRope();
        }
        DrawRope();
    }

    void ControlRope()
    {
        float vertical = boostEnabled ? ropeProperties.boostSpeed : Input.GetAxis("Vertical");
        float distance = vertical * ropeProperties.climbSpeed * Time.fixedDeltaTime;
        rope.distance = Mathf.Clamp(rope.distance - distance, 
                                    ropeProperties.minLength, 
                                    ropeProperties.maxLength);
    }

    public void AttachRope()
    {
        MakeRope();
        attached = true;
        DrawRope();
    }

    private void MakeRope()
    {
        // Adding spring joint first. The rope will attach to it
        float initialDistance = Vector2.Distance(player.transform.position, hook.transform.position);
        initialDistance *= ropeProperties.initialDistancePortion;

        // Rope Joint 
        rope = player.AddComponent<DistanceJoint2D>();
        rope.connectedBody = hook.GetComponent<Rigidbody2D>();
        rope.distance = Mathf.Clamp(initialDistance, ropeProperties.minLength, ropeProperties.maxLength);
        rope.maxDistanceOnly = true;

        RotateObjectTowardsRope();
    }

    public void DetachRope()
    {
        if (attached)
        {
            playerRenderer.gameObject.transform.rotation = Quaternion.identity;
            DestroyObject(rope);
        }
        attached = false;
    }

    void RotateObjectTowardsRope()
    {
        Transform spriteTransform = playerRenderer.gameObject.transform;

        Vector2 jointDirection = hook.transform.position - spriteTransform.position;
        spriteTransform.rotation = Quaternion.FromToRotation(Vector2.right, jointDirection);

        rope.anchor = playerRenderer.transform.localPosition + spriteTransform.rotation * anchorOffset;
    }

    void DrawRope()
    {
        if (attached)
            line.SetPosition(0, rope.transform.position + (Vector3)rope.anchor);
        else
            line.SetPosition(0, player.transform.position 
                              + playerRenderer.transform.localPosition 
                              + playerRenderer.transform.rotation * anchorOffset);
        line.SetPosition(1, hook.transform.position);
    }
}