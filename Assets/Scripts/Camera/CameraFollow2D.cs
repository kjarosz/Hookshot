﻿using UnityEngine;
using System.Collections;

public class CameraFollow2D : MonoBehaviour
{
    public int numberOfBgLayers;
    public float dampTime;
    public Transform[] Layers;
    public float[] speedOfLayers;
    private Vector3 velocity = Vector3.zero;
    private Transform target;
    public float zoomSpeed;
    public float minZoom;
    public float maxZoom;
    public float rotationSpeed;
    public bool followY;
    public bool followX;
    public bool LimitCameraPos;
    public Vector2 minPos;
    public Vector2 maxPos;
    bool displayBackground = true;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
        Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
        float yCoordinate = 0;
        float xCoordinate = 0;
        Vector3 destination = Vector3.zero;
        if (followY)
        {
            yCoordinate = (transform.position.y + delta.y);
        }
        else
        {
            yCoordinate = transform.position.y;
        }

        if (followX)
        {
            xCoordinate = transform.position.x + delta.x;
        }
        else
        {
            xCoordinate = transform.position.x;
        }

        if (LimitCameraPos)
        {
            if (followY)
            {
                if (yCoordinate > maxPos.y)
                    yCoordinate = maxPos.y;
                else if (yCoordinate < minPos.y)
                    yCoordinate = minPos.y;
            }

            if (followX)
            {
                if (xCoordinate > maxPos.x)
                    xCoordinate = maxPos.x;
                else if (xCoordinate < minPos.x)
                    xCoordinate = minPos.x;
            }
        }

        destination = new Vector3(xCoordinate, yCoordinate, transform.position.z + delta.z);

        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        Parallax(destination);

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 300, 100, 20), "Background"))
        {
            HideBackground();
        }
    }

    void Parallax(Vector3 newPos)
    {
        for (int a = 0; a < numberOfBgLayers; a++)
        {
            Layers[a].position = Vector3.SmoothDamp(Layers[a].position, new Vector3(newPos.x - (newPos.x - minPos.x) * (speedOfLayers[a]/100), newPos.y, Layers[a].position.z), ref velocity, dampTime);
        }
    }

    void HideBackground()
    {
        Material material;
        Color color;
        int alpha = 1;

        if (displayBackground == true)
        {
            alpha = 0;
            displayBackground = false;
        }
        else
        {
            alpha = 1;
            displayBackground = true;
        }

        for (int b = 0; b < numberOfBgLayers; b++)
        {
            material = Layers[b].GetComponent<SpriteRenderer>().material;
            color = material.color;
            material.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}