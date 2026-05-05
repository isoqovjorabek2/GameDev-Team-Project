using UnityEngine;

[CreateAssetMenu(fileName = "BoardTheme", menuName = "Chess/BoardTheme")]
public class BoardTheme : ScriptableObject
{
    [Tooltip("Cream / light wood tone. Pure white + pure black squares hide black pieces.")]
    public Color lightSquare = new Color(0.941f, 0.851f, 0.710f, 1f);

    [Tooltip("Brown / dark wood tone — keeps black silhouettes readable.")]
    public Color darkSquare = new Color(0.710f, 0.533f, 0.388f, 1f);
}