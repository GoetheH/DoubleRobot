using UnityEngine;

public class PlayerController : RobotController
{
    private void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // 防御中也能检测松开，不return
        HandleDefense();
        HandleMovement();
        HandleCombatInput();
    }

    private void HandleDefense()
    {
        // 按住右键防御，松开解除
        if (Input.GetMouseButtonDown(1))
        {
            Defend();
        }
        if (Input.GetMouseButtonUp(1))
        {
            StopDefend();
        }
    }

    private void HandleMovement()
    {
        // 防御中不能移动
        if (isDefending) return;

        // 只响应A/D键，不响应方向键（避免双人模式冲突）
        float moveX = 0;
        if (Input.GetKey(KeyCode.A)) moveX = -1;
        if (Input.GetKey(KeyCode.D)) moveX = 1;

        // W键/空格 = 跳跃（不响应上箭头，留给玩家2）
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        Vector2 movement = new Vector2(moveX, 0);

        if (Mathf.Abs(moveX) > 0.01f)
        {
            Move(movement);
            UpdateFacingDirection(movement);
            PlayAnimation("Walk");
        }
        else
        {
            if (IsGrounded)
                PlayAnimation("Idle");
        }
    }

    private void HandleCombatInput()
    {
        if (isDefending) return; // 防御中不能攻击

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    private void UpdateFacingDirection(Vector2 movement)
    {
        if (movement.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (movement.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
