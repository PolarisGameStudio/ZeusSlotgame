using System;
using System.Collections;
using System.Collections.Generic;
using Activity;
using Classic;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.U2D;
using UnityEngine.UI;

public class WithDrawPanel : MonoBehaviour
{
    private Button withDrawPanelBtn; //提现按钮
    public Button withDrawBtn;
    public Transform cashTarget;
    public TextMeshProUGUI cashText;
    private DelayAction tweenAction; //动画事件
    public Tweener tweener; //DOTween动画
    private float tweenerDelay = 1f;
    private float tweenerDuration = 1.5f;
    [HideInInspector]
    public int initNum = 0;
    public GameObject Guide;
    public Image[] Images;
    public Button GuideBtn;
    private Coroutine showGuideCor;
    public GameObject tip;
    public TextMeshProUGUI tipText;
    private Tween tweenerTip; //提示文字动画
    public LocalizedString _localizedString;
    public float tipIdleTime= 5f;
    public float tipShowTime=0.3f;
    public TextMeshProUGUI scoreText;
    public void Awake()
    {
        withDrawPanelBtn = GetComponent<Button>();
        if (withDrawPanelBtn!=null)
        {
            withDrawPanelBtn.onClick.AddListener(OnWithDrawBtnClick);
        }
        if (withDrawBtn!= null)
        {
            withDrawBtn.onClick.AddListener(OnWithDrawBtnClick);
        }
        if (GuideBtn!= null)
        {
            GuideBtn.onClick.AddListener(OnWithDrawBtnClick);
        }
    }

    public void OnEnable()
    {
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            Messenger.AddListener(global::SpinButtonStyle.ENABLESPIN, OnSpinEnd);
            Messenger.AddListener(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL, OnShowTip);
        }
    }

    public void OnDisable()
    {
        Messenger.RemoveListener(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL, OnShowTip);
    }

    public void OnSpinEnd()
    {
        //首次登录
        if (UserManager.GetInstance().UserProfile().IsFirstGameSession&& gameObject.activeInHierarchy)
        {
            showGuideCor = StartCoroutine(ShowGuide());
            Messenger.RemoveListener(global::SpinButtonStyle.ENABLESPIN, OnSpinEnd);
        }
    }

    public void OnShowTip()
    {
        if (tweenerTip!=null && tip.activeInHierarchy)
        {
            return;
        }
        //做一个延时操作，等待WithDrawTaskActivity切换任务
        new DelayAction(1.1f,null,() =>
        {
            if (tipText!=null)
            {
                string info = GetTipTextInfo();
                if (string.IsNullOrEmpty(info) || info == string.Empty)
                {
                    tip.SetActive(false);
                    return;
                }
                tipText.text = info;
            }
            tip.SetActive(true);
            tip.transform.localScale = Vector3.zero;
            tip.transform.DOScale(new Vector3(1,1,1),tipShowTime).SetEase(Ease.OutBack).OnComplete(() =>
            {
                new DelayAction(tipIdleTime,null,() =>
                {
                    if (tip!=null)
                    {
                        tweenerTip = tip.transform.DOScale(new Vector3(0,0,0),tipShowTime).SetEase(Ease.InBack).OnComplete(() =>
                        {
                            if (tweenerTip!=null)
                            {
                                tweenerTip.Kill();
                                tweenerTip = null;
                            }
                            tip.SetActive(false);
                        });
                        tweenerTip.Play();
                    }
                }).Play();
            }).Play();
        }).Play();
    }

    public string GetTipTextInfo()
    {
        // int nowCash = OnLineEarningMgr.Instance.Cash();
        // int leftCash = 0;
        // int targetCash = 0;
        // if (nowCash< 500*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 500*OnLineEarningMgr.Instance.GetCashMultiple();
        // }else if (nowCash< 800*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 800*OnLineEarningMgr.Instance.GetCashMultiple();
        // }else if (nowCash< 1300*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 1300*OnLineEarningMgr.Instance.GetCashMultiple();
        // }else if (nowCash< 2000*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 2000*OnLineEarningMgr.Instance.GetCashMultiple();
        // }else if (nowCash< 3000*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 3000*OnLineEarningMgr.Instance.GetCashMultiple();
        // }else if (nowCash< 8000*OnLineEarningMgr.Instance.GetCashMultiple())
        // {
        //     targetCash = 8000*OnLineEarningMgr.Instance.GetCashMultiple();
        // }
        // leftCash = targetCash - nowCash;
        // if (_localizedString != null && !string.IsNullOrEmpty(_localizedString.GetLocalizedString()))
        // {
        //     string arg1 = string.Format("<color=#D800D9>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(leftCash, 2, false, true));
        //     string arg2 = string.Format("<color=#D800D9>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(targetCash, 2, false, true));
        //     _localizedString.Arguments = new object[] {arg1,arg2};
        //     return _localizedString.GetLocalizedString();
        // }
        WithDrawTaskActivity withDrawTaskActivity = ActivityManager.Instance.GetActivityByType(ActivityType.WithDrawTask) as WithDrawTaskActivity;
        if (withDrawTaskActivity != null)
        {
            return withDrawTaskActivity.GetTaskInfoDescForTip();
        }
        return String.Empty;
    }
    
    private IEnumerator ShowGuide()
    {
        // Messenger.Broadcast<bool>(GameConstants.ShowButtonMask,true);
        // Messenger.Broadcast<int>(GameConstants.ChangeMaskOrder,400);
        yield return new WaitForSeconds(0.5f);
        bool loadSuccess = false;
        int index = 0;
        List<string> spritePath = LocalizationManager.Instance.GetPlatFormSpriteResourcePath();
        int num = spritePath.Count;
        AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
        {
            if (result != null)
            {
                for (int i = 0; i < num; i++)
                {
                    Sprite sp = result.GetSprite(spritePath[i]);
                    if (sp != null)
                    {
                        Images[index].sprite = sp;
                        Images[index].gameObject.SetActive(true);
                        index++;
                    }
                }
            }
        });
       
        yield return GameConstants.FrameTime;
        StopCoroutine(showGuideCor);
        Guide.SetActive(true);
    }
    //初始化设置钱数
    public virtual void InitMoney(int money)
    {
        initNum = money;
        cashText.text = OnLineEarningMgr.Instance.GetMoneyStr(money,needIcon:false);
    }
    
    public void OnWithDrawBtnClick()
    {
        if (Guide.activeInHierarchy)
        {
            // Messenger.Broadcast<bool>(GameConstants.ShowButtonMask,false);
            Guide.SetActive(false);
        }
        WithDrawManager.Instance.ShowWithDrawDialog();
    }

    public virtual void ShowWithCoinsFly(int coinsNum)
    {
        SetCoinsNum(coinsNum);
    }
    public void SetCoinsNum(int number)
    {
        if (tweenAction != null && tweenAction.IsPlaying)
        {
            tweenAction.Stop(true);
        }

        if (tweener != null)
        {
            tweener.Kill(true);
        }

        if (number > initNum)
        {
            tweenAction = new DelayAction(tweenerDelay, null, () =>
            {
                tweener = DOTween.To(() => this.initNum, x => this.initNum = x, number, tweenerDuration)
                    .OnUpdate(()=>CaculateTxt())
                    .OnComplete(() =>
                    {
                        tweenerDelay = 1f;
                        tweenerDuration = 1.5f;
                        CompleteShow(number);
                    }).SetUpdate(true);
            });
            tweenAction.Play();
            if (scoreText!=null)
            {
                //播放score分数飞行动画
                scoreText.text = "+" + OnLineEarningMgr.Instance.GetMoneyStr(number - initNum,needIcon:false);
                DoScoreAnim();
            }
        }
        else
        {
            CompleteShow(number);
        }
    }

    public virtual void DoScoreAnim(System.Action onComplete = null)
    {
        if (scoreText == null) return;

        scoreText.gameObject.SetActive(true);
        scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 1);

        // 记录初始状态
        Vector3 initialPos = scoreText.transform.localPosition;
        Vector3 initialScale = scoreText.transform.localScale; // 保存初始缩放值

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 第一阶段：向上移动 + 同步放大
        sequence.Append(
            scoreText.transform.DOLocalMoveY(initialPos.y + 20, 0.5f)
                .SetEase(Ease.OutCubic)
        );
        sequence.Join(
            scoreText.transform.DOScale(initialScale * 1.2f, 0.5f) // 放大到120%
                .From(initialScale * 0.8f) // 从80%开始缩放
                .SetEase(Ease.OutBack) // 带弹性效果的缓动
        );

        // 停留1.5秒
        sequence.AppendInterval(1.5f);

        // 第二阶段：继续上移 + 淡出
        sequence.Append(
            scoreText.transform.DOLocalMoveY(initialPos.y + 40, 0.5f) // 总上移40单位
                .SetEase(Ease.InCubic)
        );
        sequence.Join(
            scoreText.DOFade(0, 0.5f)
        );

        // 动画完成回调
        sequence.OnComplete(() =>
        {
            scoreText.gameObject.SetActive(false);
            scoreText.transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z);
            scoreText.transform.localScale = initialScale; // 恢复初始缩放
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 1);
            onComplete?.Invoke();
        });

        sequence.Play();
    }
    
    public virtual void CompleteShow(int coins)
    {
        this.initNum = coins;
        this.CaculateTxt();
    }
    
    public virtual void CaculateTxt()
    {
        if (cashText == null)
        {
            if (tweener != null)
            {
                tweener.Kill();
            }
            return;
        }
        SetCashText();
    }

    public virtual void SetCashText()
    {
        cashText.text = OnLineEarningMgr.Instance.GetMoneyStr(initNum,needIcon:false);
    } 
}
