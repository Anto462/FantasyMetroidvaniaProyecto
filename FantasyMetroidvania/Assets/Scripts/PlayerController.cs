using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    private Rigidbody2D rb; //Jugador

    [SerializeField]
    private float Walkspeed = 1; //Vel movimiento

    private float XAxis; //Axis movimiento

    private float YAxis;

    private float gravity;

    PlayerStateList PState;
    [Space(3)]


    [Header("Verificacion de mundo(Ground)")]
    [SerializeField]
    private float JumpForce = 45;

    private int JumpBufferCounter = 0;

    [SerializeField]
    private int JumpBufferFrames;
    private float WitchTimeCT; //Esto es para poder salvar al jugador si cae pero le pongo este nombre por Bayonetta lol
    [SerializeField]
    private float WitchTime; //Basicamente esto nos permite saltar cierto tiempo despues de haber dejado de tocar el piso, en este caso 0.2 segundos para que no sea algo demasiado notable

    private int WitchJumpCounter; //Esto es el counter para los saltos triples
    [SerializeField]
    private int MaxWitchJump; //Aca se puede modficar la cantidad de saltos en este caso 3

    [SerializeField]
    private Transform groundedCheckPoint;

    [SerializeField]
    private float GroundCheckY = 0.2f;

    [SerializeField]
    private float GroundCheckX = 0.5f;

    [SerializeField]
    private LayerMask WhatIsGround;
    [Space(3)]

    Animator anim;

    [Header("Crow Dash")]
    //Dash de bruja o transformacion en cuervo
    private bool CrowDash = true;
    private bool CrowDashed;
    [SerializeField]
    private float CDashSpeed;
    [SerializeField]
    private float CDashTime;
    [SerializeField]
    private float CDashCooldown;
    [Space(3)]

    [Header("Ataques")]
    //Ataque
    private bool AttackProta = false;
    private float TimeBetweenAttack;
    private float TimeSinceAttack;
    [SerializeField] 
    private Transform SideAttackTransform; //Area de ataque en los lados
    [SerializeField] 
    private Vector2 SideAttackArea; //Determina el largo del area de ataque

    [SerializeField] 
    private Transform UpAttackTransform; //Area de ataque arriba
    [SerializeField] 
    private Vector2 UpAttackArea; //Determina el largo del area de ataque

    [SerializeField] 
    private Transform DownAttackTransform; //Area de ataque abajo
    [SerializeField] 
    private Vector2 DownAttackArea; //Determina el largo del area de ataque

    [SerializeField]
    LayerMask attackableLayer;

    [SerializeField]
    float damage;

    [SerializeField]
    private GameObject EfectoGolpe;
    [Space(3)]

    public static PlayerController Instace;
    private void Awake()
    {
        if(Instace != null && Instace != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instace = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        PState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos() //Se dibuja un gizmos para poder observar el area de ataque
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea); //Se dibuja en los 3 ataques
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs(); //actualiza el movimiento
        UpdtJumpVar(); //Actualziacion var de salto
        if (PState.dashing) return;
        Flip(); //Funcion para cambuair direccion del personaje
        Move(); //Se añade la funcion de movimiento
        Jump(); //Saltar
        CrowStartDash(); // dash
        Attack(); //Ataque
        
    }

    void GetInputs()
    {
        XAxis = Input.GetAxisRaw("Horizontal"); //Se liga a las teclas para los mocimientos horizontales
        YAxis = Input.GetAxisRaw("Vertical");

        AttackProta = Input.GetMouseButtonDown(0);
    }

    private void Flip() //Hacer que el personaje se mueva segun hacia donde va, es decir se voltea segun el eje y
    {
        if(XAxis < 0)
        {
            transform.localScale = new Vector2(-3, transform.localScale.y);
        }
        else if(XAxis > 0)
        {
            transform.localScale = new Vector2(3, transform.localScale.y);
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2 (Walkspeed * XAxis, rb.velocity.y); //La velocidad horizon se setea en base a la velocidad de movimiento(1) sin cambar el eje Y

        anim.SetBool("WalkCaminar", rb.velocity.x != 0 && Grounded()); // animacion caminar
    }

    void CrowStartDash()
    {
        if (Input.GetButtonDown("Dash") && CrowDash && !CrowDashed)
        {
            StartCoroutine(Dash());
            CrowDashed = true;
        }
        if (Grounded())
        {
            CrowDashed = false;
        }
    }
    IEnumerator Dash()
    {
        CrowDash = false; // al activarse el dash se desactiva para que no se pueda spamear
        PState.dashing = true; //estado de jugador pasa a dashing
        anim.SetTrigger("CrowDash"); //Se corre la animacion
        rb.gravityScale = 0; //Movimiento sin gravedad, para que asi no caiga
        rb.velocity = new Vector2(transform.localScale.x * CDashSpeed, 0); //Se mueve horizontalmente segun la escala y velocidad del dash
        yield return new WaitForSeconds(CDashTime); //Se espera a que termine el dash
        rb.gravityScale = gravity; //Se vuelve a aplicar la gravedad
        PState.dashing = false; //Se deja de estar dasheando
        yield return new WaitForSeconds(CDashCooldown); //Se aplica el cd del dash
        CrowDash = true; //Se activa nuevamente el dash
    }

    void Attack()
    {
        TimeSinceAttack += Time.deltaTime;
        if (AttackProta && TimeSinceAttack >= TimeBetweenAttack) //Esto es para permitirle volver a atacar
        {
            TimeSinceAttack = 0;
            anim.SetTrigger("AttackingAtaque"); //animacion de ataque

            if (YAxis == 0 || YAxis < 0 && Grounded()) //Si estamos en el piso sin saltar se aplica el side attack area, es deicr atacamos a los lados
            {
                Hit(SideAttackTransform, SideAttackArea);
                Instantiate(EfectoGolpe, SideAttackTransform);//Efecto al lado
            }
            else if (YAxis > 0) //Si estamos en el piso pero presionando w se activa el up attack area, es deicr atacamos arriba
            {
                Hit(UpAttackTransform, UpAttackArea);
                EfectoGolpeAtAngle(EfectoGolpe, 90, UpAttackTransform); //Efecto arriba

            }
            else if (YAxis < 0 && !Grounded()) //Si estamos en saltando se activa el down attack area es decir se ataca hacia abajo
            {
                Hit(DownAttackTransform, DownAttackArea);
                EfectoGolpeAtAngle(EfectoGolpe, -90, DownAttackTransform); //Efecto abajo
            }
        }

    }

    public bool Grounded()
    {
        if(Physics2D.Raycast(groundedCheckPoint.position, Vector2.down, GroundCheckY, WhatIsGround) || 
            Physics2D.Raycast(groundedCheckPoint.position + new Vector3(GroundCheckX, 0,0), Vector2.down, GroundCheckY, WhatIsGround) ||
            Physics2D.Raycast(groundedCheckPoint.position + new Vector3(-GroundCheckX, 0, 0), Vector2.down, GroundCheckY, WhatIsGround))
        // Origen(GroundCheckP.position), Direccion(Vector2.down), mov(GroundCheckY) y a que layer se busca detectar(WhatIsGround)
        {
            return true; //Se retorna si se esta en contacto con el piso o en las esquinas de este
        }
        else
        {
            return false;
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer); //Colider para golpear multiples objetos y diferenciarlos segun el layer

        if (objectsToHit.Length > 0)
        {
            Debug.Log("Hit");
        }
        for(int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemigo>() != null)
            {
                objectsToHit[i].GetComponent<Enemigo>().EnemyHit(damage);
            }
        }
    }

    void EfectoGolpeAtAngle(GameObject _EfectoGolpe, int _effectAngle, Transform _attackTransform)
    {
        _EfectoGolpe = Instantiate(_EfectoGolpe, _attackTransform);
        _EfectoGolpe.transform.eulerAngles = new Vector3(0, 0, _effectAngle); //Cambio en la rotacion segun el angulo
        _EfectoGolpe.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y); //Para mantener el efecto en el rango que definimos antes
    }
    void Jump()
    {

        if(Input.GetButtonDown("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); //Permite variar el salto segun lo que se deja pulsado el boton

            PState.Jumping = false; //No se esta saltando
        }

        if (!PState.Jumping)
        {
            if (JumpBufferCounter > 0 && WitchTimeCT > 0) //Si le doy a saltar y mi buffer es mayor a o y ademas estoy en el piso o en todo caso si aplica el "Witch time"
            {
                rb.velocity = new Vector3(rb.velocity.x, JumpForce); //Saltar

                PState.Jumping = true; //Se esta saltando
            }
            else if (!Grounded() && WitchJumpCounter < MaxWitchJump && Input.GetButtonDown("Jump"))
            {
                
                PState.Jumping = true; //Se esta saltando

                WitchJumpCounter++;

                rb.velocity = new Vector3(rb.velocity.x, JumpForce); //Saltar

            }
        }

        anim.SetBool("JumpSaltar", !Grounded()); //animacion saltar
    }

    void UpdtJumpVar() //Actualziar variables de salto
    {
        if (Grounded())
        {
            PState.Jumping = false;
            WitchTimeCT = WitchTime;
            WitchJumpCounter = 0;
        }
        else
        {
            WitchTimeCT -= Time.deltaTime; //Delta time siendo el espacio entre frames a los cuales ya se les hace referencia anteriormente, basicamente se disminuye en 1 cada segundo por el paso de los frames
        }

        if (Input.GetButtonDown("Jump"))
        {
            JumpBufferCounter = JumpBufferFrames; //Iguala nuestro buffer a la cantidad maxima de frames en cuanto el judador da a espacio o Y(En control)
        }
        else
        {
            JumpBufferCounter--; //La disminuyes en uno cada Frame
        }
    }

}
