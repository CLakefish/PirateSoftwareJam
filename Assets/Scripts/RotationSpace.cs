using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationSpace : MonoBehaviour
{
    [SerializeField] private Vector3 up;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private AnimationCurve rotationCurve;
    private readonly Dictionary<Collider, Coroutine> spinning = new();

    private void OnTriggerEnter(Collider other)
    {
        if (spinning.ContainsKey(other))
        {
            var r = spinning[other];
            if (r != null) StopCoroutine(r);

            r = StartCoroutine(Rotate(other));
            return;
        }

        Coroutine routine = StartCoroutine(Rotate(other));
        spinning.Add(other, routine);
    }

    private void OnTriggerStay(Collider other)
    {
        if (spinning.ContainsKey(other)) return;

        Coroutine routine = StartCoroutine(Rotate(other));
        spinning.Add(other, routine);
    }

    private void OnTriggerExit(Collider other)
    {
        if (spinning.TryGetValue(other, out Coroutine routine))
        {
            if (routine != null) StopCoroutine(routine);

            spinning.Remove(other);
        }
    }

    private IEnumerator Rotate(Collider other)
    {
        Quaternion start = other.transform.localRotation;
        Quaternion end   = Quaternion.LookRotation(other.transform.forward, up.normalized);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime * rotationSpeed;
            other.transform.localRotation = Quaternion.Slerp(start, end, rotationCurve.Evaluate(elapsed));
            yield return null;
        }

        other.transform.localRotation = end;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, up.normalized * 10);
    }
}
