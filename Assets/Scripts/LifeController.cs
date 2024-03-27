using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    PlayerController player;

    private GameObject[] LifeContainers;
    private Image[] LifeFills;
    public Transform LifeParent;
    public GameObject LifeContainerPrefab;
    void Start()
    {
        player = PlayerController.Instace;
        LifeContainers = new GameObject[PlayerController.Instace.MaxHealth];
        LifeFills = new Image[PlayerController.Instace.MaxHealth];


        PlayerController.Instace.onHealthChangedCallback += UpdateLifeOnUI;
        InstantiateLifeContainers();
        UpdateLifeOnUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Estos set son para que se muestren la cantidad de "pociones" basicamente la vida de la protagonista
    void SetLifeContainers()
    {
        for(int i = 0; i < LifeContainers.Length; i++)
        {
            if(i < PlayerController.Instace.MaxHealth)
            {
                LifeContainers[i].SetActive(true);
            }
            else
            {
                LifeContainers[i].SetActive(false);
            }
        }
    }


    void SetFilledLife()
    {
        for (int i = 0; i < LifeFills.Length; i++)
        {
            if (i < PlayerController.Instace.health)
            {
                LifeFills[i].fillAmount = 1;
            }
            else
            {
                LifeFills[i].fillAmount = 0;
            }
        }
    }


    void InstantiateLifeContainers()
    {
        for (int i = 0; i < PlayerController.Instace.MaxHealth; i++) // detalla cantidad de prefabs
        {

            GameObject temp = Instantiate(LifeContainerPrefab);
            temp.transform.SetParent(LifeParent, false);
            LifeContainers[i] = temp;
            LifeFills[i] = temp.transform.Find("LifeFill").GetComponent<Image>(); //Pociones van en valo a la vida
        }
    }

    void UpdateLifeOnUI()
    {
        SetLifeContainers();
        SetFilledLife();
    }


    }
