using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public Animator attack;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Weapon" || other.gameObject.tag == "PlayerWeapon")
        {
            attack.SetInteger("EnemyNear", 0);
        }
    }
}