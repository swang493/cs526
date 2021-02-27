﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;

public enum PlayerState
{
    idle,
    Walking,
    Attacking,
    interacting,
    stagger
}

public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    Vector3 movement;
    public Animator animator;
    public static int keys = 0;
    public static int coins = 0;
    public static int boots = 0;

    public Text youwin;
    public bool endgame = false;
    public GameOver GameOver;
    public GameWin GameWin;

    public FloatValue currentHealth;
    public FloatValue enemyHealth;
    public Signal playerHealthSignal;

    [SerializeField]
    NavMeshSurface2d navMeshSurface;

    private void Start()
    {
        currentState = PlayerState.idle;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        InvokeRepeating("BuildMesh", 1.0f, 2.0f);
    }
    // Update is called once per frame
    void Update()
    {
        if(!endgame)
        {
            movement = Vector3.zero;
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            if(currentState != PlayerState.stagger)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    StartCoroutine(AttackCo());
                }
                if (currentState == PlayerState.Walking || currentState == PlayerState.idle)
                {
                    UpdateAnimationAndMove();
                }
            }
            
        }
    }

    void BuildMesh()
    {
        
        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

    }

    private IEnumerator AttackCo()
    {
        animator.SetBool("Attacking", true);
        currentState = PlayerState.Attacking;
        yield return null;
        animator.SetBool("Attacking", false);
        yield return new WaitForSeconds(0.25f);
        currentState = PlayerState.Walking;
    }

    void UpdateAnimationAndMove()
    {
        if (movement != Vector3.zero)
        {
            MoveCharacter();
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetBool("moving", true);
        } else
        {
            animator.SetBool("moving", false);
        }
        animator.SetBool("Attacking", false);
    }

    void MoveCharacter()
    {
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void Knock(Rigidbody2D rb, float knockTime, float damage)
    {
        currentHealth.runtimeValue -= damage;
        playerHealthSignal.Raise();
        if (currentHealth.runtimeValue > 0)
        {
            StartCoroutine(KnockCo(rb, knockTime));
        } else
        {

            currentHealth.runtimeValue = currentHealth.initialValue;
            enemyHealth.runtimeValue = enemyHealth.initialValue;
            //this.gameObject.SetActive(false);
            animator.SetBool("moving", false);
            GameOverAPI();
        }
    }

    private IEnumerator KnockCo(Rigidbody2D rb, float knockTime)
    {
        if (rb != null)
        {
            yield return new WaitForSeconds(knockTime);
            rb.velocity = Vector2.zero;
            currentState = PlayerState.idle;
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Keys")
        {
            keys++;
            FindObjectOfType<AudioManager>().Play("coin");
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Coins")
        {
            coins += 10;
            FindObjectOfType<AudioManager>().Play("coin");
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Boots")
        {
            moveSpeed = 10f;
            FindObjectOfType<AudioManager>().Play("coin");
            Destroy(collision.gameObject);
        }

        //if (collision.gameObject.tag == "Enemies")
        //{
        //    keys = 0;
        //    GameOverAPI();

        //    Knock(rb, 0.2f, 1);

        //}

        if (collision.gameObject.tag == "Door" && keys == 4)
        {
            GameWinAPI();
        }

    }

    

    public void GameOverAPI()
    {
        GameOver.Setup();
        endgame = true;
        FindObjectOfType<AudioManager>().Pause("background");
    }

    public void GameWinAPI()
    {
        GameWin.Setup();
        endgame = true;
        FindObjectOfType<AudioManager>().Pause("bacground");
    }
}
