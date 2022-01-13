using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using UnityEngine.UI;
using DG.Tweening;

public class CollectOils : MonoBehaviour
{
    public LeanFinger fingerTouch;
    [HideInInspector] public bool isAnyFingerTouching;
    public Transform minerObject;
    public Transform player;

    [Range(0, 1f)][SerializeField] private float oilMovement = 0.04f;
    [SerializeField] private float standartOilFillerAmount = 10f;
    [SerializeField] private List<GameObject> collectedOils = new List<GameObject>();
    private GameObject beforeUI;
    private Image filler;
    private float QualityOfCollected = 0;
    void Start()
    {
        beforeUI = UIManager.Instance.getBeforeUI();
        filler = OilFiller.Instance.getBarImage();
    }
    void Update()
    {
        if(beforeUI.activeInHierarchy == false)
        {
            ProcessInput();
        }
    }
    private void ProcessInput()
    {
        var fingers = LeanTouch.Fingers;
        isAnyFingerTouching = fingers.Count > 0;
        if (isAnyFingerTouching)
        {
            fingerTouch = fingers[0];
            if (fingers[0].Down)
            {
                
            }
            else if (fingers[0].Set)
            {
                Ray casted = Camera.main.ScreenPointToRay(fingers[0].ScreenPosition);
                RaycastHit hit;
                if (Physics.Raycast(casted, out hit, 30, 1 << 7))
                {
                    if (hit.collider.CompareTag("Oil"))
                    {
                        //var instantAmount = OilFiller.Instance.getCurrentAmount();
                        /*var oilReference = hit.collider.gameObject.GetComponent<OilTypes>();
                        QualityOfCollected = oilReference.getCurrentQuality();
                        OilFiller.Instance.CalculateAverageOil(standartOilFillerAmount * QualityOfCollected, standartOilFillerAmount);*/
                        //OilFiller.Instance.setCurrentAmount(instantAmount + standartOilFillerAmount);
                        //Destroy(hit.collider.gameObject);
                        collectedOils.Add(hit.collider.gameObject);
                        HoldOils(hit.collider.gameObject);
                    }               
                }

                if (Physics.Raycast(casted, out hit, 30, 1 << 6))
                {
                    if (hit.collider.CompareTag("Soil"))
                    {
                        //Debug.Log(hit.point);
                        minerObject.position = hit.point+Vector3.forward*1;
                    }
                }
                SetPositions();
            }
            else if (fingers[0].Up)
            {
                if(collectedOils.Count > 0)
                {
                    OilFiller.Instance.collected = true;

                    for (int i = 0; i < collectedOils.Count; i++)
                    {
                        var oilReference = collectedOils[i].GetComponent<OilTypes>();
                        QualityOfCollected = oilReference.getCurrentQuality();
                        SendToCar(collectedOils[i], QualityOfCollected);
                    }
                    collectedOils.Clear();
                }          
            }
        }
    }
    private void HoldOils(GameObject oil)
    {
        Destroy(oil.GetComponent<Collider>());
    }
    private void SetPositions()
    {
        if(collectedOils.Count > 0)
        {
            for(int j = 0; j < collectedOils.Count; j++)
            {
                collectedOils[j].transform.position = minerObject.position + Vector3.back;
            }
        }
    }
    private void SendToCar(GameObject oil, float quality)
    {
        float value = 0;
        var startPos = oil.transform.position;
        StartCoroutine(timer(value, oil.transform, startPos, player, quality));
    }
    IEnumerator timer(float val, Transform oil, Vector3 start, Transform end, float quality)
    {
        while (val < 1)
        {
            var projection = new Vector3(end.position.x, end.position.y, oil.position.z);
            val += oilMovement * Time.deltaTime * 100f;
            oil.position = Vector3.Lerp(start, projection, val);
            yield return null;
        }
        OilFiller.Instance.isGreen = true;
        var col = filler.color;
        col = Color.green;
        filler.color = col;
        OilFiller.Instance.CalculateAverageOil(standartOilFillerAmount * quality, standartOilFillerAmount);

        Destroy(oil.gameObject);

        yield return new WaitForSeconds(0.5f);
        OilFiller.Instance.collected = false;
        Invoke(nameof(RoutineCaller), 0.5f);
    }
    private void RoutineCaller()
    {
        OilFiller.Instance.isGreen = false;
    }
}
