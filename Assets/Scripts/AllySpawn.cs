using Facebook.WitAi;
using Facebook.WitAi.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class AllySpawn : MonoBehaviour
{
    private bool _changed = false;
    private bool _stopped = false;
    private bool _targeted = false;
    private float _nextActionTime = 0f;
    private float _wait = .5f;
    private int _rows;
    private int _cols;
    private int _midRow;
    private int _midCol;
    private int _r;
    private int _c;
    private AllyMov _movement;
    private ConfidenceLevel _confidence = ConfidenceLevel.Medium;
    private GameObject _clone;
    private List<GameObject> _allyList = new List<GameObject>();
    private List<GameObject> _removeList = new List<GameObject>();
    private string _command = null;
    private string[] _keywords = new string[] { "attack", "stop", "spread", "close ranks" };
    private Transform _target;

    protected PhraseRecognizer recognizer;

    public float spacing = 3f;
    public int width = 5;
    public Transform center;
    public GameObject enemySpawner;
    public GameObject player;
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        center = transform;

        if (_keywords != null)
        {
            recognizer = new KeywordRecognizer(_keywords, _confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
            Debug.Log( recognizer.IsRunning );
        }

        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }

        SpawnAllies(45);
    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        _command = args.text;
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

        if (!_targeted && Time.time > 1.5)
        {
            Target();
            _targeted = true;
        }

        if (_command != null)
        {
            switch (_command)
            {
                case "attack":
                    Charge();
                    break;
                case "stop":
                    Stop();
                    break;
                case "close ranks":
                    CloseRanks();
                    break;
                case "spread":
                    SpreadOut();
                    break;
            }
            _command = null;
        }

        if (!center)
        {
            foreach (GameObject _clone in _allyList)
            {
                if (_clone)
                    center = _clone.transform;
                    break;
            }
            if (!center && player)
                center = player.transform;
        }
    }

    public void UpdateCommand(string command)
    {
        if (command != null)
        {
            switch (command)
            {
                case "attack":
                Charge();
                break;
                case "stop":
                Stop();
                break;
                case "close ranks":
                CloseRanks();
                break;
                case "spread":
                SpreadOut();
                break;
            }
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
            _allyList.Remove(_removeList[n]);
            _removeList.RemoveAt(n);
        }

        n = _allyList.Count;
        _rows = (int)Math.Ceiling(Math.Sqrt(n / width));
        _cols = _rows * width;
        _midRow = (_rows - 1) / 2;
        if (n > _cols)
            _midCol = _cols / 2;
        else
            _midCol = (int)(n - 1) / 2;

        _changed = false;

        for (_r = _rows - 1; _r >= 0; _r--)
        {
            for (_c = 0; _c < _cols; _c++)
            {
                _clone = _allyList[(_r - _rows + 1) * -width * _rows + _c];
                _movement = _clone.GetComponent<AllyMov>();
                _movement.offset = new Vector3((_c - _midCol) * spacing, 0f, (_r - _midRow) * spacing);
                if (_r == _midRow && _c == _midCol)
                    center = _clone.transform;
                n -= 1;
                if (n <= 0)
                    break;
            }
        }
        if (!center)
            center = player.transform;
    }

    // Spawns the specified number of allies in formation.
    void SpawnAllies(float n)
    {
        _rows = (int)Math.Ceiling(Math.Sqrt(n / width));
        _cols = _rows * width;
        _midRow = (_rows - 1) / 2;
        if (n > _cols)
            _midCol = _cols / 2;
        else
            _midCol = (int)(n - 1) / 2;

        for (_r = _rows - 1; _r >= 0; _r--)
        {
            for (_c = 0; _c < _cols; _c++)
            {
                _clone = Instantiate(prefab, transform);
                _movement = _clone.GetComponent<AllyMov>();
                _movement.offset = new Vector3((_c - _midCol) * spacing, 0f, (_r - _midRow) * spacing);
                _allyList.Add(_clone);
                _clone.transform.localPosition = new Vector3((_c - _midCol) * spacing, 0f, (_r - _midRow) * spacing);
                if (_r == _midRow && _c == _midCol)
                    center = _clone.transform;
                n -= 1;
                if (n <= 0)
                    break;
            }
        }
        if (!center)
            center = player.transform;
    }

    public void Target()
    {
        _stopped = false;
        _target = enemySpawner.GetComponent<EnemySpawn>().center;
        foreach (GameObject obj in _allyList)
        {
            if (obj == null)
                continue;
            _movement = obj.GetComponent<AllyMov>();
            _movement.destination = _target;
            _movement.center = center;
        }
    }

    public void Charge()
    {
        Target();
        _targeted = true;
    }

    public void Stop()
    {
        _stopped = true;
        foreach (GameObject obj in _allyList)
        {
            if (obj == null)
                continue;
            _movement = obj.GetComponent<AllyMov>();
            _movement.destination = center;
        }
    }

    public void SpreadOut()
    {
        spacing *= 2;
        foreach (GameObject obj in _allyList)
        {
            if (obj)
                obj.GetComponent<AllyMov>().offset *= 2;
        }
    }

    public void CloseRanks()
    {
        spacing /= 2;
        foreach (GameObject obj in _allyList)
        {
            if (obj)
                obj.GetComponent<AllyMov>().offset /= 2;
        }
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }
}
