using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
public class OilFiller : MonoBehaviour
{
    #region Singleton
    private static OilFiller _instance;

    public static OilFiller Instance{

        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<OilFiller>();
            }
            return _instance;
        }
     }

    #endregion

    #region OilBar

    [TabGroup("OilBar")]
    [SerializeField] private Transform _start;
    [TabGroup("OilBar")]
    [SerializeField] private Transform _end;
    [TabGroup("OilBar")]
    [SerializeField] private Transform _pin;
    [TabGroup("OilBar")]
    [SerializeField] private Image _filler;

    #endregion

    [SerializeField]private float oilAmount = 100f;
    [SerializeField] private float currentOilAmount = 0;
    [SerializeField] private float currentQuality = 0;
    private float Value = 0;
    [Range(0f, 50f)] [SerializeField] public float decreaseValue = 3f;
    [Range(0, 1.0f)] [SerializeField] private float flickingSpeed = 0.1f;
    [Range(0, 1f)] [SerializeField] private float initialQuality = 0.5f;
    [Range(0, 10f)] public float waitBeforeFail = 4f;
    [Range(0, 100f)] public float initialFuelStart = 50;
    [Range(0, 3000f)] public float boostPower = 1250f;
    [Range(0, 10f)] public float boosTime = 3f;
    [Range(0, 500f)] public float emptyDepotBrake = 150f;
    [Range(0, 10)] public int oilToCoinMuiltiplier = 2;
    public bool isBoosted = false;
    public bool isEmpty = false;
    public bool collected = false;
    public bool gameStart = false;
    public bool isGreen = false;

    private Coroutine routine;

    void Start()
    {
        currentOilAmount = initialFuelStart;
        currentQuality = initialQuality;
        Value = currentOilAmount / oilAmount;
        _pin.position = Vector3.Lerp(_start.position, _end.position, Value);
        _filler.fillAmount = Value;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.CanPlay)
        {
            FillValue();
        }
        else
        {
            currentOilAmount = initialFuelStart;
            _filler.fillAmount = Value;
        }

        if(GameManager.CanPlay && !gameStart)
        {
            //Debug.Log("x");
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(LowOilAlert());
            gameStart = true;
        }
        CheckLoop();
    }
    private void FillValue()
    {
        DecreaseOil();
        Value = currentOilAmount / oilAmount;
        _pin.position = Vector3.Lerp(_start.position, _end.position, Value);
        _filler.fillAmount = Value;
    }
    private void DecreaseOil()
    {
        if(currentOilAmount >= 0)
        {
            currentOilAmount = currentOilAmount - decreaseValue * Time.deltaTime;
        }
        else
        {
            return;
        }
    }
    public float getCurrentValue()
    {
        return Value;
    }
    public float getCurrentAmount()
    {
        return currentOilAmount;
    }
    public float getCurrentQuality()
    {
        return currentQuality;
    }
    public void setCurrentAmount(float var)
    {
        currentOilAmount = var;
        

        if(currentOilAmount > oilAmount)
        {
            var coinValue = (int)(currentOilAmount - oilAmount);
            UIManager.Instance.AddCoin(coinValue * oilToCoinMuiltiplier);
            currentOilAmount = oilAmount;
        }
    }
    public void CheckLoop()
    {
        if(currentOilAmount <= 0)
        {
            var time = 0;
            isEmpty = true;
            StartCoroutine(FailSequence(time));
        }
    }
    public void CalculateAverageOil(float averageValueAndAddedOil, float addedOil)
    {
        var currentTotal = currentOilAmount * currentQuality;
        var addedTotal = currentTotal + averageValueAndAddedOil;
        setCurrentAmount(currentOilAmount + addedOil);
        currentQuality = addedTotal / currentOilAmount;
    }

    public Image getBarImage()
    {
        return _filler;
    }
    IEnumerator FailSequence(float time)
    {
        while(time <= waitBeforeFail)
        {
            time = time + Time.deltaTime;
            if(currentOilAmount > 0)
            {
                isBoosted = true;
                isEmpty = false;
                time = 0;
                yield break;
            }
            yield return null;
        }
        if(time >= waitBeforeFail && currentOilAmount <= 0)
        {
            GameManager.I.Fail();
        }
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator LowOilAlert()
    {
        if (!GameManager.CanPlay)
        {
            var newCol = _filler.color;
            newCol = Color.gray;
            _filler.color = newCol;
        }
        //Debug.Log("entered");
        while (GameManager.CanPlay)
        {
            if (_filler.fillAmount > 0.25f && !collected && !isGreen)
            {
               var newCol = _filler.color;
                newCol = Color.gray;
                _filler.color = newCol;
                //Debug.Log("greater than 25%:" + _filler.color);
            }
            else if (_filler.fillAmount <= 0.25f && !isGreen)
            {
                //Debug.Log("less than 25%:" + _filler.color);
                var newCol2 = _filler.color;
               newCol2= Color.red;
                _filler.color = newCol2;
                yield return new WaitForSeconds(flickingSpeed);
                //Debug.Log("less than 25%:" + _filler.color);
                newCol2 = Color.gray;
                _filler.color = newCol2;
            }
            yield return null;
        }
    }
}
