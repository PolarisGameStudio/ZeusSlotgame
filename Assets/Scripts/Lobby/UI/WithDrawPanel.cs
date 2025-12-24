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

/// <summary>
/// 提现面板控制类
/// 负责提现按钮、现金显示、提示文本、引导动画等UI逻辑
/// </summary>
public class WithDrawPanel : MonoBehaviour
{
    #region UI组件引用

    private Button withDrawPanelBtn; // 提现面板按钮（整个面板可点击）
    public Button withDrawBtn; // 提现按钮
    public Transform cashTarget; // 现金目标位置（用于动画）
    public TextMeshProUGUI cashText; // 现金数量文本
    public TextMeshProUGUI scoreText; // 增加分数显示文本（飞行动画）

    // 提示相关UI
    public GameObject tip; // 提示文本容器
    public TextMeshProUGUI tipText; // 提示文本内容
    public LocalizedString _localizedString; // 本地化字符串

    // 引导相关UI
    public GameObject Guide; // 引导UI容器
    public Image[] Images; // 引导图片数组（平台图标）
    public Button GuideBtn; // 引导按钮

    public GameObject withdrawTip; // 提现提示UI（Tap to cash）
    public GameObject contentGroup; // 内容组容器

    #endregion

    #region 动画和协程相关

    private DelayAction tweenAction; // 延迟动画事件
    public Tweener tweener; // DOTween动画对象（现金数字变化动画）
    private Tween tweenerTip; // 提示文字动画
    private float tweenerDelay = 1f; // 动画延迟时间
    private float tweenerDuration = 1.5f; // 动画持续时间
    public float tipIdleTime = 5f; // 提示停留时间
    public float tipShowTime = 0.3f; // 提示显示/隐藏动画时间

    private Coroutine showGuideCor; // 显示引导协程（首次登录）
    private Coroutine showGuideCor1; // 显示引导文本协程（达到提现金额）

    #endregion

    #region 数据字段

    [HideInInspector]
    public int initNum = 0; // 当前现金数量
    private bool isDoingWithDraw = true; // 是否正在进行提现操作
    private bool SequencePlaying = false; // 分数飞行动画是否正在播放

    #endregion

    #region Unity生命周期方法

    /// <summary>
    /// 初始化按钮事件监听
    /// </summary>
    public void Awake()
    {
        // 获取并绑定提现面板按钮
        withDrawPanelBtn = GetComponent<Button>();
        if (withDrawPanelBtn!=null)
        {
            withDrawPanelBtn.onClick.AddListener(OnWithDrawBtnClick);
        }

        // 绑定提现按钮
        if (withDrawBtn!= null)
        {
            withDrawBtn.onClick.AddListener(OnWithDrawBtnClick);
        }

        // 绑定引导按钮
        if (GuideBtn!= null)
        {
            GuideBtn.onClick.AddListener(OnWithDrawBtnClick);
        }
    }

    /// <summary>
    /// 激活时注册事件监听
    /// </summary>
    public void OnEnable()
    {
        // 非白包平台才监听这些事件
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            // 监听spin结束事件，用于首次登录引导
            Messenger.AddListener(global::SpinButtonStyle.ENABLESPIN, OnSpinEnd);
            // 监听显示提现提示面板事件
            Messenger.AddListener(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL, OnShowTip);
        }

        // 监听提现完成事件
        Messenger.AddListener<int>(WithDrawConstants.DoneWithDrawAction, DoneWithDrawAction);
        // 监听关闭引导消息事件
        Messenger.AddListener(System.WithDrawConstants.CloseWithDrawGuideMsg, HideGuide);
    }

    /// <summary>
    /// 禁用时移除事件监听
    /// </summary>
    public void OnDisable()
    {
        Messenger.RemoveListener(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL, OnShowTip);
        Messenger.RemoveListener<int>(WithDrawConstants.DoneWithDrawAction, DoneWithDrawAction);
        Messenger.RemoveListener(System.WithDrawConstants.CloseWithDrawGuideMsg, HideGuide);
    }

    #endregion

    #region 事件响应方法

    /// <summary>
    /// Spin结束时的响应方法
    /// 用于首次登录时显示引导UI
    /// </summary>
    public void OnSpinEnd()
    {
        // 仅在首次游戏会话且面板激活时触发
        if (UserManager.GetInstance().UserProfile().IsFirstGameSession && gameObject.activeInHierarchy)
        {
            contentGroup.gameObject.SetActive(true);

            // 【已注释】不再隐藏 withdrawTip，改由 WithDrawPromptDialog 代替
            // withdrawTip.gameObject.SetActive(false);

            // 开启引导显示协程
            showGuideCor = StartCoroutine(ShowGuide());

            // 移除监听器，避免重复触发
            Messenger.RemoveListener(global::SpinButtonStyle.ENABLESPIN, OnSpinEnd);
        }
    }

    /// <summary>
    /// 显示提现提示的响应方法
    /// 根据当前现金和目标档位显示不同的提示UI
    /// </summary>
    public void OnShowTip()
    {
        // 获取当前现金数量
        int cash = OnLineEarningMgr.Instance.Cash();
        // 获取目标提现档位金额
        int targetCash = WithDrawManager.Instance.GetTaskLevelCash(isDoingWithDraw)*OnLineEarningMgr.Instance.GetCashMultiple();

        // 当前没有档位时，不再显示任何提示
        if (targetCash == 0)
        {
            return;
        }

        // 现金未达到目标档位，显示进度提示文本
        if (cash<targetCash)
        {
            // 如果提示文本动画正在播放且tip已显示，则不重复显示
            if (tweenerTip!=null && tip.activeInHierarchy)
            {
                return;
            }

            // 构建并显示提示文本（例如："再收集 $XX 即可提现 $XX"）
            if (tipText!=null)
            {
                string info = GetTipTextInfo(targetCash);
                if (string.IsNullOrEmpty(info) || info == string.Empty)
                {
                    tip.SetActive(false);
                    return;
                }
                tipText.text = info;
            }

            // 显示提示文本，带缩放动画效果
            tip.SetActive(true);
            tip.transform.localScale = Vector3.zero;
            tip.transform.DOScale(new Vector3(1,1,1),tipShowTime).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // 停留一段时间后自动隐藏
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
        }
        else
        {
            // 【核心修改】现金已达到提现档位，弹出 WithDrawPromptDialog
            isDoingWithDraw = false;

            // 广播弹出 WithDrawPromptDialog
            Messenger.Broadcast<int>(GameDialogManager.OpenWithDrawPromptDialogMsg, cash);

            // 【已注释】不再显示 withdrawTip，改由 WithDrawPromptDialog 代替
            // withdrawTip.gameObject.SetActive(true);

            contentGroup.gameObject.SetActive(false);

            // 停止之前的引导协程
            if (showGuideCor1!=null)
            {
                StopCoroutine(showGuideCor1);
                showGuideCor1 = null;
            }

            // 显示引导文本（Tap to cash）
            showGuideCor1 = StartCoroutine(ShowGuideText());
        }
    }

    /// <summary>
    /// 构建提示文本信息
    /// 显示"再收集 $XX 即可提现 $XX"格式的文本
    /// </summary>
    /// <param name="targetCash">目标提现档位金额</param>
    /// <returns>本地化后的提示文本</returns>
    private string GetTipTextInfo(int targetCash)
    {
        int nowCash = OnLineEarningMgr.Instance.Cash();
        int leftCash = targetCash - nowCash;

        LocalizedString _localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"CollectToWithdrawTips");
        if (_localizedString != null && !string.IsNullOrEmpty(_localizedString.GetLocalizedString()))
        {
            // 构建高亮的金额参数
            string arg1 = string.Format("<color=#D800D9>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(leftCash, 2, false, true));
            string arg2 = string.Format("<color=#D800D9>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(targetCash, 2, false, true));
            _localizedString.Arguments = new object[] {arg1,arg2};
            return _localizedString.GetLocalizedString();
        }
        return String.Empty;
    }

    /// <summary>
    /// 提现完成后的回调
    /// 重置提现状态标记
    /// </summary>
    /// <param name="money">提现金额</param>
    private void DoneWithDrawAction(int money)
    {
        isDoingWithDraw = true;
    }

    /// <summary>
    /// 提现按钮点击事件
    /// 关闭引导并打开提现弹窗
    /// </summary>
    public void OnWithDrawBtnClick()
    {
        // 如果引导UI正在显示，先关闭它
        if (Guide.activeInHierarchy)
        {
            Guide.SetActive(false);
        }

        // 打开提现弹窗
        WithDrawManager.Instance.ShowWithDrawDialog();
    }

    /// <summary>
    /// 隐藏引导UI
    /// </summary>
    private void HideGuide()
    {
        if (Guide != null)
        {
            Guide.SetActive(false);
            Debug.Log("[WithDrawPanel] Guide已隐藏");
        }
    }

    #endregion

    #region 引导相关方法

    /// <summary>
    /// 显示首次登录引导协程
    /// 加载并显示平台图标
    /// </summary>
    private IEnumerator ShowGuide()
    {
        yield return new WaitForSeconds(0.5f);
        bool loadSuccess = false;
        int index = 0;

        // 获取平台图标资源路径
        List<string> spritePath = LocalizationManager.Instance.GetPlatFormSpriteResourcePath();
        int num = spritePath.Count;

        // 加载平台图标 SpriteAtlas
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

        // 显示引导UI
        Guide.SetActive(true);
    }

    /// <summary>
    /// 显示引导文本协程
    /// 达到提现金额时显示 "Tap to cash" 引导
    /// </summary>
    private IEnumerator ShowGuideText()
    {
        yield return new WaitForSeconds(0.5f);
        StopCoroutine(showGuideCor1);

        // 显示引导UI
        Guide.SetActive(true);
    }

    #endregion

    #region 现金显示相关方法

    /// <summary>
    /// 初始化设置现金数量
    /// </summary>
    /// <param name="money">初始现金数量</param>
    public virtual void InitMoney(int money)
    {
        initNum = money;
        cashText.text = OnLineEarningMgr.Instance.GetMoneyStr(money,needIcon:false);
    }

    /// <summary>
    /// 显示现金飞行动画
    /// </summary>
    /// <param name="coinsNum">新的现金数量</param>
    public virtual void ShowWithCoinsFly(int coinsNum)
    {
        SetCoinsNum(coinsNum);
    }

    /// <summary>
    /// 设置现金数量（带数字滚动动画）
    /// </summary>
    /// <param name="number">目标现金数量</param>
    public void SetCoinsNum(int number)
    {
        // 停止正在播放的延迟动作
        if (tweenAction != null && tweenAction.IsPlaying)
        {
            tweenAction.Stop(true);
        }

        // 停止正在播放的数字滚动动画
        if (tweener != null)
        {
            tweener.Kill(true);
        }

        // 仅当新数量大于当前数量时播放增长动画
        if (number > initNum)
        {
            tweenAction = new DelayAction(tweenerDelay, null, () =>
            {
                // 创建数字滚动动画
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

            // 显示增加的分数飞行动画
            if (scoreText!=null)
            {
                scoreText.text = "+" + OnLineEarningMgr.Instance.GetMoneyStr(number - initNum,needIcon:false);
                DoScoreAnim();
            }
        }
        else
        {
            CompleteShow(number);
        }
    }

    /// <summary>
    /// 动画完成后的显示
    /// </summary>
    /// <param name="coins">最终现金数量</param>
    public virtual void CompleteShow(int coins)
    {
        this.initNum = coins;
        this.CaculateTxt();
    }

    /// <summary>
    /// 计算并更新现金文本
    /// </summary>
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

    /// <summary>
    /// 设置现金文本内容
    /// </summary>
    public virtual void SetCashText()
    {
        cashText.text = OnLineEarningMgr.Instance.GetMoneyStr(initNum,needIcon:false);
    }

    #endregion

    #region 分数飞行动画

    /// <summary>
    /// 执行分数飞行动画
    /// 分数文本向上飞行并放大，然后淡出
    /// </summary>
    /// <param name="onComplete">动画完成回调</param>
    public virtual void DoScoreAnim(System.Action onComplete = null)
    {
        if (scoreText == null) return;

        // 防止重复播放
        if (SequencePlaying)
        {
            return;
        }

        scoreText.gameObject.SetActive(true);
        scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 1);

        // 记录初始状态
        Vector3 initialPos = scoreText.transform.localPosition;
        Vector3 initialScale = scoreText.transform.localScale;

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        SequencePlaying = true;

        // 第一阶段：向上移动 + 同步放大
        sequence.Append(
            scoreText.transform.DOLocalMoveY(initialPos.y + 20, 0.5f)
                .SetEase(Ease.OutCubic)
        );
        sequence.Join(
            scoreText.transform.DOScale(initialScale * 1.4f, 0.5f) // 放大到140%
                .From(initialScale)
                .SetEase(Ease.OutBack) // 带弹性效果的缓动
        );

        // 停留2.5秒
        sequence.AppendInterval(2.5f);

        // 第二阶段：继续上移 + 淡出
        sequence.Append(
            scoreText.transform.DOLocalMoveY(initialPos.y + 40, 0.5f) // 总上移40单位
                .SetEase(Ease.InCubic)
        );
        sequence.Join(
            scoreText.DOFade(0, 0.5f)
        );

        // 动画完成回调：重置状态
        sequence.OnComplete(() =>
        {
            scoreText.gameObject.SetActive(false);
            scoreText.transform.localPosition = new Vector3(initialPos.x, initialPos.y, initialPos.z);
            scoreText.transform.localScale = initialScale;
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 1);
            onComplete?.Invoke();
            SequencePlaying = false;
        });

        sequence.Play();
    }

    #endregion
}
