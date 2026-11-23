
using UnityEngine;
using Unity.Netcode;

public class LANWarriorNetwork : NetworkBehaviour
{
    public enum Team
    {
        Blue,
        Red
    }

    public Team team = Team.Blue;
    public float moveSpeed = 2f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector3 targetPosition;

    private NetworkVariable<bool> isMoving =
        new NetworkVariable<bool>(false);

    private NetworkVariable<bool> facingLeft =
        new NetworkVariable<bool>(false);

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
    }

    public bool CanLocalPlayerControl()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening ||
            !IsSpawned)
            return false;

        if (NetworkManager.Singleton.IsHost)
            return team == Team.Blue;

        return NetworkManager.Singleton.IsConnectedClient
            && team == Team.Red;
    }

    public void RequestMove(Vector3 destination)
    {
        if (!CanLocalPlayerControl())
            return;

        MoveServerRpc(destination);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(
        Vector3 destination,
        ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        ulong hostId = NetworkManager.ServerClientId;

        // Host chi dieu khien doi Xanh.
        if (team == Team.Blue && senderId != hostId)
            return;

        // Client chi dieu khien doi Do.
        if (team == Team.Red && senderId == hostId)
            return;

        targetPosition = new Vector3(
            destination.x,
            destination.y,
            transform.position.z
        );

        if (targetPosition.x < transform.position.x)
            facingLeft.Value = true;
        else if (targetPosition.x > transform.position.x)
            facingLeft.Value = false;

        isMoving.Value = true;
    }

    void Update()
    {
        // Chi Host xu ly vi tri thuc te.
        if (IsServer && IsSpawned && isMoving.Value)
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
                isMoving.Value = false;
            }
        }

        // Ca hai may deu cap nhat Animation.
        if (animator != null && IsSpawned)
            animator.SetBool("IsMoving", isMoving.Value);

        if (spriteRenderer != null && IsSpawned)
            spriteRenderer.flipX = facingLeft.Value;
    }
}
