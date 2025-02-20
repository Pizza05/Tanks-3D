using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNames : NetworkBehaviour
{
    [Networked, HideInInspector, Capacity(24), OnChangedRender(nameof(OnNicknameChanged))]
    public string nickname { get; set; }

    public TextMeshProUGUI Nickname;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            nickname = PlayerPrefs.GetString("PlayerName"); //get value from PlayerName
                                                            //and assign it to nickname
            print("NICKNAME : " + nickname);
        }
        OnNicknameChanged(); //assign the text on Nickname.text equal to
                             //nickname which is the networked variable 
    }

    private void OnNicknameChanged()
    {
        Nickname.text = nickname;
        print("ON NICKNAME CHANGED IS WORKING FINE");
    }

    private void Awake()
    {
        Nickname.text = string.Empty;
    }
}
