﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HookshotControl : MonoBehaviour {
    enum HookshotState {
        READY,
        EXTENDING,
        RETRACTING,
        HOOKED,
        FLYING
    }
    private delegate void StateFunction();
    private StateFunction[] stateProcesses;

    void MapStateFunctions()
    {
        stateProcesses = new StateFunction[] {
            this.Ready,
            this.Extend,
            this.Retract,
            this.Hooked,
            this.Flying
        };
    }

    public GameObject hookFab;
    public GameObject ropeFab;

    public float retractTime;

    private List<Collider2D> playerColliders;
    private GameObject hand;
    private RopeControl rope;
    private AimAtMouse mouseAimer;
    private GameObject hook;

    private KeybindScript keybinds;
    private HookshotState state;
    private Vector2 retractPoint;
    private float stateSwitchTime;

    private LateralMovement player;
    private GameObject playerRenderer;
    private JumpControl jumpControl;
    private AnimateFrog frogAnim;

    private GameObject ropeObj;

    void Start()
    {
        MapStateFunctions();
        keybinds = GameObject.FindGameObjectWithTag("KeyBinds").GetComponent<KeybindScript>();
        FindPlayerParts();
        ChangeState(HookshotState.READY);
    }

    private void FindPlayerParts()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<LateralMovement>();
        playerRenderer = player.getSprite();
        frogAnim = playerRenderer.GetComponent<AnimateFrog>();
        jumpControl = player.GetComponent<JumpControl>();
        FindPlayerColliders();
        hand = transform.parent.gameObject;
        mouseAimer = hand.GetComponent<AimAtMouse>();
    }

    private void FindPlayerColliders()
    {
        playerColliders = new List<Collider2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player Sprite");
        foreach (Collider2D shittyCollider in player.GetComponents<Collider2D>())
            playerColliders.Add(shittyCollider);
    }

    void ChangeState(HookshotState newState)
    {
        state = newState;
        stateSwitchTime = Time.time;
        if (CanMouseAim()) {
            mouseAimer.ToggleAiming(true);
        } else {
            mouseAimer.ToggleAiming(false);
        }
    }

	void Update () 
    {
        stateProcesses[(int)state]();
	}

    void Ready()
    {
        UpdateHookFire();
    }

    void Extend() { /* The hook object is traveling through the world. */
        StartCoroutine(frogAnim.PlayHooked());
    }

    void Hooked()
    {
        if (Input.GetButtonDown("Fire1") || keybinds.GetButtonDown("Jump"))
        {
            DestroyHookAndRope();
            ChangeState(HookshotState.FLYING);
        }

        RotatGunToFaceHook();
    }

    void Flying()
    {
        UpdateHookFire();

        if (jumpControl.isGrounded())
        {
            ChangeState(HookshotState.READY);
        }
    }

    void UpdateHookFire()
    {
        if (Input.GetButtonDown("Fire1")/* && !jumpControl.isGrounded()*/)
        {
            FireHookAndRope();
            ChangeState(HookshotState.EXTENDING);
        }
    }

    private bool CanMouseAim()
    {
        return state == HookshotState.READY || state == HookshotState.FLYING;
    }

    void Retract()
    {
        float t = (Time.time - stateSwitchTime) / retractTime;
        Vector2 targetPosition = transform.position;
        hook.transform.position = Vector3.Lerp(retractPoint, targetPosition, t);
        if (t >= 1.0f)
        {
            DestroyHookAndRope();
            ChangeState(HookshotState.READY);
        }
    }

    void FireHookAndRope()
    {
        // Shoot out a hook instance
        hook = (GameObject)Instantiate(hookFab, transform.position, transform.rotation);
        hook.GetComponent<Hook>().hookGun = this;

        IgnoreHookPlayerCollisions();

        // And spawn a rope to go with it
        ropeObj = (GameObject)Instantiate(ropeFab, transform.position, transform.rotation);
        rope = ropeObj.GetComponent<RopeControl>();
        rope.hookshot = this;
        rope.hook = hook;
    }

    void IgnoreHookPlayerCollisions()
    {
        Collider2D hookCollider = hook.GetComponent<CircleCollider2D>();
        foreach(Collider2D playerCollider in playerColliders)
            Physics2D.IgnoreCollision(hookCollider, playerCollider, true);
    }

    void DestroyHookAndRope()
    {
        Destroy(hook);
        if (rope != null) {
            rope.DetachRope();
            Destroy(rope.gameObject);
        }
    }

    public void HookOn()
    {
        ChangeState(HookshotState.HOOKED);
        rope.AttachRope();
    }

    public void RetractRope()
    {
        retractPoint = hook.transform.position;
        hook.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        ChangeState(HookshotState.RETRACTING);
    }

    public bool IsHooked()
    {
        return state == HookshotState.HOOKED; 
    }

    public bool IsExtending()
    {
        return state == HookshotState.EXTENDING;
    }

    public bool IsFlying()
    {
        return state == HookshotState.FLYING;
    }

    public Transform HookPoint()
    {
        return hook == null ? null : hook.transform;
    }

    public void CancelHook()
    {
        if(state != HookshotState.READY)
            DestroyHookAndRope();
        ChangeState(HookshotState.READY);
    }

    public GameObject Rope()
    {
        return ropeObj;
    }

    void RotatGunToFaceHook()
    {
        hand.transform.rotation = Quaternion.FromToRotation(
            Vector2.right, 
            hook.transform.position - hand.transform.position
        );
    }

    void AimAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 playerPos = playerRenderer.transform.position;
        Vector3 direction = mousePos - playerPos;
        direction = new Vector3(direction.x, direction.y, 0);
        Vector3 angles = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles;
        angles = new Vector3(0, 0, angles.z);
        playerRenderer.transform.rotation = Quaternion.Euler(angles);
    }
}
