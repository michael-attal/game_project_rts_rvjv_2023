using System.Collections.Generic;
using UnityEngine;

public enum MecaUpgrade
{
    GLASS_CANNON
}

[CreateAssetMenu(menuName = "Rust Vs Goo/Mecas/Upgrade", fileName = "UpgradeDescriptor")]
public class UpgradeDescriptor : ScriptableObject
{
    public Sprite image;
    public string title;
    [TextArea] public string description;
    public MecaUpgrade upgrade;
}