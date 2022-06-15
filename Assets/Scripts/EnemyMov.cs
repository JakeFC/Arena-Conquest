using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyMov : MonoBehaviour
{
    public float rotSpeed;
    public Text debug;
    public Transform destination;
    private List<Transform> _allies = new List<Transform>();
    public Vector3 offset;

    private Animator _attack;
    private bool _enemyNear = false;
    private float _nextActionTime;
    private float _singleStep;
    private float _wait = 1f;
    private NavMeshAgent _agent;
    private System.Random _rd = new System.Random();
    private Transform _player = null;
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
            CleanList(_allies);
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
        if (other.gameObject.tag == "Ally")
        {
            _attack.SetInteger("EnemyNear", _rd.Next(3));
            _allies.Add(other.transform);
            if (!_target)
                _target = _allies[0];
            _enemyNear = true;
        }

        if (other.gameObject.tag == "Player")
        {
            _attack.SetInteger("EnemyNear", _rd.Next(3));
            _player = other.transform;
            if (!_target)
                _target = _player;
            _enemyNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _player = null;
            FindTarget();
        }
    }

    public void RemoveSelf()
    {
        transform.parent.GetComponent<EnemySpawn>().Remove(this.gameObject);
        CleanList(_allies);
        foreach (Transform t in _allies)
        {
            if (t)
                t.parent.gameObject.GetComponent<AllyMov>().Remove(transform.GetChild(0));
        }
        Destroy(this.gameObject);
    }

    public void Remove(Transform t)
    {
        CleanList(_allies);
        _allies.Remove(t);
        FindTarget();
    }

    void FindTarget()
    {
        if (_allies.Count == 0)
        {
            if (_player)
                _target = _player;
            else
            {
                _enemyNear = false;
                _target = null;
                _attack.SetInteger("EnemyNear", 0);
            }
        }
        else if (_player)
        {
            _target = Vector3.Distance(transform.position, _player.position) <=
                    Vector3.Distance(transform.position, _allies[0].position) ? _player : _allies[0];
        }
        else
            _target = _allies[0];
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
