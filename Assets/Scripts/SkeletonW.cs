using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonW : Enemigo
{
    // Start is called before the first frame update
    void Start()
    {
        rb.gravityScale = 12f;
    }

    protected override void Awake()
    {
        base.Awake(); //Se llama al awake de la clase base, es decir de enemigo
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();//Se llama al Update de la clase base, es decir de enemigo
        if (!isRecoiling)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(PlayerController.Instace.transform.position.x, transform.position.y), speed * Time.deltaTime);
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce); //Se llama al EnemyHit de la clase base, es decir de enemigo
    }
}
