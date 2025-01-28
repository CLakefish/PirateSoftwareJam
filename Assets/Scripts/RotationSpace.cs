using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSpace : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private float zAngle;
    [SerializeField] private float xAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private AnimationCurve rotationCurve;
    private Quaternion startRotation;
    private readonly Dictionary<Collider, Coroutine> spinning = new();

    private void Awake()
    {
        startRotation = parent.transform.localRotation;
    }

    public void ClearAll()
    {
        foreach (var s in spinning)
        {
            if (s.Value != null) StopCoroutine(s.Value);
        }

        parent.transform.localRotation = startRotation;

        spinning.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spinning.ContainsKey(other)) {
            return;
        }

        if (!other.CompareTag("Player")) return;

        Coroutine routine = StartCoroutine(Rotate(other));
        spinning.Add(other, routine);
    }

    private IEnumerator Rotate(Collider other)
    {
        Quaternion start = parent.transform.localRotation;
        Quaternion end   = Quaternion.Euler(new Vector3(xAngle, startRotation.eulerAngles.y, zAngle));

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * rotationSpeed;
            parent.transform.localRotation = Quaternion.Slerp(start, end, rotationCurve.Evaluate(elapsed));
            yield return null;
        }

        parent.transform.localRotation = end;
        spinning.Remove(other);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
    }
}
