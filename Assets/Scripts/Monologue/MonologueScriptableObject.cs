using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monologue")]
public class MonologueScriptableObject : ScriptableObject
{
    [SerializeField] private List<string> monologues;

    public string GetMonologue()
    {
        return monologues[Random.Range(0, monologues.Count)];
    }
}
