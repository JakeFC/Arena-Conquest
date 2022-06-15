using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AllyMov : MonoBehaviour
{
    public float rotSpeed;
    public Text debug;
    public Transform destination = null;
    private List<Transform> _enemies = new List<Transform>();
    public Vector3 offset;

    private Animator _attack;
    private bool _enemyNear = false;
    private float _nextActionTime;
    private float _singleStep;
    private float _wait = 1f;
    private NavMeshAgent _agent;
    private System.Random _rd = new System.Random();
    private Transform _target = null;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _attack = transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _nextActionTime)
        {
            _nextActionTime = Time.time + _wait;
            CleanList(_enemies);
            if (_enemyNear)
                _attack.SetInteger("EnemyNear", _rd.Next(3));
        }

        if (destination)
        {
            if (!_enemyNear || !_target)
                _agent.destination = destination.position + offset;
            else
                _agent.destination = _target.position;
            //debug.text = _agent.destination.ToString();
        }

        if (_target != null)
        {
            _singleStep = rotSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward,
                                    _target.position - transform.position, _singleStep, 0f));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            _attack.SetInteger("EnemyNear", _rd.Next(3));
            _enemies.Add(other.transform);
            _target = _enemies[0];
            _enemyNear = true;
        }
    }

    public void RemoveSelf()
    {
        transform.parent.GetComponent<AllySpawn>().Remove(this.gameObject);
        CleanList(_enemies);
        foreach (Transform t in _enemies)
        {
            if (t)
                t.parent.gameObject.GetComponent<EnemyMov>().Remove(transform.GetChild(0));
        }
        Destroy(this.gameObject);
    }

    public void Remove(Transform t)
    {
        CleanList(_enemies);
        _enemies.Remove(t);
        if (_enemies.Count == 0)
        {
            _enemyNear = false;
            _target = null;
            _attack.SetInteger("EnemyNear", 0);
        }
        else
            _target = _enemies[0];
    }

    public void CleanList(List<Transform> list)
    {
        foreach (Transform t in list)
        {
            if (t == null)
            {
                list.Remove(t);
                CleanList(list);
                break;
            }
        }
    }
}
