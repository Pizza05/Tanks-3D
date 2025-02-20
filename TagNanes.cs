using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TagNanes : MonoBehaviour
{
    public TMP_InputField nameInput;
    public static TagNanes instance;

    public void SaveUsername()
    {
        PlayerPrefs.SetString("PlayerName", nameInput.text);
        // set PlayerName value to be equal to the value
        // that store in the inputField
    }

    private void OnEnable()
    {
        var nickname = PlayerPrefs.GetString("PlayerName"); // assign nickname with the value
                                                            // in PlayerName variable

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Player" + Random.Range(10000, 100000); // in case that a user doesn't enter their name
                                                               // this will random a name for the user
        }
        nameInput.text = nickname; // assign value to inputField on the screen

        PlayerPrefs.SetString("PlayerName", nameInput.text); // set PlayerName value to be equal to the value
                                                             // that store in the inputField
    }
}
