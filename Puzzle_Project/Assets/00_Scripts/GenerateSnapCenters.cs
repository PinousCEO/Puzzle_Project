// Assets/Editor/GenerateSnapCenters.cs

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GenerateSnapCenters : MonoBehaviour
{
   private void Start()
   {
      for (int i = 0; i < transform.childCount; i++)
      {
         transform.GetChild(i).gameObject.name = (i+1).ToString("D2");
      }
   }
}
