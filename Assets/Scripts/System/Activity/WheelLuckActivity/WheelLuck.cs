
using System.Collections;
using System.Collections.Generic;
using Ads;
using CardSystem;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace Activity
{
    public class WheelLuck : MonoBehaviour
    {
        public Button btnSpin;
        private SpinState _spinState = SpinState.Normal;
        private readonly List<GameObject> _wheelItems = new List<GameObject>();

       
        private WheelLuckActivity _activity;
        protected void Awake()
        {
            _activity = ActivityManager.Instance.GetActivityByID(WheelLuckActivity.ActiveId) as WheelLuckActivity;
            
            Transform content = Util.FindObject<Transform>(transform, "content");
            for (int i = 0; i < 8; i++)
            {
                GameObject go = Util.FindObject<GameObject>(content, "" + i);
                _wheelItems.Add(go);
            }

            if (btnSpin != null)
            {
                btnSpin.onClick.AddListener(OnClickSpinButton);
            }

            spinCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 2), // 开始快速
                new Keyframe(slowDownRatio, 0.9f, 0, 0), // 减速点
                new Keyframe(1, 1, 0, 0) // 缓慢停止
            );
            UpdateSelection(0);
        }

        protected void Start()
        {
            var spinItems = _activity.SpinItems;
            
            for (int i = 0; i < spinItems.Count; i++)
            {
                GameObject go = _wheelItems[i];
                Transform imgUnSelect = Util.FindObject<Transform>(go.transform, "img_cardpack");
                TextMeshProUGUI txt = Util.FindObject<TextMeshProUGUI>(go.transform, "name");
                var icon = imgUnSelect.GetComponent<Image>();

                var shopItemData = _activity.ShopItems[spinItems[i].ShopItemId];
                
                _activity.LoadIcon(shopItemData.Id,icon);
                
                var localizedString = new LocalizedString(LocalizationManager.Instance.tableName, shopItemData.Name);
                
                txt.text = localizedString.GetLocalizedString();
            }
        }
        
        private void SetItemAni(GameObject go, string trigger = "idle")
        {
            // 设置转轮项的动画
            Animator ani = go.GetComponent<Animator>();
            if (ani != null)
            {
                ani.SetTrigger(trigger);
            }
        }
        
        private void OnClickSpinButton()
        {
            if (_spinState != SpinState.Normal)
            {
                Debug.LogWarning("Already spinning, please wait.");
                return;
            }
            OnDoSpin();
        }

        private int _targetPosition;
        void OnDoSpin()
        {
            if(btnSpin!=null)
            {
                btnSpin.interactable = false; // 禁用按钮，防止重复点击
            }
            _spinState = SpinState.Spinning;
            //cardIndex = weightCondition.GetResultByWeight();

            _targetPosition = _activity.GetPos();
            Debug.Log($"Target Position: {_targetPosition}");
            if (_targetPosition < 0 || _targetPosition >= _wheelItems.Count)
            {
                Debug.LogError("无效的目标位置: " + _targetPosition);
                return;
            }
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "CardLottery");
            _spinCoroutine = StartCoroutine(SpinToPosition(_targetPosition));
        }


        #region 转轮

        public float spinDuration = 3f; // 旋转持续时间

        [Range(0.1f, 0.9f)] [Header("可调节减速阶段占总动画时长的比例（默认70%时间完成90%旋转，剩下30%时间完成最后10%")]
        public float slowDownRatio = 0.7f;
        [Header("动画曲线")]
        public AnimationCurve spinCurve;
        private int _currentSelection = 0;
        private Coroutine _spinCoroutine;
        public int turnCycles = 5; // 转动圈数

        // 修改后的位置角度定义（0在左上135度，顺时针排列）
        private readonly float[] _positionAngles = new float[8]
        {
            135f, // 0 - 左上 (起始位置)
            180f, // 1 - 左
            225f, // 2 - 左下
            270f, // 3 - 下
            315f, // 4 - 右下
            0f, // 5 - 右
            45f, // 6 - 右上
            90f // 7 - 上
        };

        private IEnumerator SpinToPosition(int targetPosition)
        {
            float elapsedTime = 0f;
            float startAngle = _positionAngles[_currentSelection];

            // 计算需要旋转的角度（考虑最短路径）
            float targetAngle = _positionAngles[targetPosition];
            float angleDifference = Mathf.DeltaAngle(startAngle, targetAngle);

            // 添加额外旋转圈数（3圈）
            float totalRotation = angleDifference + 360f * turnCycles;

            while (elapsedTime < spinDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / spinDuration;

                // 应用缓动曲线
                float easedProgress = spinCurve.Evaluate(progress);

                // 计算当前角度
                float currentAngle = startAngle + totalRotation * easedProgress;

                // 更新当前选择
                UpdateCurrentSelection(currentAngle);

                yield return null;
            }

            yield return GameConstants.TwoIn10SecondWait;
            ShowWheelItemState(targetPosition, WheelItemState.Select);

            yield return GameConstants.TwoIn10SecondWait;
            //转动完成的回调
            OnCompleteCor();
        }

        private void UpdateCurrentSelection(float currentAngle)
        {
            currentAngle = (currentAngle % 360 + 360) % 360;

            float minDiff = float.MaxValue;
            int closestPosition = 0;

            for (int i = 0; i < _positionAngles.Length; i++)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, _positionAngles[i]));
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestPosition = i;
                }
            }

            if (closestPosition != _currentSelection)
            {
                UpdateSelection(closestPosition);
            }
        }

        private void UpdateSelection(int newSelection)
        {
            // 取消之前的选择状态
            if (_currentSelection >= 0 && _currentSelection < _wheelItems.Count)
            {
                ShowWheelItemState(_currentSelection, WheelItemState.UnSelected);
            }

            // 设置新的选择状态
            _currentSelection = newSelection;
            if (_currentSelection >= 0 && _currentSelection < _wheelItems.Count)
            {
                ShowWheelItemState(_currentSelection, WheelItemState.Turn);
            }
        }

        private void ShowWheelItemState(int index, WheelItemState state)
        {
            if (index < 0 || index >= _wheelItems.Count)
            {
                Debug.LogError("Index out of range: " + index);
                return;
            }
            GameObject go = _wheelItems[index];
            Transform imgUnSelect = Util.FindObject<Transform>(go.transform, "img_bg");
            Transform imgTurn = Util.FindObject<Transform>(go.transform, "img_turn");
            Transform imgSelect = Util.FindObject<Transform>(go.transform, "img_select");
            imgUnSelect.gameObject.SetActive(state == WheelItemState.UnSelected);
            imgTurn.gameObject.SetActive(state == WheelItemState.Turn);
            imgSelect.gameObject.SetActive(state == WheelItemState.Select);
            if (state == WheelItemState.Select)
            {
                SetItemAni(go, "select");
            }
        }

        void OnCompleteCor()
        {
            // 停止转动
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }
            
            var shopItem = _activity.SpinItems[_targetPosition];

            int shopItemId = shopItem.ShopItemId;
            if (shopItemId == WheelLuckActivity.RandomShopItemId)
            {
                shopItemId = _activity.GetRandomShopItem();
            }
            
            Messenger.Broadcast(GameDialogManager.OpenWheelLucyGetRewardDialogMsg,shopItemId);
            
            _spinState = SpinState.Normal;
         
            btnSpin.interactable = true; 
        }

        #endregion

    }
}