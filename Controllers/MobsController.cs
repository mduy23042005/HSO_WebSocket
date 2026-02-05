using System;
using System.Collections.Generic;
using System.Numerics;

public class SyncMobsResultData
{
    public int id;
    public int idMob;
    public float posX;
    public float posY;
    public string state;
    public int idState;
    public int direction;
}
public class SyncMobsResultPacket
{
    public string cmd;
    public List<SyncMobsResultPacket> mobs;
}

public class MobsController
{
    private float moveSpeed = 2f;
    private float waitAfterMove = 0f;
    private float changeTargetCooldown = 0f;
    private bool isAttacking = false;

    private Vector2 position;
    private Vector2 targetPos;
    private float direction;
    private int idState;

    private float visionRadius = 5f;      // vùng nhìn thấy
    private float attackRange = 0.5f;     // tầm đánh
    private float attackCooldown = 0f;
    private float attackInterval = 1.2f; // thời gian giữa 2 đòn
    private int targetPlayerId = -1;      // player đang bị target

    private static readonly Random random = new Random();
    /// <summary>
    /// centerX = offset(Unity)
    /// minX = posX - sizeX / 2
    /// maxX = posX + sizeX / 2
    /// minY = posY - sizeY / 2
    /// maxY = posY + sizeY / 2
    /// </summary>
    private float minX; // minX, minY ----------- maxX, minY
    private float minY; //     |                      |
    private float maxX; //     |                      |
    private float maxY; // minX, maxY ----------- maxX, maxY

    public MobsController(float posX, float posY, float sizeX, float sizeY)
    {
        minX = posX - sizeX / 2;
        maxX = posX + sizeX / 2;
        minY = posY - sizeY / 2;
        maxY = posY + sizeY / 2;

        position = GetRandomPosition();
        targetPos = position;
    }

    public Vector2 GetRandomPosition()
    {
        float x = (float)(random.NextDouble() * (maxX - minX) + minX);
        float y = (float)(random.NextDouble() * (maxY - minY) + minY);

        return new Vector2(x, y);
    }
    public void Move(float deltaTime)
    {
        if (isAttacking)
            return;

        if (waitAfterMove > 0f)
        {
            waitAfterMove -= deltaTime;
            return;
        }

        // tới target hoặc hết cooldown thì chọn target mới
        if (Vector2.Distance(position, targetPos) < 0.1f || changeTargetCooldown <= 0f)
        {
            targetPos = GetRandomPosition();
            changeTargetCooldown = 1f + (float)random.NextDouble() * 2f; // 1–3s
            waitAfterMove = 0.3f + (float)random.NextDouble() * 0.7f; // 0.3–1s
            return;
        }

        changeTargetCooldown -= deltaTime;

        // di chuyển về target
        Vector2 dir = targetPos - position;
        direction = dir.X; // lấy direction gửi cho client

        if (dir.LengthSquared() > 0.0001f)
        {
            dir = Vector2.Normalize(dir);
            position += dir * moveSpeed * deltaTime;
        }

        // đảm bảo không vượt vùng
        position.X = Clamp(position.X, minX, maxX);
        position.Y = Clamp(position.Y, minY, maxY);
    }
    private float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private int FindNearestPlayerInVision()
    {
        float minDistSq = float.MaxValue;
        int nearestPlayerId = -1;

        foreach (var kv in CacheManager.Instance.accounts)
        {
            var playerData = kv.Value.syncData;
            if (playerData == null) continue;

            Vector2 playerPos = new Vector2(playerData.posX, playerData.posY);
            float distSq = Vector2.DistanceSquared(position, playerPos);

            if (distSq <= visionRadius * visionRadius && distSq < minDistSq)
            {
                minDistSq = distSq;
                nearestPlayerId = kv.Key;
            }
        }

        return nearestPlayerId;
    }
    public void Attack(float deltaTime)
    {
        // cooldown đòn đánh
        if (attackCooldown > 0f)
            attackCooldown -= deltaTime;

        // Nếu chưa có target => tìm target
        if (targetPlayerId == -1)
        {
            targetPlayerId = FindNearestPlayerInVision();
            if (targetPlayerId == -1)
            {
                // không attack nữa => reset
                isAttacking = false;
                idState = 0;
                return;
            }
        }

        var account = CacheManager.Instance.GetAccountData(targetPlayerId);
        if (account == null || account.syncData == null)
        {
            targetPlayerId = -1;
            isAttacking = false;
            idState = 0; // reset khi mất target
            return;
        }

        Vector2 playerPos = new Vector2(account.syncData.posX, account.syncData.posY);
        Vector2 dir = playerPos - position;
        float distSq = dir.LengthSquared();

        // Player ra khỏi vùng nhìn
        if (distSq > visionRadius * visionRadius)
        {
            targetPlayerId = -1;
            isAttacking = false;
            idState = 0; // reset khi out vision

            return;
        }

        isAttacking = true;

        // cập nhật hướng gửi client
        direction = dir.X;

        // chưa vào tầm đánh => lao tới
        if (distSq > attackRange * attackRange && dir.LengthSquared() > 0.0001f)
        {
            dir = Vector2.Normalize(dir);
            position += dir * moveSpeed * deltaTime;
            return;
        }

        if (attackCooldown <= 0f)
        {
            idState++;
            attackCooldown = attackInterval;

            // xử lý damage tại đây
        }
    }

    public Vector2 GetPosition()
    {
        return position;
    }
    public string GetState()
    {
        if (isAttacking)
            return "Atk";
        if (waitAfterMove > 0f)
            return "Stand";
        return "Move";
    }
    public int GetIDState()
    {
        return idState;
    }
    public int GetDirection()
    {
        if (direction < 0)
            return -1;
        return 1;
    }
}

