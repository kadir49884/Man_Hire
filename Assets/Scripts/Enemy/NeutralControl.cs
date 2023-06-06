using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class NeutralControl : BasePlayerControl
{
    [SerializeField]
    private GameObject _neutralBody;

    [SerializeField, ReadOnly]
    private bool _instantiateControl = false;

    private float _distanceValue = 2;
    private float distance = 0;
    private float _colliderDistance;
    private int _playerCount = 0;

    private bool _playerCrash = false;
    private bool _fakeCrash = false;
    private bool _isCrashed = false;
    private bool _neutralInPlayer = true;

    private GameObject followObject;
    private GameObject rotaterObject;

    private CapsuleCollider _capsulControl;
    private PlayerCollision playerCollision;
    private Material _bodyMaterial;
    private Quaternion _newRotate;



    public bool IsCrashed { get => _isCrashed; set => _isCrashed = value; }
    public bool PlayerCrash { get => _playerCrash; set => _playerCrash = value; }
    public bool FakeCrash { get => _fakeCrash; set => _fakeCrash = value; }
    public bool InstantiateControl { get => _instantiateControl; set => _instantiateControl = value; }

    private bool isMouseUp;


    protected override void Start()
    {
        base.Start();
        playerCollision = PlayerCollision.Instance;
        _bodyMaterial = _neutralBody.GetComponent<Renderer>().material;
        _capsulControl = GetComponent<CapsuleCollider>();
        speed = 1;

    }

    protected override void FixedUpdate()
    {
        DistanceControl();
        base.FixedUpdate();
    }

    private void DistanceControl()
    {
        if (IsCrashed)
        {

           


            distance = Vector3.Distance(transform.position, followObject.transform.position);

            if (distance > _distanceValue)
            {
                speed = 8;
                _neutralInPlayer = false;
            }
            else if (distance < _distanceValue)
            {
                speed = 4.9f;
                _neutralInPlayer = true;
            }

            if (isMouseUp)
            {
                speed = 0;
            }
            else
            {
                speed = 8;
            }


            if (distance > _colliderDistance)
            {
                _capsulControl.enabled = false;
            }
            else if (!_capsulControl.enabled)
            {
                _capsulControl.enabled = true;
            }
            if (!_neutralInPlayer)
            {
                transform.LookAt(followObject.transform.position);
            }
            else
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, rotaterObject.transform.localRotation, 0.1f);
            }

        }
        _newRotate = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
        transform.localRotation = _newRotate;
    }
    private void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            isMouseUp = true;
            anim.SetInteger("AnimStatus", 2);

        }
        if (Input.GetMouseButtonDown(0))
        {
            isMouseUp = false;
            anim.SetInteger("AnimStatus", 1);
        }

      
    }


    private void OnTriggerEnter(Collider other)
    {
        GameObject _other = other.gameObject;


        if (!IsCrashed && (_other.GetComponent<BaseCollision>()?.MoneyCount > 0 || InstantiateControl))
        {
            _other.GetComponent<PlayerCollision>()?.CrashMePlayer(gameObject);
            _other.GetComponent<FakeCollision>()?.CrashMeFake(gameObject);
            _other.GetComponent<BaseCollision>()?.CrashNeutral(gameObject);

            if (!InstantiateControl)
            {
                _other.GetComponent<BaseCollision>().MoneyCount--;
            }
     
            playerCollision.WriteMoneyCount();
            _capsulControl.isTrigger = false;
        }

        if (IsCrashed && transform.parent.GetChild(0) != null)
        {
            ShowScore(transform.parent.GetChild(0).gameObject);
        }
        if (IsCrashed && transform.parent.GetChild(1) != null)
        {
            ShowScore(transform.parent.GetChild(1).gameObject);
        }

        if (IsCrashed && _other.GetComponent<BaseCollision>()?.MyPlayerCount > _playerCount)
        {
            _other.GetComponent<BaseCollision>()?.CrashNeutral(gameObject);
        }
    }

    private void ShowScore(GameObject newGameObject)
    {
        newGameObject.GetComponent<BaseCollision>()?.WriteScore();

    }

    public void ToIntoCrashObject(GameObject newGameObject, Material newMat)
    {
        transform.parent = newGameObject.transform.parent;
        rotaterObject = newGameObject;

        if (PlayerCrash)
        {
            followObject = newGameObject.transform.GetChild(0).gameObject;
        }
        else if (FakeCrash)
        {
            followObject = newGameObject.transform.parent.GetChild(0).gameObject;
        }

        newGameObject.gameObject.GetComponent<BaseCollision>()?.UpdateBoxSize();
        ShowScore(newGameObject);

        _neutralBody.GetComponent<Renderer>().material = newMat;

        rb.mass = 1;
        rb.drag = 0;
        anim.SetInteger("AnimStatus", 1);
        _isCrashed = true;
    }

    public void UpdatePlayerCountParent(int newPlayerCount, float newDistance)
    {
        _playerCount = newPlayerCount;
        _distanceValue = newDistance;
        _colliderDistance = newDistance * 5;

    }
    public void ChangeRotation()
    {
        if (!PlayerCrash && !FakeCrash)
        {
            Quaternion newRotate = transform.rotation;
            newRotate.y += 180;
            transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f, RotateMode.Fast).SetRelative(true);
        }
    }


    public void InstantiateTrue()
    {
        _instantiateControl = true;
    }



}
