using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using App;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Beebyte.Obfuscator;

namespace Libs
{
	[Skip]
	public class CoinsBezier : MonoBehaviour
	{
		public GameObject BezierObject;
		public GameObject CoinEffect;
		private GameObject _coinEffect;
		private  string BezierObjectPath = "Prefab/Shared/CashImage";
		private  string BezierPanelPath = "Prefab/Shared/CoinsBezierPanel";
		public Transform targatTansform;
		public Transform startTransform;
		private static CoinsBezier _instance;

		private DelayAction stopCoinShakeDelay = null;
		private Animator coinShakeAnimator;
		public float coinShakeTime;

		// 用来描述曲线与运动过程中变化的相关参数
		// 曲线的中间点的相对坐标
		private static List<List<Vector3>> bezierMiddlePoints;
		// 曲线中间点的相对随机偏移量范围
		private static List<float> bezierMiddlePointsOffset;
		// 描述硬币随运动的大小变化
		private static List<List<Vector2>> sizeScaleChangePolylines;
		// 描述硬币在何时运动到曲线的何处
		private static List<List<Vector2>> timeScaleChangePolylines;
		// 硬币从起点运动到终点的时间范围
		private static List<Vector2> timeDuration;
		// 硬币出现的持续时间
		private static List<float> popDuration;

		// 用来在unity中实时调试曲线的参数，调完之后记得写到代码里。仅支持对一段曲线的调试
		[Header("是否启用调试")]
		public bool testOn;
		[Header("硬币的出现个数")]
		public int coinNumTest;
		[Header("曲线的中间点的相对坐标")]
		public List<Vector3> bezierMiddlePointsTest;
		[Header("曲线中间点的相对随机偏移量范围")]
		public float bezierMiddlePointsOffsetTest;
		[Header("描述硬币在何时运动到曲线的何处")]
		public List<Vector2> timeScaleChangePolylinesTest;
		[Header("描述硬币随运动的大小变化")]
		public List<Vector2> sizeScaleChangePolylinesTest;
		[Header("硬币从起点运动到终点的时间范围")]
		public Vector2 timeDurationTest;
		[Header("硬币出现的持续时间")]
		public float popDurationTest;

		// 购买特效的两段曲线专属相关参数，可在unity直接调节
		[Header("第一段曲线的下落高度范围")]
		public float splash1EndMin, splash1EndMax;
		[Header("第一段曲线的横向散布宽度")]
		public float splash1Width;
		[Header("第一段曲线的中间点相对位置X坐标范围")]
		public Vector3 splash1StandardMidpointXRange;
		[Header("第一段曲线的中间点相对位置Y坐标范围")]
		public Vector3 splash1StandardMidpointYRange;
		[Header("第二段曲线的中间点相对位置")]
		public Vector3 splash2StandardMidpoint;
		[Header("第一段曲线的运动时间范围")]
		public float splash1TimeMin, splash1TimeMax;
		[Header("第二段曲线的运动时间范围")]
		public float splash2TimeMin, splash2TimeMax;
		[Header("第一段曲线，描述硬币在何时运动到曲线的何处")]
		public List<Vector2> timeScaleChangePolylinesSplash1;
		[Header("第一段曲线，描述硬币随运动的大小变化")]
		public List<Vector2> sizeScaleChangePolylinesSplash1;
		[Header("第二段曲线，描述硬币在何时运动到曲线的何处")]
		public List<Vector2> timeScaleChangePolylinesSplash2;
		[Header("第二段曲线，描述硬币随运动的大小变化")]
		public List<Vector2> sizeScaleChangePolylinesSplash2;
		[Header("硬币绕Z轴旋转的速度，非动画实现")]
		public Vector3 splashCoinRotateSpeed;
		
		public static Vector2 iphoneScale = new Vector2(0.46f,0.46f);
		public static Vector2 iphoneXScale = new Vector2(0.38f, 0.38f);
		public static Vector2 ipadScale = new Vector2(0.7f, 0.7f);
		// 单例模式
		public static CoinsBezier Instance {
			get {
				if (_instance == null) {
					// 尝试获取场景中已存在的，active的同类对象
					_instance = GameObject.FindObjectOfType<CoinsBezier> ();
					if (_instance == null) {
						// 初始化
						_instance = Instantiate(ResourceLoadManager.Instance.LoadResource<GameObject> ("Prefab/Shared/CoinsBezierPanel")).GetComponent<CoinsBezier> ();
						// 向场景中导入指定prefab，并从中获取对象
						// 设置transform并移动至最上层
						_instance.transform.SetParent (Libs.UIManager.Instance.Root.transform.parent);
						(_instance.transform as RectTransform).localPosition = new Vector3(0,0,0);
						
						_instance.transform.SetAsLastSibling ();
					
					}
					// 导入指定的运动物体
					_instance.BezierObject = ResourceLoadManager.Instance.LoadResource<GameObject> (_instance.BezierObjectPath);
					Canvas canvas = _instance.GetComponent<Canvas> ();
					if (canvas != null) {
						canvas.overrideSorting = true;
					}

					// 从代码中导入相关参数
					BezierMiddlePointSettings();
					BezierMiddlePointOffsetSettings();
					SizeScaleChangeSettings();
					TimeScaleChangeSettings();
					TimeDurationSettings();
					PopDurationSettings();
				}
				if (SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait)
				{
					if (IphoneXAdapter.IsIphoneX())
					{
						(_instance.transform as RectTransform).localScale = new Vector3(iphoneXScale.x, iphoneXScale.y, 1);
					}
					else if (SkySreenUtils.GetScreenSizeType() == SkySreenUtils.ScreenSizeType.Size_16_9)
					{
						(_instance.transform as RectTransform).localScale = new Vector3(iphoneScale.x, iphoneScale.y, 1);
					}
					else if (SkySreenUtils.GetScreenSizeType() == SkySreenUtils.ScreenSizeType.Size_4_3)
					{
						(_instance.transform as RectTransform).localScale = new Vector3(ipadScale.x, ipadScale.y, 1);
					}
				}
				else
				{
					(_instance.transform as RectTransform).localScale = new Vector3(1, 1, 1);
				}
				return _instance;
			}
		}

		public Vector3 LocalPositionFromInputMouse()
		{
			Vector2 v;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Libs.CoinsBezier.Instance.transform.transform as RectTransform,new Vector2(Input.mousePosition.x,Input.mousePosition.y),Libs.UIManager.Instance.UICamera,out v);
			return new Vector3(v.x,v.y,0);
		}

		public Vector3 LocalPositionFromTransForm(Camera camera, Transform targatTansform)
		{
			Vector2 endPos;
			Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(camera,targatTansform.position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Libs.CoinsBezier.Instance.transform.transform as RectTransform,endScreen,Libs.UIManager.Instance.UICamera,out endPos);
			return endPos;
		}

		private void PlayCoinEffect(Vector3 fromPosition)
		{
			if(CoinEffect==null)return;
			if (_coinEffect!=null)
			{
				_coinEffect.SetActive(false);
				_coinEffect.SetActive(true);
				_coinEffect.transform.localPosition = fromPosition;
			}
			else
			{
				_coinEffect = Instantiate(CoinEffect) as GameObject;
				_coinEffect.transform.SetParent(transform, false);
				_coinEffect.transform.localPosition = fromPosition;
				_coinEffect.name = "CoinEffect";
			}
		}

		private void StopCoinEffect()
		{
			if(CoinEffect==null)return;
			_coinEffect = this.transform.Find("CoinEffect").gameObject;
			if (_coinEffect!=null)
			{
				_coinEffect.SetActive(false);
			}
		}

		public void Create (Vector3 fromPosition, Vector3 toPosition, int count = 10)
		{
			GetAnimator ();
//			CreateSplash(fromPosition, toPosition, BezierType.Test, count); return;
			for (int i=0; i<count; i++) {
//				CoinBezierObject element = GameObject.Instantiate (skyBezierObject).GetComponent<CoinBezierObject> ();
				// 从池中取物体，节省性能
				CoinBezierObject element = (PoolMgr.DefaultPools.GetOrCreatePool("BezierCoins").CreateObject( BezierObject) as GameObject).GetComponent<CoinBezierObject>();
				// 相关设置（位置、路径）
				element.gameObject.SetActive (true);
				element.skyBezierCurve = new SkyBezierCurve ();
				element.skyBezierCurve.endPoint = toPosition; //targatTansform.parent.transform.localPosition + targatTansform.localPosition;
				element.transform.SetParent (this.transform, false);
				element.transform.localPosition = fromPosition; //new Vector3(220,550,0	); //startTransform.parent.transform.localPosition + startTransform.localPosition;
				element.transform.localScale = Vector3.zero;
				Vector3 temp1 = Vector3.zero;
				temp1.x = genOffset () + element.transform.localPosition.x;
				temp1.y = genOffset ();
				Vector3 temp = Vector3.zero;
				temp.x = genOffset () + element.skyBezierCurve.endPoint.x;
				temp.y = genOffset ();
				element.skyBezierCurve.middlePoints.Clear ();
				element.skyBezierCurve.middlePoints.Add (temp1);
				element.skyBezierCurve.middlePoints.Add (temp);

				element.sizeScaleChangePolyline = new List<Vector2>();
				element.sizeScaleChangePolyline.Add(new Vector2(0f, 0.5f));
				element.sizeScaleChangePolyline.Add(new Vector2(1f, 0.5f));
				element.timeScaleChangePolyline = new List<Vector2>();
				element.timeScaleChangePolyline.Add(new Vector2(0f, 0f));
				element.timeScaleChangePolyline.Add(new Vector2(1f, 1f));
				// 运动时间
				element.skyBezierCurve.timeDuration = UnityEngine.Random.Range (1.3f, 1.8f);
				// 旋转速度
				element.rotateSpeed = Vector3.zero;
				// 设置运动开始和结束的回调
				bezierObjects.Add(element);
				element.MOnAnimationStart = new CoinBezierObject.MCallBack (() => {
				});
				element.MOnAnimationCompleted = new CoinBezierObject.MCallBack (() => {
					// 运动结束时的扫尾工作
                    KillCoin(element);
                    bezierObjects.Remove(element);
                    PlayCoinAnimation();
				});
				// 开始运动
				element.startAnimation (i * 1.1f / count);
            }
		}

		// 生成普通（一段曲线）的飞行金币，用于免费获取的场合，可配置参数增加
        public void Create (Vector3 fromPosition, Vector3 toPosition, BezierType bezierType, System.Action lastCoinCallback = null, int count = 16)
		{
			new Libs.DelayAction(0.3f, null, () =>
			{
				PlayCoinEffect(fromPosition);
				PlayCoinFly(fromPosition,toPosition,bezierType,lastCoinCallback,count);
			}).Play();
		}

		private void PlayCoinFly(Vector3 fromPosition, Vector3 toPosition, BezierType bezierType, System.Action lastCoinCallback = null, int count = 16)
		{
			System.Action callback = ()=>
			{
				AudioEntity.Instance.StopCoinCollectionEffect();
				if(lastCoinCallback != null) lastCoinCallback();
				StopCoinEffect();
			};

			GetAnimator ();
			if (bezierType == BezierType.Purchase) {
                CreateSplash(fromPosition, toPosition, bezierType, callback);
				return;
			}
			if (testOn) {count = coinNumTest;}
			int index = bezierType == BezierType.ByPosition ? GetBezierSettingIndex(toPosition) : GetBezierSettingIndex(bezierType);

            float minEndTime = float.MaxValue, maxEndTime = 0;
			float startTime = Time.realtimeSinceStartup;

            int arrivedCoin = 0;
			
			for (int i = 0; i < count; i++) {
				// 从池中取物体，节省性能
				CoinBezierObject element = (PoolMgr.DefaultPools.GetOrCreatePool("BezierCoins").CreateObject( BezierObject) as GameObject).GetComponent<CoinBezierObject>();
				// 相关设置（位置、路径）
				element.gameObject.SetActive (true);
//				element.gameObject.GetComponent<Animator>().enabled = false;
				element.transform.SetParent (this.transform, false);
				element.transform.localPosition = fromPosition;
				element.transform.localScale = new Vector3(sizeScaleChangePolylines[index][0].y, sizeScaleChangePolylines[index][0].y, 1f);

				element.skyBezierCurve = new SkyBezierCurve ();
				element.skyBezierCurve.startPoint = element.transform.localPosition;
				element.skyBezierCurve.endPoint = toPosition;
				element.skyBezierCurve.middlePoints.Clear ();
				for (int j = 0; j < (testOn ? bezierMiddlePointsTest.Count : bezierMiddlePoints[index].Count); j++) {
					element.skyBezierCurve.AddMiddlePointRelatively((testOn ? bezierMiddlePointsTest[j] : bezierMiddlePoints[index][j])
						* (1f + (testOn ? Random.Range(-bezierMiddlePointsOffsetTest, bezierMiddlePointsOffsetTest) : Random.Range(-bezierMiddlePointsOffset[index], bezierMiddlePointsOffset[index]))));
				}
				element.sizeScaleChangePolyline = testOn ? sizeScaleChangePolylinesTest : sizeScaleChangePolylines[index];
				element.timeScaleChangePolyline = testOn ? timeScaleChangePolylinesTest : timeScaleChangePolylines[index];
				// 旋转速度
				element.rotateSpeed = Vector3.zero;

				// 运动时间
                float duration = UnityEngine.Random.Range
                    (testOn ? timeDurationTest.x : timeDuration[index].x, testOn ? timeDurationTest.y : timeDuration[index].y);
                element.skyBezierCurve.timeDuration = duration;
				// 设置运动开始和结束的回调
				bezierObjects.Add(element);
				element.MOnAnimationStart = new CoinBezierObject.MCallBack (() => {
				});
				element.MOnAnimationCompleted = new CoinBezierObject.MCallBack (() => {
                    // 运动结束时的扫尾工作
                    KillCoin(element);
                    bezierObjects.Remove(element);
                    PlayCoinAnimation();

                    arrivedCoin++;
                    if (callback != null && arrivedCoin == count) {
                        callback();
                    }
				});

                // 开始运动
                float delay = i * (testOn ? popDurationTest : popDuration[index]) / count;
                element.startAnimation (delay);

                duration /= 2;
                if (delay + duration < minEndTime) {
                    minEndTime = delay + duration;
                }
                if (delay + duration > maxEndTime)
                {
                    maxEndTime = delay + duration;
                }
			}
            float sigma = 0f;
			Messenger.Broadcast(GameConstants.SET_COINSPANEL_TWEENER_PARAM, minEndTime - sigma, maxEndTime - minEndTime);
		}

		// 生成抛洒-收拢（二段曲线）的飞行金币，用于付费获取的场合
        public void CreateSplash(Vector3 fromPosition, Vector3 toPosition, BezierType bezierType, System.Action lastCoinCallback = null, int count = 60)
        {
			GetAnimator ();
            float minEndTime = float.MaxValue, maxEndTime = 0;

            int arrivedCoin = 0;

			for (int i = 0; i < count; i++) {
				float offset = Random.Range(-1f, 1f);
//				offset = -1f + 2f * i / (count - 1);
				// 从池中取物体，节省性能
				CoinBezierObject element = (PoolMgr.DefaultPools.GetOrCreatePool("BezierCoins").CreateObject( BezierObject) as GameObject).GetComponent<CoinBezierObject>();
				// 起始位置、起始大小设置
				element.gameObject.SetActive (true);
				element.gameObject.GetComponent<Animator>().enabled = false;

				element.transform.SetParent (this.transform, false);
				element.transform.localPosition = fromPosition;
				element.transform.localScale = new Vector3(0f, 0f, 0f);

				// 曲线参数设置
				element.skyBezierCurve = new SkyBezierCurve ();
				element.skyBezierCurve.endPoint = new Vector3(fromPosition.x + offset * splash1Width, Random.Range(splash1EndMin, splash1EndMax), 0);
				element.skyBezierCurve.startPoint = element.transform.localPosition;
				element.skyBezierCurve.middlePoints.Clear ();

				element.skyBezierCurve.AddMiddlePointRelatively(new Vector3(Random.Range(splash1StandardMidpointXRange.x, splash1StandardMidpointXRange.y)
					, Random.Range(splash1StandardMidpointYRange.x, splash1StandardMidpointYRange.y), 0));

				// 旋转速度
				element.rotateSpeed = splashCoinRotateSpeed;

//				CoinBezierObject midpoint = GameObject.Instantiate(element);
//				midpoint.transform.SetParent (this.transform, false);
//				midpoint.transform.localPosition = element.skyBezierCurve.middlePoints[0];

				element.sizeScaleChangePolyline = sizeScaleChangePolylinesSplash1;
				element.timeScaleChangePolyline = timeScaleChangePolylinesSplash1;

				// 运动时间
                float duration1 = Random.Range(splash1TimeMin, splash1TimeMax);
                element.skyBezierCurve.timeDuration = duration1;
				// 设置运动开始和结束的回调
				bezierObjects.Add(element);
				element.MOnAnimationStart = new CoinBezierObject.MCallBack (() => {
				});
                float duration2 = Random.Range(splash2TimeMin, splash2TimeMax);
				element.MOnAnimationCompleted = new CoinBezierObject.MCallBack (() => {
                    //第二段曲线
					element.skyBezierCurve = new SkyBezierCurve ();
					element.skyBezierCurve.endPoint = toPosition;
					element.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
					element.skyBezierCurve.startPoint = element.transform.localPosition;

					element.skyBezierCurve.middlePoints.Clear ();
					element.skyBezierCurve.AddMiddlePointRelatively(splash2StandardMidpoint * offset);
					element.sizeScaleChangePolyline = sizeScaleChangePolylinesSplash2;
					element.timeScaleChangePolyline = timeScaleChangePolylinesSplash2;

                    // 运动时间
                    element.skyBezierCurve.timeDuration = duration2;
					// 开始运动
					element.startAnimation(0);
					// 设置运动开始和结束的回调
					element.MOnAnimationStart = new CoinBezierObject.MCallBack (() => {
					});
					element.MOnAnimationCompleted = new CoinBezierObject.MCallBack (() => {
						// 运动结束时的扫尾工作
                        KillCoin(element);
                        bezierObjects.Remove(element);
						PlayCoinAnimation();

                        arrivedCoin++;
                        if (lastCoinCallback != null && arrivedCoin == count) {
                            lastCoinCallback();
                        }
					});
				});
                // 开始运动
                float delay = Random.Range(0f, 1.5f);
				element.startAnimation (delay);

                if (delay + (duration1 + duration2) / 2 < minEndTime)
                {
                    minEndTime = delay + (duration1 + duration2) / 2;
                }
                if (delay + (duration1 + duration2) / 2 > maxEndTime)
                {
                    maxEndTime = delay + (duration1 + duration2) / 2;
                }
			}
            float sigma = 0f;
			Messenger.Broadcast(GameConstants.SET_COINSPANEL_TWEENER_PARAM, minEndTime - sigma, maxEndTime - minEndTime);
		}

        // 停止所有的金币，令其隐藏
        public void KillAllCoins() {
            foreach (CoinBezierObject element in bezierObjects) {
                KillCoin(element);
            }
            bezierObjects.Clear();
        }

        private void KillCoin(CoinBezierObject element) {
//            element.gameObject.SetActive(false);
//            StartCoroutine(UI.Utils.UIUtil.DelayAction(2f, delegate () {
                PoolMgr.DefaultPools.GetOrCreatePool("BezierCoins").DestoryObject(element.gameObject);
//            }));
        }

		private void GetAnimator()
		{
			// if (coinShakeAnimator == null) {
			// 	if (BaseGameConsole.singletonInstance.IsInLobby ()) {
			// 		coinShakeAnimator = BaseGameConsole.singletonInstance.LobbyController.CoinsTransform.gameObject.GetComponent<Animator>();
			// 	} else {
			// 		coinShakeAnimator = BaseGameConsole.singletonInstance.SlotMachineController.CoinsTransform.gameObject.GetComponent<Animator>();
			// 	}
			// }
			// coinShakeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
		}

		// 播放终点的金币动画，在金币到达终点时调用
		// 一定时间后停止动画，若在此之前又有新的金币到达（函数被调用）则重置计时器
		private void PlayCoinAnimation() {
			if (stopCoinShakeDelay == null) {
				stopCoinShakeDelay = new DelayAction(coinShakeTime, null, StopCoinAnimation);
			}
			if (stopCoinShakeDelay.IsPlaying) {
				stopCoinShakeDelay.Restart();
			}
			else {
				if (coinShakeAnimator != null) {
					coinShakeAnimator.SetTrigger ("Play");
					stopCoinShakeDelay.Play();
				}
			}
		}

		// 停止终点的金币动画
		private void StopCoinAnimation() {
			if (coinShakeAnimator != null) {
				coinShakeAnimator.SetTrigger("Stop");
			}
		}
	
		private float genOffset ()
		{
			return UnityEngine.Random.Range (-100, 100);
		}

		private List<Object> bezierObjects = new List<Object>();

		// 曲线的种类
		public enum BezierType {
			ByPosition = -1,
			DailyBonus = 0,
			EmailBonus = 1,
			Inbox = 2,
            WelcomVegas = 3,
			Purchase = 100,
			Purchase1 = 101,
			Purchase2 = 102,
			Test = 999
		}

		// 这两个方法会返回应该使用的参数的编号
		private int GetBezierSettingIndex(BezierType bezierType) {
			switch (bezierType) {
			case BezierType.ByPosition:
				return -1;

			case BezierType.DailyBonus:
				return 0;

			case BezierType.EmailBonus:
				return 1;

			case BezierType.Inbox:
				return 2;

			case BezierType.Purchase1:
				return 3;

			case BezierType.Purchase2:
				return 4;

            case BezierType.WelcomVegas:
                return 6;

			case BezierType.Test:
				return 2;
			}
			return 0;
		}

		private int GetBezierSettingIndex(Vector3 worldPos) {
			if (worldPos.y > 1) {
				if (worldPos.x < -4.8) {
					return 4;
				}
				else if (worldPos.x < 4.8) {
					return 3;
				}
				else {
					return 2;
				}
			}
			else {
				if (worldPos.x < -4.8) {
					return 5;
				}
				else if (worldPos.x < 4.8) {
					return 0;
				}
				else {
					return 1;
				}
			}
		}

		// 在这里设定曲线中间点的相对坐标
		private static void BezierMiddlePointSettings() {
			if (bezierMiddlePoints == null) {
				bezierMiddlePoints = new List<List<Vector3>>();
			}

			// 路径设定
			bezierMiddlePoints.Add(new List<Vector3>());
			if (OnLineEarningMgr.Instance.isThreeHundredOpen())
			{
				bezierMiddlePoints[0].Add(new Vector3(-1.5f, 0.4f, 0f));
				bezierMiddlePoints[0].Add(new Vector3(1f, 0.7f, 0f));
			}
			else
			{
				bezierMiddlePoints[0].Add(new Vector3(-5f, 0.4f, 0f));
				bezierMiddlePoints[0].Add(new Vector3(5f, 0.7f, 0f));
			}

			bezierMiddlePoints.Add(new List<Vector3>());
			bezierMiddlePoints[1].Add(new Vector3(-1.2f, 0.299487f, 0f));
			bezierMiddlePoints[1].Add(new Vector3(1.04149f, 0.528205f, 0f));

			bezierMiddlePoints.Add(new List<Vector3>());
			bezierMiddlePoints[2].Add(new Vector3(0.64f, -0.38f, 0f));
//
			bezierMiddlePoints.Add(new List<Vector3>());
//			bezierMiddlePoints[3].Add(new Vector3(f, f, 0f));
//			bezierMiddlePoints[3].Add(new Vector3(f, f, 0f));
//
			bezierMiddlePoints.Add(new List<Vector3>());
//			bezierMiddlePoints[4].Add(new Vector3(f, f, 0f));
//			bezierMiddlePoints[4].Add(new Vector3(f, f, 0f));
//
			bezierMiddlePoints.Add(new List<Vector3>());
//			bezierMiddlePoints[5].Add(new Vector3(1f, 1f, 0f));
//			bezierMiddlePoints[5].Add(new Vector3(1f, 1f, 0f));

            bezierMiddlePoints.Add(new List<Vector3>());
            bezierMiddlePoints[6].Add(new Vector3(0.14f, -3.08f, 0f));
		}

		// 在这里设定曲线中间点的随机相对偏移量最大值
		private static void BezierMiddlePointOffsetSettings() {
			if (bezierMiddlePointsOffset == null) {
				bezierMiddlePointsOffset = new List<float>();
			}

			bezierMiddlePointsOffset.Add(0.04f);
			bezierMiddlePointsOffset.Add(0.04f);
			bezierMiddlePointsOffset.Add(0.42f);
			bezierMiddlePointsOffset.Add(0f);
            bezierMiddlePointsOffset.Add(0f);
            bezierMiddlePointsOffset.Add(0.04f);
            bezierMiddlePointsOffset.Add(0.04f);
		}

		// 在这里设定物体在何时运动到曲线的何处
		private static void TimeScaleChangeSettings() {
			if (timeScaleChangePolylines == null) {
				timeScaleChangePolylines = new List<List<Vector2>>();
			}

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[0].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[0].Add(new Vector2(0.5f, 0.4f));
			timeScaleChangePolylines[0].Add(new Vector2(0.6f, 0.5f));
			timeScaleChangePolylines[0].Add(new Vector2(0.9f, 0.8f));
			timeScaleChangePolylines[0].Add(new Vector2(1.0f, 1.0f));

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[1].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[1].Add(new Vector2(0.5f, 0.4f));
			timeScaleChangePolylines[1].Add(new Vector2(0.8f, 0.5f));
			timeScaleChangePolylines[1].Add(new Vector2(0.9f, 0.65f));
			timeScaleChangePolylines[1].Add(new Vector2(0.95f, 0.8f));
			timeScaleChangePolylines[1].Add(new Vector2(1.0f, 1.0f));

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[2].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[2].Add(new Vector2(1.0f, 1.0f));

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[3].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[3].Add(new Vector2(0.55f, 0.22f));
			timeScaleChangePolylines[3].Add(new Vector2(1.0f, 1.0f));

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[4].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[4].Add(new Vector2(0.55f, 0.22f));
			timeScaleChangePolylines[4].Add(new Vector2(1.0f, 1.0f));

			timeScaleChangePolylines.Add(new List<Vector2>());
			timeScaleChangePolylines[5].Add(new Vector2(0.0f, 0.0f));
			timeScaleChangePolylines[5].Add(new Vector2(0.55f, 0.22f));
            timeScaleChangePolylines[5].Add(new Vector2(1.0f, 1.0f));

            timeScaleChangePolylines.Add(new List<Vector2>());
            timeScaleChangePolylines[6].Add(new Vector2(0.0f, 0.0f));
            timeScaleChangePolylines[6].Add(new Vector2(1.0f, 1.0f));
		}

		// 在这里设定物体在运动时的大小变化
		private static void SizeScaleChangeSettings() {
			if (sizeScaleChangePolylines == null) {
				sizeScaleChangePolylines = new List<List<Vector2>>();
			}

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[0].Add(new Vector2(0.0f, 1f));
			sizeScaleChangePolylines[0].Add(new Vector2(0.2f, 1.2f));
			sizeScaleChangePolylines[0].Add(new Vector2(0.5f, 1.5f));
			sizeScaleChangePolylines[0].Add(new Vector2(0.7f, 1.0f));
			sizeScaleChangePolylines[0].Add(new Vector2(1.0f, 0.8f));

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[1].Add(new Vector2(0.0f, 0.5f));
			sizeScaleChangePolylines[1].Add(new Vector2(0.5f, 1.0f));
			sizeScaleChangePolylines[1].Add(new Vector2(0.8f, 1.0f));
			sizeScaleChangePolylines[1].Add(new Vector2(1.0f, 0.5f));

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[2].Add(new Vector2(0.0f, 0.8f));
//			sizeScaleChangePolylines[2].Add(new Vector2(0.5f, 1.0f));
//			sizeScaleChangePolylines[2].Add(new Vector2(0.8f, 1.0f));
			sizeScaleChangePolylines[2].Add(new Vector2(1.0f, 0.5f));

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[3].Add(new Vector2(0.0f, 1.0f));
			sizeScaleChangePolylines[3].Add(new Vector2(0.8f, 1.0f));
//			sizeScaleChangePolylines[3].Add(new Vector2(0.8f, 1.0f));
			sizeScaleChangePolylines[3].Add(new Vector2(1.0f, 0.5f));

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[4].Add(new Vector2(0.0f, 0.5f));
			sizeScaleChangePolylines[4].Add(new Vector2(0.5f, 1.0f));
			sizeScaleChangePolylines[4].Add(new Vector2(0.8f, 1.0f));
			sizeScaleChangePolylines[4].Add(new Vector2(1.0f, 0.5f));

			sizeScaleChangePolylines.Add(new List<Vector2>());
			sizeScaleChangePolylines[5].Add(new Vector2(0.0f, 0.5f));
			sizeScaleChangePolylines[5].Add(new Vector2(0.5f, 1.0f));
			sizeScaleChangePolylines[5].Add(new Vector2(0.8f, 1.0f));
			sizeScaleChangePolylines[5].Add(new Vector2(1.0f, 0.5f));


            sizeScaleChangePolylines.Add(new List<Vector2>());
            sizeScaleChangePolylines[6].Add(new Vector2(0.0f, 0.8f));
            sizeScaleChangePolylines[6].Add(new Vector2(1.0f, 0.5f));
		}

		// 在这里设定物体做曲线运动的时间范围
		private static void TimeDurationSettings() {
			if (timeDuration == null) {
				timeDuration = new List<Vector2>();
			}

			timeDuration.Add(new Vector2(1f, 1f));
			timeDuration.Add(new Vector2(3f, 3f));
			timeDuration.Add(new Vector2(2f, 2f));
			timeDuration.Add(new Vector2(1.8f, 2.3f));
            timeDuration.Add(new Vector2(1.8f, 2.3f));
            timeDuration.Add(new Vector2(1.8f, 2.3f));
            timeDuration.Add(new Vector2(1.8f, 2.3f));
		}

		// 在这里设定物体出现的出现时间
		private static void PopDurationSettings() {
			if (popDuration == null) {
				popDuration = new List<float>();
			}

			popDuration.Add(0.75f);
			popDuration.Add(1.5f);
			popDuration.Add(0.75f);
			popDuration.Add(1.1f);
            popDuration.Add(1.1f);
            popDuration.Add(1.1f);
            popDuration.Add(1.1f);
		}
	}
}
