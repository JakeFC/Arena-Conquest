using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    private float _nextActionTime;
    private float _strength;
    private float _wait = 1f;
    private Material _mat;
    private Slider _health;

    public Transform HealthBar;

    // Start is called before the first frame update
    void Start()
    {
        _health = HealthBar.GetComponent<Slider>();
        _mat = GetComponent<Renderer>().material;
        _mat.SetFloat("Cloned", 1);
    }

    void Update()
    {
        if (_strength == 1)
            StartCoroutine(Blink());

        if (Time.time > _nextActionTime)
        {
            _nextActionTime = Time.time + _wait;
            if (_health.value == 0)
                transform.parent.GetComponent<EnemyMov>().RemoveSelf();
        }
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Weapon" && (other.relativeVelocity.magnitude > .000001 || other.relativeVelocity.magnitude < -.000001)
            || other.gameObject.tag == "PlayerWeapon")
        {
            _health.value -= 10;
            _mat.SetFloat("GlowStrength", 1);
            _strength = 1;
            if (_health.value == 0)
                transform.parent.GetComponent<EnemyMov>().RemoveSelf();
        }
    }

    IEnumerator Blink()
    {
        while (_strength > 0.1)
        {
            _strength -= .2f;
            _mat.SetFloat("GlowStrength", _strength);
            yield return new WaitForSeconds(.1f);
        }
        yield return null;
    }
}
