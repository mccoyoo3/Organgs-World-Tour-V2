﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string sceneName;
    public string nextScene;
    private GameObject firstDoor;
    private Camera mCamera;
    private float respawnTimeStart;
    private bool respawn;
    private float respawnTime = 2f;
    private Animator anim;

    [Header("LEVEL MANAGER")]
    [SerializeField]
    private GameObject player;
    public Transform respawnPoint;
    [Header("COLLIDERS")]
    public Collider2D[] ground;
    public Collider2D[] trap;
    public Collider2D[] slipperyground;
    public Collider2D[] sugarplatform;
    public Collider2D[] throughplatforms;
    public Collider2D[] solidplatforms;
    public RaycastHit2D[] groundRay = new RaycastHit2D[1];
    public GameObject Rplayer { get; set; }
    
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        mCamera = Camera.main;
        firstDoor = GameObject.Find("door");
        anim = firstDoor.GetComponent<Animator>();

        if(Fader.Instance != null)
        {
            Fader.Instance.FadeImmediately();
            CountdownTimer.Instance.ResetTimer();
        }

        Invoke("FirstSpawn", 1);
    }

    private void FirstSpawn()
    {
        StartCoroutine(Spawnplayer());
    }

    private void Update()
    {
        CheckRespawn();
    }

    public void Respawn()
    {
        var newMask = mCamera.cullingMask & ~(1 << 2);
        respawnTimeStart = Time.time;
        mCamera.cullingMask = newMask;
        respawn = true;
    }

    private void CheckRespawn()
    {
        if(Time.time >= respawnTimeStart + respawnTime && respawn)
        {
            StartCoroutine(Spawnplayer());
            respawn = false;
        }
    }

    private IEnumerator Spawnplayer()
    {
        Rplayer = Instantiate(player, respawnPoint);
        yield return new WaitForSeconds(0.01f);

        if(Fader.Instance != null)
        {
            CountdownTimer.Instance.enabled = true;
            ScoringMechanism.Instance.NumberOfLives();
            Fader.Instance.BGFader(false);
        }

        var newMask = mCamera.cullingMask | (1 << 2);
        yield return new WaitForEndOfFrame();
        mCamera.cullingMask = newMask;
        Rplayer.transform.parent = null;
        Rplayer.name = player.name;
        anim.SetTrigger("opening");
    }
}
