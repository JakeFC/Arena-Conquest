using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    public Image damageScreen;
    public int health;

    private float _alpha;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        if (_alpha == 0.5f)
            StartCoroutine(Blink());
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "EnemyWeapon" && (other.relativeVelocity.magnitude > .00001 || other.relativeVelocity.magnitude < -.00001))
        {
            health -= 10;
            _alpha = 0.5f;
            if (health == 0)
                transform.parent.GetComponent<AllyMov>().RemoveSelf();
        }
    }

    IEnumerator Blink()
    {
        while (_alpha > 0.05f)
        {
            _alpha -= .1f;
            damageScreen.color = new Color(damageScreen.color.r, damageScreen.color.g,
                                           damageScreen.color.b, _alpha);
            yield return new WaitForSeconds(.1f);
        }
        yield return null;
    }
}
