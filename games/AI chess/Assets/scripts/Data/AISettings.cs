using UnityEngine;

[CreateAssetMenu(fileName = "AISettings", menuName = "Chess/AISettings")]
public class AISettings : ScriptableObject
{
    public int depth = 4;
    // Other AI settings
}