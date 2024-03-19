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

    [HideInInspector] public PlayerStateList PState;
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

    private bool landingSoundPlayed;

    [Space(3)]
    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;

    [Space(3)]
    [Header("Vida")]
    [SerializeField] public int health;
    [SerializeField] public int MaxHealth;
    [SerializeField] GameObject DamageFeathers;

    //Esto es para que el jugador pueda reaccionar y asi si vinen varios no se quede atascado y muera
    bool restoreTime;
    float restoreTimeSpeed;


    public static PlayerController Instace;
    private void Awake()
    {
        if (Instace != null && Instace != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instace = this;
        }
        Health = MaxHealth;
    }


    // Start is called before the first frame update
    void Start()
    {
        PState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

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
        Recoil(); //accion de recoil
        restoreTimeEscale(); //Para que el tiempo se detenga un poco al recibir un golpe

    }

    private AudioSource audioSource;

    [Space(5)]
    [Header("Audio")]
    [SerializeField] AudioClip landingSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashAndAttackSound;
    [SerializeField] AudioClip SpellCastSound;
    [SerializeField] AudioClip hurtSound;


    private void FixedUpdate()
    {
        if (PState.dashing) return;
        Recoil();
    }

    void GetInputs()
    {
        XAxis = Input.GetAxisRaw("Horizontal"); //Se liga a las teclas para los mocimientos horizontales
        YAxis = Input.GetAxisRaw("Vertical");

        AttackProta = Input.GetButtonDown("Attack");
    }

    private void Flip() //Hacer que el personaje se mueva segun hacia donde va, es decir se voltea segun el eje y
    {
        if (XAxis < 0)
        {
            transform.localScale = new Vector2(-3, transform.localScale.y);
            PState.lookingRight = false;
        }
        else if (XAxis > 0)
        {
            transform.localScale = new Vector2(3, transform.localScale.y);
            PState.lookingRight = true;
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(Walkspeed * XAxis, rb.velocity.y); //La velocidad horizon se setea en base a la velocidad de movimiento(1) sin cambar el eje Y

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
        audioSource.PlayOneShot(dashAndAttackSound);
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
            audioSource.PlayOneShot(dashAndAttackSound);

            if (YAxis == 0 || YAxis < 0 && Grounded()) //Si estamos en el piso sin saltar se aplica el side attack area, es deicr atacamos a los lados
            {
                Hit(SideAttackTransform, SideAttackArea, ref PState.recoilX, recoilXSpeed);
                Instantiate(EfectoGolpe, SideAttackTransform);//Efecto al lado
            }
            else if (YAxis > 0) //Si estamos en el piso pero presionando w se activa el up attack area, es deicr atacamos arriba
            {
                Hit(UpAttackTransform, UpAttackArea, ref PState.recoilY, recoilYSpeed);
                EfectoGolpeAtAngle(EfectoGolpe, 90, UpAttackTransform); //Efecto arriba

            }
            else if (YAxis < 0 && !Grounded()) //Si estamos en saltando se activa el down attack area es decir se ataca hacia abajo
            {
                Hit(DownAttackTransform, DownAttackArea, ref PState.recoilY, recoilYSpeed);
                EfectoGolpeAtAngle(EfectoGolpe, -90, DownAttackTransform); //Efecto abajo
            }
        }

    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundedCheckPoint.position, Vector2.down, GroundCheckY, WhatIsGround) ||
            Physics2D.Raycast(groundedCheckPoint.position + new Vector3(GroundCheckX, 0, 0), Vector2.down, GroundCheckY, WhatIsGround) ||
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

    void stopRecoilX()
    {
        stepsXRecoiled = 0;
        PState.recoilX = false;
    }
    void stopRecoilY()
    {
        stepsYRecoiled = 0;
        PState.recoilY = false;
    }

    void Recoil()
    {
        if (PState.recoilX)
        {
            if (PState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }
        if (PState.recoilY)
        {
            rb.gravityScale = 0;
            if (YAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            WitchJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //pa ver si ya se finalizo el recoil al pegar
        if(PState.recoilX && stepsXRecoiled < recoilXSteps) //PARAR EJE X
        {
            stepsXRecoiled++;
        }
        else
        {
            stopRecoilX();
        }

        if (PState.recoilY && stepsYRecoiled < recoilYSteps) //PARAR EJE Y
        {
            stepsYRecoiled++;
        }
        else
        {
            stopRecoilY();
        }

        if (Grounded())
        {
            stopRecoilY();
        }
    }

    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(stoptakingdamage());
    }

    IEnumerator stoptakingdamage()
    {
        PState.invincible = true;

        GameObject _damageFeathers = Instantiate(DamageFeathers, transform.position, Quaternion.identity); //Para que se vean las plumas al revicir el daño
        Destroy(_damageFeathers, 1.5f);
        anim.SetTrigger("TakeDamage");

        yield return new WaitForSeconds(1f);
        PState.invincible = false;
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value) //Si la vida es diferente al valor retornado por health
            {
                health = Mathf.Clamp(value, 0, MaxHealth); //Se hace el calculo
            }
        }
    }

    void restoreTimeEscale()
    {
        if(restoreTime == true)
        {
            if(Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else{
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    public void HitStopsTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        if(_delay > 0)
        {
            StopCoroutine(starTimeAgain(_delay));
            StartCoroutine(starTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    IEnumerator starTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer); //Colider para golpear multiples objetos y diferenciarlos segun el layer

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
            Debug.Log("Hit");
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemigo>() != null)
            {
                objectsToHit[i].GetComponent<Enemigo>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
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

        if (Input.GetButtonDown("Jump") && rb.velocity.y > 0)
        {
            audioSource.PlayOneShot(jumpSound);

            rb.velocity = new Vector2(rb.velocity.x, 0); //Permite variar el salto segun lo que se deja pulsado el boton

            PState.Jumping = false; //No se esta saltando
        }

        if (!PState.Jumping)
        {
            if (JumpBufferCounter > 0 && WitchTimeCT > 0) //Si le doy a saltar y mi buffer es mayor a o y ademas estoy en el piso o en todo caso si aplica el "Witch time"
            {
                audioSource.PlayOneShot(jumpSound);

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
            if (!landingSoundPlayed)
            {
                audioSource.PlayOneShot(landingSound);
                landingSoundPlayed = true;
            }
            PState.Jumping = false;
            WitchTimeCT = WitchTime;
            WitchJumpCounter = 0;
        }
        else
        {
            WitchTimeCT -= Time.deltaTime; //Delta time siendo el espacio entre frames a los cuales ya se les hace referencia anteriormente, basicamente se disminuye en 1 cada segundo por el paso de los frames
            landingSoundPlayed = false;
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
