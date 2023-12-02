using System;
using UnityEngine;

namespace ARPG_AE_JOKER.SkillEditor
{
    public class Skill_Player : MonoBehaviour
    {
        private Animation_Controller animation_Controller;

        private bool isPlaying;
        public bool IsPlaying { get => isPlaying; }

        private SkillConfig skillConfig;//配置文件
        private int currentFrameIndex = -1;//当前播放帧
        private float playTotalTime;//播放时长
        private int frameRate;//帧率

        private Action<Vector3, Quaternion> rootMotionAction;
        private Action SkillEndAction;

        private Transform modelTransfrom;

        private SkillSingLineTrackDataBase<SkillAnimationEvent> AnimationData;
        private SkillMultiLineTrackDataBase<SkillAudioEvent> AudioData;
        private SkillMultiLineTrackDataBase<SkillEffectEvent> EffectData;
        private SkillMultiLineTrackDataBase<AttackDetectionEvent> attackDetectionData;

        public void Init(Animation_Controller animation_Controller, Transform modelTransfrom)
        {
            this.animation_Controller = animation_Controller;
            this.modelTransfrom = modelTransfrom;
        }

        /// <summary>
        /// 播放技能
        /// </summary>
        public void PlaySkill(SkillConfig skillConfig, Action skillEndAction, Action<Vector3, Quaternion> rootMotionEvent = null)
        {
            this.skillConfig = skillConfig;
            this.frameRate = skillConfig.FrameRate;
            this.currentFrameIndex = -1;
            this.playTotalTime = 0;
            this.isPlaying = true;
            this.rootMotionAction = rootMotionEvent;
            this.SkillEndAction = skillEndAction;

            this.AnimationData = null;
            this.AudioData = null;
            this.EffectData = null;
            this.attackDetectionData = null;

            if (skillConfig.trackDataDic.ContainsKey("ARPG_AE_JOKER.SkillEditor.AnimationTrack"))
            {
                this.AnimationData = skillConfig.trackDataDic["ARPG_AE_JOKER.SkillEditor.AnimationTrack"] as SkillSingLineTrackDataBase<SkillAnimationEvent>;
            }
            if (skillConfig.trackDataDic.ContainsKey("ARPG_AE_JOKER.SkillEditor.AudioTrack"))
            {
                this.AudioData = skillConfig.trackDataDic["ARPG_AE_JOKER.SkillEditor.AudioTrack"] as SkillMultiLineTrackDataBase<SkillAudioEvent>;
            }
            if (skillConfig.trackDataDic.ContainsKey("ARPG_AE_JOKER.SkillEditor.EffectTrack"))
            {
                this.EffectData = skillConfig.trackDataDic["ARPG_AE_JOKER.SkillEditor.EffectTrack"] as SkillMultiLineTrackDataBase<SkillEffectEvent>;
            }
            if (skillConfig.trackDataDic.ContainsKey("ARPG_AE_JOKER.SkillEditor.AttackDetectionTrack"))
            {
                this.attackDetectionData = skillConfig.trackDataDic["ARPG_AE_JOKER.SkillEditor.AttackDetectionTrack"] as SkillMultiLineTrackDataBase<AttackDetectionEvent>;
            }
            TickSkill();
        }

        /// <summary>
        /// 播放
        /// </summary>
        private void Update()
        {
            if (isPlaying)
            {
                playTotalTime += Time.deltaTime;
                //判断在第几帧
                int tragetFrameIndex = (int)(playTotalTime * frameRate);
                //防止
                while (currentFrameIndex < tragetFrameIndex)
                {
                    //驱动技能
                    TickSkill();
                }
                if (tragetFrameIndex >= skillConfig.FrameCount)//播放完毕
                {
                    isPlaying = false;
                    skillConfig = null;
                    if (rootMotionAction != null) animation_Controller.ClearRootMotionAction();
                    rootMotionAction = null;
                    SkillEndAction?.Invoke();
                }
            }
        }

        /// <summary>
        /// 实际驱动
        /// </summary>
        private void TickSkill()
        {
            currentFrameIndex += 1;

            //驱动动画
            AnimationDriver.Drive(AnimationData, currentFrameIndex, rootMotionAction, animation_Controller);

            //驱动音效果
            AudioDriver.Drive(AudioData, currentFrameIndex, transform.position);

            //驱动特效
            EffectDriver.Drive(EffectData, currentFrameIndex, modelTransfrom, frameRate);

            //驱动攻击检测
            AttackDetectionDriver.Driver(attackDetectionData, currentFrameIndex, modelTransfrom, frameRate);
        }
    }
}