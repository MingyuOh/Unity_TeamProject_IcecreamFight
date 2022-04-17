using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class Position : MonoSingleton<Position>
    {

        //public
        public GameObject ATeamStartPosition;
        public GameObject BTeamStartPosition;

        void Start()
        {

        if (Static.Instance.TeamColor)
        {
            transform.position = ATeamStartPosition.transform.position;
            transform.Rotate(Vector3.up, 180);
        }
        else
            transform.position = BTeamStartPosition.transform.position;
    }
    }
