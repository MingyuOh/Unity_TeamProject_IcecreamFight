using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Static : MonoSingleton<Static> {

    public bool TeamColor;                  //true 면 A팀, false 면 B팀


    void Awake()
    {
        TeamColor = false;    
    }
}
