using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BuildingOptionsDescriptor", menuName = "Rust Vs Goo/BuildingOptionsDescriptor")]
public class BuildingOptionsDescriptor : ScriptableObject
{
    public List<BuildingOptionData> prefabsIdList;
}

[Serializable]
public struct BuildingOptionData
{
    public string id;
    public string title;
    public Sprite image;
}
