
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    private bool isMoving = false;

    public bool IsSelected { get; private set; }

    public float moveSpeed = 2f;

    private Vector3 targetPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        originalColor = spriteRenderer.color;
        targetPosition = transform.position;
    }

    void Update()
    {
        // Di chuyen linh den dich
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(
                transform.position,
                targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }

        // Chuyen Idle va Run
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    // Chon hoac bo chon linh
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (IsSelected)
        {
            spriteRenderer.color =
                new Color(0.6f, 1f, 0.6f, 1f);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    // Nhan lenh di chuyen tu SelectionManager
    public void MoveTo(Vector3 destination)
    {
        targetPosition = new Vector3(
            destination.x,
            destination.y,
            transform.position.z
        );

        float directionX =
            targetPosition.x - transform.position.x;

        // Tu quay dau trai / phai
        if (Mathf.Abs(directionX) > 0.01f)
        {
            spriteRenderer.flipX = directionX < 0;
        }

        isMoving = Vector3.Distance(
            transform.position,
            targetPosition
        ) > 0.05f;
    }
}
