using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust Vs Goo/Mecas/UpgradeGraph", fileName = "UpgradeGraph")]
public class UpgradeGraph : ScriptableObject
{
    public List<UpgradeDescriptor> upgrades;
}