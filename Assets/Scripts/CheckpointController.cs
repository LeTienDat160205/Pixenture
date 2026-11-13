using UnityEngine;

/// <summary>Starts a one-shot checkpoint animation when a full-progress player touches it.</summary>
[RequireComponent(typeof(Animator), typeof(Collider2D))]
public sealed class CheckpointController : MonoBehaviour
{
    private static readonly int FlagOutState = Animator.StringToHash("Base Layer.FlagOut");

    private Animator animator;
    private Collider2D checkpointTrigger;
    private bool activated;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        checkpointTrigger = GetComponent<Collider2D>();
        checkpointTrigger.isTrigger = true;

        ResetCheckpoint();
    }

    private void OnEnable()
    {
        if (animator != null)
            ResetCheckpoint();
    }

    private void ResetCheckpoint()
    {
        activated = false;
        // The controller has a single default state. Hold it at frame zero until activation.
        animator.Play(FlagOutState, 0, 0f);
        animator.Update(0f);
        animator.speed = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryActivate(other);
    private void OnTriggerStay2D(Collider2D other) => TryActivate(other);

    private void TryActivate(Collider2D other)
    {
        Rigidbody2D attachedBody = other.attachedRigidbody;
        bool isPlayer = other.CompareTag("Player") ||
                        (attachedBody != null && attachedBody.CompareTag("Player"));
        if (activated || !isPlayer)
            return;

        GameController controller = FindFirstObjectByType<GameController>();
        if (controller != null && controller.TryBeginCheckpointTransition(this))
            activated = true;
    }

    public float PlayActivationAnimation()
    {
        animator.speed = 1f;
        animator.Play(FlagOutState, 0, 0f);
        animator.Update(0f);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.length / Mathf.Max(Mathf.Abs(animator.speed), 0.01f);
    }
}
