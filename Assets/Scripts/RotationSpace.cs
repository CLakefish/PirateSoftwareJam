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
    private readonly Dictionary<Collider, Coroutine> spinning = new();

    public void ClearAll()
    {
        foreach (var s in spinning)
        {
            if (s.Value != null) StopCoroutine(s.Value);
        }

        parent.transform.localEulerAngles = Vector3.zero;

        spinning.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spinning.ContainsKey(other))
        {
            return;
        }

        if (!other.CompareTag("Player")) return;

        Coroutine routine = StartCoroutine(Rotate(other));
        spinning.Add(other, routine);
    }

    private IEnumerator Rotate(Collider other)
    {
        Quaternion start = parent.transform.localRotation;
        Quaternion end   = Quaternion.Euler(new Vector3(xAngle, 0, zAngle));

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
