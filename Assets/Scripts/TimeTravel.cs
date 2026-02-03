using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeTravel : MonoBehaviour
{
    [SerializeField] GameObject present, past;
    [SerializeField] bool presentIsVisible = true;


    void Start()
    {
        present.SetActive(true);
        past.SetActive(false);
    }

    public void Interact()
    {
        bool isPresentActive = present.activeSelf;

        present.SetActive(!isPresentActive);
        past.SetActive(isPresentActive);
    }

}