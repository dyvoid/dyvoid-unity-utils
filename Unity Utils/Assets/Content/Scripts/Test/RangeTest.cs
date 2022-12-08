using System;
using MediaMonks.Utility;
using UnityEngine;
using Range = MediaMonks.Utility.Range;

public class RangeTest : MonoBehaviour
{
    [SerializeField] private Range _range;
    [SerializeField] private RangedObservableFloat _rangedFloatValue = new();

}
