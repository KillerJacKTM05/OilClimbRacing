using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{

    public AnimationCurve powerRatioCurve;

    [SerializeField] WheelCollider FrontRight;
    [SerializeField] WheelCollider FrontLeft;
    [SerializeField] WheelCollider BackRight;
    [SerializeField] WheelCollider BackLeft;
    [SerializeField] float Acceleration = 500;
    [SerializeField] float Break = 300;
    [Range(0f,100f)][SerializeField] float Back_FrontPowerRatio = 100;
    [SerializeField] Transform FRWheel;
    [SerializeField] Transform FLWheel;
    [SerializeField] Transform BRWheel;
    [SerializeField] Transform BLWheel;
    private float relativeSpeedMod;

    private float currentAcceleration;
    private float currentBreak;
    private Vector3 position;
    private Quaternion rotation;
    private int counter = 0;
    private float beforeGameTime;

    void Start()
    {
        CameraManager.Instance.SetTarget(transform, transform);
        CameraManager.Instance.SetPlayerStartPos(transform.position);
        relativeSpeedMod = OilFiller.Instance.getCurrentQuality();
        beforeGameTime = 0;
        StartCoroutine(CheckLoop());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.CanPlay && !OilFiller.Instance.isBoosted)
        {
            relativeSpeedMod = OilFiller.Instance.getCurrentQuality();
            //currentAcceleration = relativeSpeedMod * Acceleration;
            currentAcceleration = Acceleration * powerRatioCurve.Evaluate(relativeSpeedMod) ;
            TorqueChanger();
        }

        if (OilFiller.Instance.isBoosted)
        {
            var boostedtime = 0;
            TorqueChanger();
            StartCoroutine(Boosted(boostedtime));
        }
        if (Input.GetKey(KeyCode.Space) || !GameManager.CanPlay)
        {
            currentBreak = Break;
        }
        else if (OilFiller.Instance.isEmpty && !OilFiller.Instance.isBoosted)
        {
            currentBreak = OilFiller.Instance.emptyDepotBrake;
        }
        else
        {
            currentBreak = 0;
        }

        UpdateWheel(FrontLeft, FLWheel);
        UpdateWheel(FrontRight, FRWheel);
        UpdateWheel(BackLeft, BLWheel);
        UpdateWheel(BackRight, BRWheel);

        FrontRight.brakeTorque = currentBreak;
        FrontLeft.brakeTorque = currentBreak;
        BackRight.brakeTorque = currentBreak;
        BackLeft.brakeTorque = currentBreak;
    }
    private void TorqueChanger()
    {
        FrontRight.motorTorque = currentAcceleration * (Back_FrontPowerRatio / 100f);
        FrontLeft.motorTorque = currentAcceleration * (Back_FrontPowerRatio / 100f);
        BackRight.motorTorque = currentAcceleration * ((100f - Back_FrontPowerRatio) / 100f);
        BackLeft.motorTorque = currentAcceleration * ((100f - Back_FrontPowerRatio) / 100f);
    }

    private void UpdateWheel(WheelCollider wheelCol, Transform wheelTrans)
    {
        wheelCol.GetWorldPose(out position, out rotation);

        if (currentBreak == 0)
        {
            wheelTrans.position = position;
            wheelTrans.rotation = rotation;
        }

    }

    public IEnumerator CheckLoop()
    {
        while (!(this.gameObject.GetComponent<Rigidbody>().velocity.x <= 0.1f && ProgressBarManager.Instance.gameEnd))
        {
            yield return null;
        }
        Debug.Log("car stopped");
        yield return new WaitForSeconds(1f);
        GameManager.I.WinGame();
    }
    IEnumerator Boosted(float boosttime)
    {
        if (counter == 0)
        {
            Debug.Log("boost given");
            currentAcceleration = currentAcceleration + OilFiller.Instance.boostPower;
            counter = 1;
        }
        while (boosttime <= OilFiller.Instance.boosTime)
        {       
            boosttime += Time.deltaTime;
            yield return null;
        }
        currentAcceleration = currentAcceleration - OilFiller.Instance.boostPower;
        OilFiller.Instance.isBoosted = false;
        counter = 0;
        yield return new WaitForSeconds(0.1f);
    }
}
