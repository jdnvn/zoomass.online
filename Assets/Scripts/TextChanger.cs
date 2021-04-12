using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextChanger : MonoBehaviour
{
    public TextMeshProUGUI loadingText;

    string[] loadingQuotes = new string[]
    {
        "feeding the geese...",
        "on a hammock, by the pond...",
        "hitting the beach...(at Puffers)",
        "imagine you are waiting in line at Late Night...",
        "it's weird, but it works...",
        "Mr. Brightside is playing in another room...",
    };

    void Start()
    {
        int randomIndex = Random.Range(0, loadingQuotes.Length);

        loadingText.text = loadingQuotes[randomIndex];
    }
}
