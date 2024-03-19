using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    [SerializeField]
    private float followspeed = 0.1f; //Velocidad que sigue la camra al player

    [SerializeField]
    private Vector3 offset; //Acomodar la camara 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, PlayerController.Instace.transform.position + offset, followspeed);
    }
}
