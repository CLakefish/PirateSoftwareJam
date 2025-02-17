using UnityEngine;

public class AreaEnd : MonoBehaviour
{
    [SerializeField] private GameObject spawned;
    [SerializeField] private Transform view;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobIntensity;
    [SerializeField] private float idleRotateSpeed;
    private Area parent;
    private Vector3 startPos;

    public Transform FollowPos { get; set; }

    public void SetParent(Area parent) => this.parent = parent;

    private void Awake()
    {
        startPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        transform.localPosition = startPos + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobIntensity, 0);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + (Time.deltaTime * idleRotateSpeed), 0);

        if (FollowPos == null) return;

        view.transform.forward = (transform.position - FollowPos.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject spawn = Instantiate(spawned, transform);
        spawn.transform.localPosition = Vector3.zero;
        
        parent.BrainTrigger();
    }
}
