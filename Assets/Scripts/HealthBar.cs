using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider fuelBar;

    // Start is called before the first frame update
    void Start()
    {
        
        fuelBar = GetComponent<Slider>();
    }

    public void setFuel(double f){
        fuelBar.value = (float)f;
    }

    public void setMax(double m){
        fuelBar.maxValue = (float)m;
    }


}
