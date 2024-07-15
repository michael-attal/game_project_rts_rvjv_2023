using UnityEngine;

[CreateAssetMenu(menuName = "Rust Vs Goo/Slimes/FusionDescriptor", fileName = "FusionDescriptor")]
public class FusionDescriptor : ScriptableObject
{
    public string DisplayName;
    public Sprite DisplayImage;
    public FusionInfo FusionInfo;
}
