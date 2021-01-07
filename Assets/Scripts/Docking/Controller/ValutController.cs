using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{   
    public class ValutController : DockingController
    {
        enum STATE
        {
            IN_VAULT,
            OUT_VAULT,
            ERROR
        }

        private int[] inVaultStateHashes;
        private int[] outVaultStateHashes;
       
        private STATE vaultPeriod;

        private DockingQuadVaultTarget target;

        public override void OnInit(ControllerInitContext context)
        {
            string[] inVaultString = { "Vault.vault_left_hand_Start", "Vault.vault_double_hand_Start" };
            string[] outVaultString = { "Vault.vault_left_hand_End", "Vault.vault_double_hand_End" };

            inVaultStateHashes = Utils.GetStateMachineStateHash(inVaultString);
            outVaultStateHashes = Utils.GetStateMachineStateHash(outVaultString);

            vaultPeriod = STATE.IN_VAULT;

            base.OnInit(context);
        }

        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");

            target = (DockingQuadVaultTarget)context.dockingtarget;
            base.OnEnter(context);          
        }

        public override void Tick(float deltaTime)
        {
            if (!active) return;

            var newPeriod = GetState();

            // 当切换到Out Valut动画时候，寻找地面
            if(newPeriod == STATE.OUT_VAULT && vaultPeriod == STATE.IN_VAULT)
            {
                var dockingBoneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
                var landPoint = target.GetDesiredLandHintTRWS(dockingBoneTrans.position, dockingBoneTrans.rotation);

                var context = CreateFloorVertexTarget(landPoint.translation, landPoint.rotation);                
                base.OnEnter(context);
                active = true;               
            }

            vaultPeriod = newPeriod;
        }

        // 当前State处于哪一个阶段
        private STATE GetState()
        {
            if (Utils.IsCurrentState(m_animator, inVaultStateHashes)) return STATE.IN_VAULT;
            if (Utils.IsCurrentState(m_animator, outVaultStateHashes)) return STATE.OUT_VAULT;
            Debug.LogError("Vault Error, Controller and State machine async！");
            return STATE.ERROR;
        }

        // 当前Out Vault动画State已经结束了，该控制器也结束了
        public override void OnFSMStateExit() 
        {
            if(vaultPeriod == STATE.OUT_VAULT)
            {
                m_nextControllerEnterContext = null;
                m_nextControllerType = typeof(IdleController);
            }            
        }
    }

}
