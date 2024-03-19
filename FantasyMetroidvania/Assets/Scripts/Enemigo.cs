using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [SerializeField]
    float health; //Vida del enemigo


    //Estas lineas manejan el recoil del enmigo es decir la animacion de irse hacia atras al ser golpeadas
    [SerializeField] 
    float recoilLength; //Cuanto se desplaza hacia atras al ser golpeado

    [SerializeField] 
    float recoilFactor; // 

    [SerializeField] 
    bool isRecoiling = false; //Enemigo en recoil, despues de ser golpeado
    void Start()
    {
        
    }
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void EnemyHit(float _damageDone)
    {
        health -= _damageDone; //Muere el enemigo
    }
}
