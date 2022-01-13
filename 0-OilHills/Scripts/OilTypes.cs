using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilTypes : MonoBehaviour
{
    
    [Range(0,1)][SerializeField]float LowQualityValue = 0.2f;
    [Range(0, 1)] [SerializeField] float MidQualityValue = 0.5f;
    [Range(0, 1)] [SerializeField] float HighQualityValue = 0.8f;

    public bool LowQuality = false;
    public bool MidQuality = false;
    public bool HighQuality = true;

    private float currentQuality;
    void Start()
    {
        SetQuality();
    }

    private void SetQuality()
    {
        if(LowQuality && !MidQuality && !HighQuality)
        {
            currentQuality = LowQualityValue;
        }
        else if(!LowQuality && MidQuality && !HighQuality)
        {
            currentQuality = MidQualityValue;
        }
        else if(!LowQuality && !MidQuality && HighQuality)
        {
            currentQuality = HighQualityValue;
        }
        else
        {
            currentQuality = 0;
        }
    }
    public float getCurrentQuality()
    {
        return currentQuality;
    }
}
