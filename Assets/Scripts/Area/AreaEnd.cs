using UnityEngine;

public class AreaEnd : MonoBehaviour
{
    [SerializeField] private GameObject spawned;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobIntensity;
    [SerializeField] private float idleRotateSpeed;
    private Area parent;
    private Vector3 startPos;

    public void SetParent(Area parent) => this.parent = parent;

    private void Awake()
    {
        startPos = transform.position;
    }

    private void FixedUpdate()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobIntensity, 0);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + (Time.deltaTime * idleRotateSpeed), 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject spawn = Instantiate(spawned, transform);
        spawn.transform.localPosition = Vector3.zero;
        
        parent.EndTrigger();
    }
}
