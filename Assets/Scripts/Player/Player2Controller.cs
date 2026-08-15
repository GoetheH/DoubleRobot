using UnityEngine;

public class Player2Controller : RobotController
{
    private void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        // 双人模式才激活
        if (GameManager.Instance != null && GameManager.Instance.CurrentMode != GameMode.TwoPlayer) return;

        HandleDefense();
        HandleMovement();
        HandleCombatInput();
    }

    private void HandleDefense()
    {
        // K键防御
        if (Input.GetKeyDown(KeyCode.K))
        {
            Defend();
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            StopDefend();
        }
    }

    private void HandleMovement()
    {
        if (isDefending) return;

        float moveX = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1;

        // 上箭头跳跃
        if (Input.GetKeyDown(KeyCode.UpArrow))
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
        if (isDefending) return;

        // L键射击
        if (Input.GetKeyDown(KeyCode.L))
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
