using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float hitFlashDuration = 0f;

    private static readonly int Magnitude = Animator.StringToHash("magnitude");
    private static readonly int YVelocity = Animator.StringToHash("yVelocity");
    private static readonly int WallSliding = Animator.StringToHash("isWallSliding");
    private static readonly int Hit = Animator.StringToHash("Hit");

    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (animator == null || body == null) return;

        animator.SetFloat(Magnitude, Mathf.Abs(body.linearVelocity.x));
        animator.SetFloat(YVelocity, body.linearVelocity.y);
        animator.SetBool(WallSliding, false);
    }

    public void SetFacingDirection(float horizontalDirection)
    {
        if (spriteRenderer == null || Mathf.Abs(horizontalDirection) < 0.01f) return;
        spriteRenderer.flipX = horizontalDirection < 0f;
    }

    public void PlayHit()
    {
        if (animator != null && HasHitTrigger())
        {
            animator.ResetTrigger(Hit);
            animator.SetTrigger(Hit);
        }

        if (spriteRenderer == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashWhite());
    }

    private bool HasHitTrigger()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == Hit && parameter.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        flashRoutine = null;
    }
}
