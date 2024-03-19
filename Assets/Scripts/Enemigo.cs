using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [SerializeField]
    protected float health; //Vida del enemigo


    //Estas lineas manejan el recoil del enmigo es decir la animacion de irse hacia atras al ser golpeadas
    [SerializeField]
    protected float recoilLength; //Cuanto se desplaza hacia atras al ser golpeado

    [SerializeField]
    protected float recoilFactor; // 

    [SerializeField]
    protected bool isRecoiling = false; //Enemigo en recoil, despues de ser golpeado

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected virtual void Start()
    {
        
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instace;
    }
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        if (isRecoiling) //Si el enemigo esta sufriendo recoil
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone; //Muere el enemigo
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    protected void OnTriggerStay2D(Collider2D _Other)
    {
        if (_Other.CompareTag("Player") && !PlayerController.Instace.PState.invincible)
        {
            Attack();
            PlayerController.Instace.HitStopsTime(0, 5, 0.5f);
        }
    }
    protected virtual void Attack()
    {
        PlayerController.Instace.TakeDamage(damage);
    }

}
