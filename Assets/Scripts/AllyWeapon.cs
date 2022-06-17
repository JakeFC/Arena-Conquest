using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyWeapon : MonoBehaviour
{
    public Animator attack;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "EnemyWeapon")
        {
            attack.SetInteger("EnemyNear", 0);
        }
    }
}
