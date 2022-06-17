using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class EnemySpawn : MonoBehaviour
{
    private bool _changed = false;
    private bool _stopped = false;
    private bool _targeted = false;
    private bool _won = false;
    private float _nextActionTime = 0f;
    private float _wait = .5f;
    private int _rows;
    private int _cols;
    private int _midRow;
    private int _midCol;
    private int _r;
    private int _c;
    private EnemyMov _movement;
    private GameObject _clone;
    private List<GameObject> _enemyList = new List<GameObject>();
    private List<GameObject> _removeList = new List<GameObject>();
    private Transform _target;

    public Canvas canvas;
    public float spacing = 3f;
    public int width = 5;
    public Transform center;
    public Transform player;
    public GameObject allySpawner;
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        center = transform;
        SpawnEnemies(40);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _nextActionTime)
        {
            _nextActionTime = Time.time + _wait;
            if (_changed)
            {
                Reorganize();
                if (!_stopped)
                    Target();
            }
        }

        if (!_targeted && Time.time > 15)
        {
            if (!_stopped)
                Target();
            _targeted = true;
        }
        if (!_targeted && allySpawner.GetComponent<AllySpawn>().center.position.z > -175)
        {
            if (!_stopped)
                Target();
            _targeted = true;
        }

        if (!center)
        {
            foreach (GameObject _clone in _enemyList)
            {
                if (_clone)
                    center = _clone.transform;
                    break;
            }
            if (!center && !_won)
                ShowWinScreen();
        }
    }

    public void Remove(GameObject obj)
    {
        _removeList.Add(obj);
        _changed = true;
    }

    void Reorganize()
    {
        int n = _removeList.Count - 1;
        for (; n >= 0; n--)
        {
            _enemyList.Remove(_removeList[n]);
            _removeList.RemoveAt(n);
        }

        n = _enemyList.Count;
        _rows = (int)Math.Ceiling(Math.Sqrt(n / width));
        _cols = _rows * width;
        _midRow = (_rows - 1) / 2;
        if (n > _cols)
            _midCol = _cols / 2;
        else
            _midCol = (int)(n - 1) / 2;

        _changed = false;

        for (_r = 0; _r < _rows; _r++)
        {
            for (_c = 0; _c < _cols; _c++)
            {
                _clone = _enemyList[_r * width * _rows + _c];
                _movement = _clone.GetComponent<EnemyMov>();
                _movement.offset = new Vector3((_midCol - _c) * spacing, 0f, (_midRow - _r) * spacing);
                if (_r == _midRow && _c == _midCol)
                    center = _clone.transform;
                n -= 1;
                if (n <= 0)
                    break;
            }
        }
    }

    // Spawns the specified number of enemies in formation.
    void SpawnEnemies(float n)
    {
        _rows = (int)Math.Ceiling(Math.Sqrt(n / width));
        _cols = _rows * width;
        _midRow = (_rows - 1) / 2;
        if (n > _cols)
            _midCol = _cols / 2;
        else
            _midCol = (int)(n - 1) / 2;

        for (_r = 0; _r < _rows; _r++)
        {
            for (_c = 0; _c < _cols; _c++)
            {
                _clone = Instantiate(prefab, transform);
                _movement = _clone.GetComponent<EnemyMov>();
                _movement.offset = new Vector3((_c - _midCol) * spacing, 0f, (_r - _midRow) * spacing);
                _enemyList.Add(_clone);
                _clone.transform.localPosition = new Vector3((_midCol - _c) * spacing, 0f, (_midRow - _r) * spacing);
                if (_r == _midRow && _c == _midCol)
                    center = _clone.transform;
                n -= 1;
                if (n <= 0)
                    break;
            }
        }
    }

    void Target()
    {
        _stopped = false;
        _target = allySpawner.GetComponent<AllySpawn>().center;
        foreach (GameObject obj in _enemyList)
        {
            if (obj == null)
                continue;
            _movement = obj.GetComponent<EnemyMov>();
            _movement.destination = _target;
        }
    }

    void Stop()
    {
        _stopped = true;
        foreach (GameObject obj in _enemyList)
        {
            if (obj == null)
                continue;
            _movement = obj.GetComponent<EnemyMov>();
            _movement.destination = center;
            _movement.center = center;
        }
    }

    public void SpreadOut()
    {
        spacing *= 2;
        foreach (GameObject obj in _enemyList)
        {
            if (obj)
                obj.GetComponent<EnemyMov>().offset *= 2;
        }
    }

    public void CloseRanks()
    {
        spacing /= 2;
        foreach (GameObject obj in _enemyList)
        {
            if (obj)
                obj.GetComponent<EnemyMov>().offset /= 2;
        }
    }

    public void ShowWinScreen()
    {
        _won = true;
        canvas.gameObject.SetActive(true);
    }
}
