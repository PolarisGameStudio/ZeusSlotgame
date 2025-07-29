using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine.Unity.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace Activity
{
    public class H5RewardActivityIcon:BaseIcon
    {
        private SkeletonGraphic skeletonGraphic;
        // private SkeletonGraphic qipao;
        // private Transform saqian;
        private Button clickButton;
        //当前展示的H5按钮序号
        private int curShowIndex = 0;
        private int totalCount = 5;
        private int showInterval = 0;
        private bool needShowQiPao = true;
        private BaseActivity activity;
        private bool isClicked = false;
        private bool isNormal = false;
        private void Awake()
        {
            skeletonGraphic = Utils.Utilities.RealFindObj<SkeletonGraphic>(transform, "spinAni");
            // qipao = Utils.Utilities.RealFindObj<SkeletonGraphic>(transform, "qipao");
            // saqian = Utils.Utilities.RealFindObj<Transform>(transform, "saqian");
            clickButton = GetComponent<Button>();
            if (clickButton!=null)
            {
                clickButton.onClick.AddListener(OnButtonClick);
            }
        }

        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            activity = ActivityManager.Instance.GetActivityByID(Id);
            if (activity == null)
            {
                Debug.LogError($"Activity with ID {Id} not found.");
                Destroy(this.gameObject);
                return;
            }

            if (activity is H5RewardActivity h5RewardActivity)
            {
                showInterval = h5RewardActivity.TimeInterval;
                isNormal = showInterval < 0;
                if (isNormal) curShowIndex = 1;
                UpdateIcon(h5RewardActivity.CheckCanShow());
            }
        }
        
        public void UpdateIcon(bool isShow = true)
        {
            if (isShow)
            {
                ChangeSkinByIndex(curShowIndex);
                skeletonGraphic.gameObject.SetActive(true);
                // saqian.gameObject.SetActive(true);
                // NeedShowQiPao();
            }
            else
            {
                skeletonGraphic.gameObject.SetActive(false);
                // saqian.gameObject.SetActive(false);
                // DestroyShowQiPao();
            }
        }
        
        public void ChangeSkinByIndex(int index)
        {
            if (skeletonGraphic != null)
            {
                // 假设有一个方法可以根据索引获取皮肤名称
                string skinName = GetSkinNameByIndex(index);
                skeletonGraphic.Skeleton.SetSkin(skinName);
                skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }
        
        private string GetSkinNameByIndex(int index)
        {
            // 根据索引返回对应的皮肤名称
            // 这里需要根据实际情况实现
            return (index+1).ToString();
        }
        
        // private void NeedShowQiPao()
        // {
        //     // needShowQiPao = true;
        //     // DestroyShowQiPao();
        //     qipao.Skeleton.SetSlotsToSetupPose();
        //     qipao.Initialize(true);
        //     qipao.AnimationState.SetAnimation(0,"animation",true);
        //     qipao.gameObject.SetActive(true);
        // }
        //
        // private void DestroyShowQiPao()
        // {
        //     qipao.gameObject.SetActive(false);
        // }
        
        private Coroutine timeCor = null;
        private long lastTime = 0;
        public void OnExitH5()
        {
            //是否被点击过
            if (!isClicked)
            {
                return;
            }
            isClicked = false;
            if (isNormal)
            {
                skeletonGraphic.AnimationState.SetAnimation(0, "without_pao", true);
            }
            else
            {
                //隐藏当前显示的按钮
                skeletonGraphic.gameObject.SetActive(false);
                //重置计数
                lastTime = 0;
                //序号自增，展示下一个
                curShowIndex = (curShowIndex==totalCount - 1) ? 0 : curShowIndex + 1;
                // //销毁气泡展示协程
                // needShowQiPao = false;
                UpdateIcon(false);
                // DestroyShowQiPao();
                if (timeCor!=null)
                {
                    StopCoroutine(timeCor);
                    timeCor = null;
                }
                timeCor = StartCoroutine(StartCalculateTime());
            }
        }
        
        IEnumerator StartCalculateTime()
        {
            while (lastTime<=showInterval)
            {
                yield return new WaitForSecondsRealtime(1f);
                lastTime += 1;
            }
            UpdateIcon();
            StopCoroutine(timeCor);
            timeCor = null;
            lastTime = 0;
        }
        
        void OnButtonClick()
        {
            if (isClicked)
            {
                return;
            }
            isClicked = true;
            activity.OnClickIcon();
        }
    }
}