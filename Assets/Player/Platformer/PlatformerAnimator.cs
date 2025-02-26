using UnityEngine;

public class PlatformerAnimator : PlayerManager.PlayerController
{
    [SerializeField] private Animator hand;
    [SerializeField] private RectTransform rect;
    [SerializeField] private float xIntensity, xBobIntensity = 15, xSpeed = 3;
    [SerializeField] private float yIntensity, yBobIntensity = 15, ySpeed = 1.5f;
    [SerializeField] private float interpolateSpeed;
    private Vector3 start;
    private Vector3 vel = Vector3.zero;
    private float moveTime;

    public void HandAnim(string name) => hand.SetTrigger(name);

    private void Awake()
    {
        start = rect.localPosition;
    }

    private void Update()
    {
        moveTime = PlayerInputs.IsInputting ? moveTime + Time.unscaledDeltaTime : 0;

        float bobX = Mathf.Sin(moveTime * xSpeed) * xBobIntensity;
        float bobY = Mathf.Cos(moveTime * ySpeed) * yBobIntensity;

        float x = (xIntensity * PlayerInputs.Input.x) + start.x + bobX;
        float y = (Mathf.Clamp(PlayerInputs.Input.y, -1.0f, 1.0f) * yIntensity) + start.y + bobY;

        Vector3 pos = new(x, y, 0);

        rect.localPosition = Vector3.SmoothDamp(rect.localPosition, pos, ref vel, interpolateSpeed);
    }
}
