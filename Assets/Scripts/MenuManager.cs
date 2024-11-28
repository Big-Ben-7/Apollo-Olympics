using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using TMPro;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameSettings;

    [Header("Graphics")]
    [SerializeField] TMP_Dropdown numberDropdown;
    [SerializeField] TMP_Dropdown difficultyDropdown;
    [SerializeField] TMP_Dropdown ship1Dropdown;
    [SerializeField] TMP_Dropdown ship2Dropdown;
    [SerializeField] TMP_Dropdown ship3Dropdown;
    [SerializeField] TMP_Dropdown ship4Dropdown;
    [SerializeField] GameObject[] shipDropdownObjects;
    RectTransform[] shipDropdownTransforms;
    [SerializeField] RectTransform numberTransform;
    [SerializeField] RectTransform difficultyTransform;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip majorClick;
    [SerializeField] AudioClip minorClick;
    [SerializeField] AudioClip backClip;
    
    public static int playerNumber;
    public static int difficulty;
    public static int[] playerModels;

    void Awake()
    {
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        shipDropdownTransforms = new RectTransform[4];
        for(int i = 0; i < 4; i++)
        {
            shipDropdownTransforms[i] = shipDropdownObjects[i].GetComponent<RectTransform>();
        }

        numberTransform.anchoredPosition = new Vector2(numberTransform.anchoredPosition.x, 35);
        difficultyTransform.anchoredPosition = new Vector2(difficultyTransform.anchoredPosition.x, 35);
        shipDropdownObjects[0].SetActive(true);
        shipDropdownTransforms[0].anchoredPosition = new Vector2(0, -35);
        shipDropdownObjects[1].SetActive(false);
        shipDropdownObjects[2].SetActive(false);
        shipDropdownObjects[3].SetActive(false);
    }

    public void ShowMain()
    {
        mainMenu.SetActive(true);
        gameSettings.SetActive(false);
        audioSource.PlayOneShot(backClip);
    }

    public void MenuPlay()
    {
        gameSettings.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void Play()
    {
        playerNumber = numberDropdown.value + 1;
        difficulty = difficultyDropdown.value + 1;
        playerModels = new int[] {ship1Dropdown.value, ship2Dropdown.value, ship3Dropdown.value, ship4Dropdown.value};
        SceneManager.LoadScene(1);
    }

    public void ChangedPlayerNumber()
    {
        switch(numberDropdown.value)
        {
            case 0:
                numberTransform.anchoredPosition = new Vector2(numberTransform.anchoredPosition.x, 35);
                difficultyTransform.anchoredPosition = new Vector2(difficultyTransform.anchoredPosition.x, 35);
                shipDropdownObjects[0].SetActive(true);
                shipDropdownTransforms[0].anchoredPosition = new Vector2(0, -35);
                shipDropdownObjects[1].SetActive(false);
                shipDropdownObjects[2].SetActive(false);
                shipDropdownObjects[3].SetActive(false);
                break;

            case 1:
                numberTransform.anchoredPosition = new Vector2(numberTransform.anchoredPosition.x, 35);
                difficultyTransform.anchoredPosition = new Vector2(difficultyTransform.anchoredPosition.x, 35);
                shipDropdownObjects[0].SetActive(true);
                shipDropdownTransforms[0].anchoredPosition = new Vector2(-130, -35);
                shipDropdownObjects[1].SetActive(true);
                shipDropdownTransforms[1].anchoredPosition = new Vector2(100, -35);
                shipDropdownObjects[2].SetActive(false);
                shipDropdownObjects[3].SetActive(false);
                break;

            case 2:
                numberTransform.anchoredPosition = new Vector2(numberTransform.anchoredPosition.x, 45);
                difficultyTransform.anchoredPosition = new Vector2(difficultyTransform.anchoredPosition.x, 45);
                shipDropdownObjects[0].SetActive(true);
                shipDropdownTransforms[0].anchoredPosition = new Vector2(-130, -10);
                shipDropdownObjects[1].SetActive(true);
                shipDropdownTransforms[1].anchoredPosition = new Vector2(100, -10);
                shipDropdownObjects[2].SetActive(true);
                shipDropdownTransforms[2].anchoredPosition = new Vector2(-130, -50);
                shipDropdownObjects[3].SetActive(false);
                break;
            
            case 3:
                numberTransform.anchoredPosition = new Vector2(numberTransform.anchoredPosition.x, 45);
                difficultyTransform.anchoredPosition = new Vector2(difficultyTransform.anchoredPosition.x, 45);
                for(int i = 0; i < 4; i++)
                {
                    shipDropdownObjects[i].SetActive(true);
                }
                shipDropdownTransforms[0].anchoredPosition = new Vector2(-130, -10);
                shipDropdownTransforms[1].anchoredPosition = new Vector2(100, -10);
                shipDropdownTransforms[2].anchoredPosition = new Vector2(-130, -50);
                shipDropdownTransforms[3].anchoredPosition = new Vector2(100, -50);
                break;
        }
    }

    public void MinorClick()
    {
        audioSource.PlayOneShot(minorClick);
    }

    public void MajorClick()
    {
        audioSource.PlayOneShot(majorClick);
    }
}
