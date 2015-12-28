﻿using UnityEngine;
using System.Collections.Generic;

public class InfiniteRunGenerator : MonoBehaviour
{
    public float parallaxBackgroundSpeed;

    public GameObject backgroundPrefab;
    private GameObject backgroundFolder;
    private List<GameObject> backgrounds;

    private float bgWidth;
    public float levelPartWidth;

    private Dictionary<string, GameObject> prefabs;
    private Dictionary<int, string> indexes;
    private Dictionary<int, GameObject> indexedGameObjects;

    private Vector2 deathBoxPos;
    private Vector2 backgroundPos;
    private GameObject bgFolder;

    private bool doneRespawning;

    private int oldSection = 0;
    private int section = 0;
    private int oldParallaxSection = 0;
    private int parallaxSection = 0;

    public delegate void RespawnAction();
    public static event RespawnAction Respawn;

    /* ********************************************************************* */
    //                                Start Up                    
    /* ********************************************************************* */
    void Awake()
    {
        KillEnemies.OnRespawn += DoneRespawning;
        InitalizeVariables();
        FindObjects();
        LoadAssets();
        InitalizeEnvironment();
    }

    void InitalizeVariables()
    {
        prefabs = new Dictionary<string, GameObject>();
        indexes = new Dictionary<int, string>();
        indexedGameObjects = new Dictionary<int, GameObject>();

        backgroundFolder = new GameObject("Background");
        backgroundFolder.transform.position = Vector2.zero;
        backgrounds = new List<GameObject>();

        //Make the backgrounds overlap just a little bit, to prevent the white back-background from showing
        bgWidth = (backgroundPrefab.GetComponent<SpriteRenderer>().sprite.bounds.extents.x * 2) - 0.01f;
        backgroundPos = Vector2.zero;
        deathBoxPos = backgroundPos + (Vector2.down * 5);

        doneRespawning = true;
    }

    void FindObjects()
    {
        bgFolder = GameObject.Find("Background");
    }

    void LoadAssets()
    {
        foreach(GameObject g in Resources.LoadAll("Level Parts"))
        {
            indexes.Add(prefabs.Count, g.name);
            prefabs.Add(g.name, g);
        }
        if (prefabs.Count > 0)
            InitalizeLevel();
    }

    void InitalizeEnvironment()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject background = (GameObject)Instantiate(backgroundPrefab, backgroundPos + (Vector2.right * bgWidth * i), Quaternion.identity);
            backgrounds.Add(background);
            backgrounds[i].transform.parent = bgFolder.transform;
        }
    }

    void InitalizeLevel()
    {
        indexedGameObjects.Add(-1, GetRandomLevelPart(Vector2.left * levelPartWidth));
        indexedGameObjects.Add(0, (GameObject)Instantiate(prefabs["Start"], Vector2.zero, Quaternion.identity));
        indexedGameObjects.Add(1, GetRandomLevelPart(Vector2.right * levelPartWidth));

        foreach (KeyValuePair<int, GameObject> pair in indexedGameObjects)
        {
            foreach (Transform part in pair.Value.transform)
            {
                if (part.GetComponent<SpringJoint2D>() != null)
                {
                    part.GetComponent<SpringJoint2D>().connectedAnchor 
                        = (Vector2)(part.transform.position) + (Vector2.up * 0.4f);
                }
            }
        }
    }

    GameObject GetRandomLevelPart(Vector2 position)
    {
        string randomIndex = indexes[(int)(Random.value * prefabs.Count)];
        return (GameObject)Instantiate(prefabs[randomIndex], position, Quaternion.identity);
    }

    /* ********************************************************************* */
    //                             Process
    /* ********************************************************************* */
    void Update()
    {
        UpdateLevelParts();
        ParallaxBackground();
        CheckForDeath();
    }

    void UpdateLevelParts()
    {
        oldSection = section;
        float pos = transform.position.x;
        section = (int)(pos / levelPartWidth);
        if (section != oldSection)
        {
            int direction = section - oldSection;
            bool canGenerate = !indexedGameObjects.ContainsKey(section);
            bool canGenerateAhead = !indexedGameObjects.ContainsKey(section + direction);
            if (canGenerate)
            {
                GenerateSection(section);
            }
            if (canGenerateAhead)
            {
                GenerateSection(section + direction);
            }
            DetermineVisibleSections();
        }
    }

    void GenerateSection(int index)
    {
        int i = (int)(Random.value * prefabs.Count);
        indexedGameObjects.Add(index, (GameObject)Instantiate(prefabs[indexes[i]],
            Vector2.right * levelPartWidth * index, Quaternion.identity));

        foreach (Transform part in indexedGameObjects[index].transform)
        {
            if (part.GetComponent<SpringJoint2D>() != null)
            {
                part.GetComponent<SpringJoint2D>().connectedAnchor
                    = (Vector2)(part.transform.position) + (Vector2.up * 0.4f);
            }
        }
    }

    void ParallaxBackground()
    {
        foreach(GameObject bg in backgrounds)
        {
            bg.GetComponent<Rigidbody2D>().velocity = Vector2.right * (GetComponent<Rigidbody2D>().velocity.x / GetComponent<LateralMovement>().speed) * parallaxBackgroundSpeed;
        }
        oldParallaxSection = parallaxSection;
        float positionXFromCenter = transform.position.x - backgrounds[1].transform.position.x;
        parallaxSection = (int)(positionXFromCenter / bgWidth);
        if(parallaxSection != oldParallaxSection)
        {
            MoveSection();
        }
    }

    void MoveSection()
    {
        for (int i = 0; i < backgrounds.Count; i++)
        {
            Vector2 offset = Vector2.right * bgWidth * parallaxSection;
            backgrounds[i].transform.position += (Vector3)offset;
        }
    }

    void DetermineVisibleSections()
    {
        foreach(KeyValuePair<int, GameObject> pair in indexedGameObjects)
        {
            pair.Value.SetActive(Mathf.Abs(pair.Key - section) <= 1);
        }
    }

    void CheckForDeath()
    {
        if (transform.position.y < deathBoxPos.y && doneRespawning)
        {
            if (Respawn != null)
            {
                Respawn();
                doneRespawning = false;
            }
        }
    }

    void DoneRespawning()
    {
        doneRespawning = true;
        ScoreTracker.ResetScore();
    }

    /* ********************************************************************* */
    //                              Clean Up                      
    /* ********************************************************************* */
    void OnDestroy()
    {
        KillEnemies.OnRespawn -= DoneRespawning;
    }
}
