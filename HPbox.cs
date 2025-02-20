using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HPbox : NetworkBehaviour
{
    Collider[] hits = new Collider[5]; // อาร์เรย์เก็บผลลัพธ์การชน
    public LayerMask colLayer;         // เลเยอร์ที่กำหนดให้ตรวจจับ Collider
    [Networked] private TickTimer respawnTimer { get; set; } // จับเวลา Respawn

    public override void FixedUpdateNetwork()
    {
        // ตรวจสอบว่า HP Box ยังไม่หมดเวลา respawn
        if (!respawnTimer.IsRunning)
        {
            // ตรวจจับการชนกับวัตถุในรัศมี 1.0 หน่วย
            int hitCount = Runner.GetPhysicsScene().OverlapSphere(transform.position, 1.0f, hits, colLayer, default);
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    TankHealthMP target = hits[i].GetComponentInParent<TankHealthMP>();
                    if (target != null)
                    {
                        // เมื่อมีการชนกับ Tank ให้เพิ่ม HP ให้กับ Tank และหยุดจับเวลา respawn
                        target.HealRpc(40);
                        respawnTimer = TickTimer.CreateFromSeconds(Runner, 5.0f); // กำหนดเวลารอใหม่หลังจากถูกเก็บ
                        Runner.Despawn(Object); // ลบ HP Box ออกจากเกม
                        break;
                    }
                }
            }
        }
        else if (respawnTimer.Expired(Runner)) // เมื่อ respawn timer หมด
        {
            // Spawn HP Box ใหม่ที่ตำแหน่งเดิม
            Runner.Spawn(this, transform.position, transform.rotation, Object.InputAuthority);
        }
    }
}




