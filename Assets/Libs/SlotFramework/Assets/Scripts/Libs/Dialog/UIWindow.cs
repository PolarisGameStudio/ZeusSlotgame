using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using DG.Tweening;


namespace Libs
{
	public enum UIAnimation
	{
		NOAnimation,
		Scale,
		Left,
		Custom,
		Right,
		Top,
		Bottom,
		Animator
	}

	public class UIWindow : UIBase
	{
		const string k_OpenTransitionName = "Open";
		const string k_ClosedStateName = "Closed";
		const string k_NormalStateName = "Normal";
		const string k_OpenEndStateName = "OpenEnd";
		const string k_ClosedEndStateName = "ClosedEnd";
		const int layer_Index = 0;
		Animator animator;
		public bool DestoryOnQuit = false;
		public UIAnimation UIAnimationIn = UIAnimation.Scale;
		public ActionCurve AniDurationIn = ActionCurve.EaseOutBack;
		public float AppearTime = 0.3f;
		public UIAnimation UIAnimationOut = UIAnimation.Scale;
		public ActionCurve AniDurationOut = ActionCurve.EaseInBack;
		public float DisappearTime = 0.3f;
		public bool AutoQuit = false;
		public float DisplayTime = 2;

		public bool ShowOutOnce = false;
//		public bool EnableAnimation = true;
		[HideInInspector] public MaskUI maskUI;
		protected override void Awake ()
		{
			base.Awake ();
			Init ();

			UnScaleTimeAnimator ();
			UnScaleTimeParticle ();
		}
		protected override void Start ()
		{
			try
			{
				Refresh ();
				ShowIn ();
			}
			catch (Exception e)
			{
				ExceptionHandle(e);
			}
			
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EndCB();
			/*if (inTweener != null) //防止当Start时调用了销毁，启动了In动画，且在执行中，销毁时，调用当前对象销毁
			{
				inTweener.Kill();
				
			}*/
			DOTween.Kill(inTweener);
			inTweener = null;
			/*if (outTweener != null) ////防止当Destroy时调用了销毁，启动了Out动画，且在执行中，销毁时，调用当前对象销毁
			{
				outTweener.Kill();
				outTweener = null;
			}*/
			DOTween.Kill(outTweener);
			outTweener = null;
			
			/*if (tickUntilQuitDA != null)
			{
				tickUntilQuitDA.Kill();
			}*/
			
			DOTween.Kill(tickUntilQuitDA);
			tickUntilQuitDA = null;

		}
		private void EndCB()
		{
			if (CompleteQuitCallBack!=null){
				CompleteQuitCallBack();
				CompleteQuitCallBack = null;
			}
		}

		private void ExceptionHandle(Exception e)
		{
			EndCB();
#if DEBUG || UNITY_EDITOR
			Utils.Utilities.LogError($"{CurPrefabName} SetData Has Exception! \n{e.Message}");
#endif
			BaseGameConsole.ActiveGameConsole().SendCatchExceptionToServer(e);
		}
		public override void Refresh(){
		}
		public override void Init ()
		{
			animator = gameObject.GetComponent<Animator> ();

			initPosition = this.transform.localPosition;
			OutAction = new ActionCallBack ();
			InAction = new ActionCallBack ();
			OutAction.SetCompleteMethod (EndCB);

			InAction.AddCompleteMethod (TickUntilQuit);

		}

		public float GetTotalDisplayTime ()
		{
			return AppearTime + DisappearTime + DisplayTime;
		}

		private DelayAction tickUntilQuitDA;

		private void TickUntilQuit ()
		{
			if (AutoQuit) {
				tickUntilQuitDA = ActionAnimator.delayTo (DisplayTime, ShowOut);
				tickUntilQuitDA.Play ();
			}
		}

		public void StopAutoQuit(){
			if (tickUntilQuitDA!=null) {
				tickUntilQuitDA.Kill ();
			}
		}

		public virtual void ShowIn ()
		{
			//if(maskUI!=null) maskUI.FadeIn();//将实现移动到了maskUI内
			//在执行到这时，可能对话框已经被销毁
			if (this!=null&&gameObject!=null)
			{
				gameObject.SetActive (true);
				this.transform.localPosition = initPosition;
				showInAnimation ();
			}
		
		}

		private Tweener inTweener;
		private void showInAnimation ()
		{

			switch (UIAnimationIn) {
			case UIAnimation.Scale:
				scaleIn ();
				break;
			case UIAnimation.Bottom:
				bottomIn ();
				break;
			case UIAnimation.Top:
				topIn ();
				break;
			case UIAnimation.Left:
				leftIn ();
				break;
			case UIAnimation.Right:
				rightIn ();
				break;
			case UIAnimation.Custom:
				customIn ();
				break;
			case UIAnimation.Animator:
				animatorIn ();
				break;
			default:
				noAnimationIn ();
				break;
			}
		}
		
		
		private void noAnimationIn ()
		{
			if (InAction?.OnCompleteMethod != null) {
				InAction.OnCompleteMethod ();
			}
		}

		private void scaleIn ()
		{
			RectTransform rectTransform = transform as RectTransform;
			inTweener = ActionAnimator.scaleFrom (gameObject, AppearTime, new Vector3(0.4f,.4f,1f), AniDurationIn, InAction).SetUpdate(true);
		}


		private void bottomIn ()
		{
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localScale = Vector3.one;
			inTweener = ActionAnimator.moveFrom (gameObject, AppearTime, new Vector3 (initPosition.x, -Screen.height, 0), true, AniDurationIn, InAction).SetUpdate(true);
		}

		private void topIn ()
		{
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localScale = Vector3.one;
			inTweener = ActionAnimator.moveFrom (gameObject, AppearTime, new Vector3 (initPosition.x, Screen.height, 0), true, AniDurationIn, InAction).SetUpdate(true);
		}

		private void leftIn ()
		{
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localScale = Vector3.one;
			inTweener = ActionAnimator.moveFrom (gameObject, AppearTime, new Vector3 (-Screen.width, initPosition.y, 0), true, AniDurationIn, InAction).SetUpdate(true);
		}

		private void rightIn ()
		{
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.localScale = Vector3.one;
			inTweener = ActionAnimator.moveFrom (gameObject, AppearTime, new Vector3 (Screen.width, initPosition.y, 0), true, AniDurationIn, InAction).SetUpdate(true);
		}

		private  void animatorIn ()
		{
			if (animator != null) {
				animator.updateMode = AnimatorUpdateMode.UnscaledTime;
				animator.speed = 1f / AppearTime;
				animator.Play (k_OpenTransitionName);
//				StartCoroutine (Open (animator));
				InAction?.OnCompleteMethod ();
			}
			InAction?.OnCompleteMethod ();
		}
	
		IEnumerator Open (Animator anim)
		{
			if (anim != null) {
				bool closedStateReached = anim.GetCurrentAnimatorStateInfo (layer_Index).IsName (k_OpenEndStateName);
				while (!closedStateReached) {
					if (!anim.IsInTransition (layer_Index))
						closedStateReached = anim.GetCurrentAnimatorStateInfo (layer_Index).IsName (k_OpenEndStateName);
					yield return new WaitForEndOfFrame ();
				}
			}
			InAction?.OnCompleteMethod ();
		}

		protected virtual void customIn ()
		{
		}

		public System.Action onKeyback = null;

		public virtual void ShowOut ()
		{
			if (ShowOutOnce) return;
			ShowOutOnce = true;
			//当前对象存在时，执行，否则直接跳过
			if (this!=null&&gameObject!=null)
			{
				if(maskUI!=null) maskUI.FadeOut();
				setQuitAction ();
				shwoOutAnimation ();
			}
			
		}

		private void setQuitAction ()
		{
			if (DestoryOnQuit) {
				OutAction?.AddCompleteMethod (() => { 
					if (this!=null&&gameObject != null)
						Destroy (gameObject);
				}
				);
			} else {
				OutAction?.AddCompleteMethod (() => {
					try {
						if (this!=null&&gameObject != null) {
							gameObject.SetActive(false);
						}
					}catch(Exception e){
						ExceptionHandle(e);
					}
				}
				);
			}
		}

		private void shwoOutAnimation ()
		{
//			if (!EnableAnimation) {
//				noAnimationOut();
//				return ;
//			}				

			switch (UIAnimationOut) {
			case UIAnimation.Scale:
				scaleOut ();
				break;
			case UIAnimation.Bottom:
				bottomOut ();
				break;
			case UIAnimation.Top:
				topOut ();
				break;
			case UIAnimation.Left:
				leftOut ();
				break;
			case UIAnimation.Right:
				rightOut ();
				break;
			case UIAnimation.Custom:
				customOut ();
				break;
			case UIAnimation.Animator:
				animatorOut ();
				break;
			default:
				noAnimationOut ();
				break;
			}

		}

		private void noAnimationOut ()
		{
			try {
				if (OutAction?.OnCompleteMethod != null) {
					OutAction.OnCompleteMethod ();
				}
			} catch (Exception e) {
				ExceptionHandle(e);
			}
		}

		private Tweener outTweener;

		private void scaleOut ()
		{
			outTweener = ActionAnimator.scaleTo (gameObject, DisappearTime, new Vector3(0.5f,0.5f,1f), AniDurationOut, OutAction).SetUpdate(true);
		}

		private void bottomOut ()
		{
			outTweener = ActionAnimator.moveTo (gameObject, DisappearTime, new Vector3 (initPosition.x, -Screen.height, 0), true, AniDurationOut, OutAction).SetUpdate(true);
		}

		private void topOut ()
		{
			outTweener = ActionAnimator.moveTo (gameObject, DisappearTime, new Vector3 (initPosition.x, 1.5f*Screen.height, 0), true, AniDurationOut, OutAction).SetUpdate(true);
		}

		private void leftOut ()
		{
			outTweener = ActionAnimator.moveTo (gameObject, DisappearTime, new Vector3 (-Screen.width, initPosition.y, 0), true, AniDurationOut, OutAction).SetUpdate(true);
		}
	
		private void rightOut ()
		{
			outTweener = ActionAnimator.moveTo (gameObject, DisappearTime, new Vector3 (Screen.width, initPosition.y, 0), true, AniDurationOut, OutAction).SetUpdate(true);
		}

		protected virtual void customOut ()
		{
		}

		private void animatorOut ()
		{
			if (animator != null) {
				animator.updateMode = AnimatorUpdateMode.UnscaledTime;
				animator.speed = 1f / DisappearTime;
				animator.Play (k_ClosedStateName);
				StartCoroutine (Closed (animator));
			}
		}
	
		IEnumerator Closed (Animator anim)
		{
			if (anim != null) {
				bool closedStateReached = anim.GetCurrentAnimatorStateInfo (layer_Index).IsName (k_ClosedEndStateName);
				while (!closedStateReached) {
					if (!anim.IsInTransition (layer_Index))
						closedStateReached = anim.GetCurrentAnimatorStateInfo (layer_Index).IsName (k_ClosedEndStateName);
					yield return new WaitForEndOfFrame ();
				}
			}
			OutAction?.OnCompleteMethod ();
		}

		//忽略时间的速度
		private void UnScaleTimeAnimator()
		{
			Animator[] animators = this.gameObject.GetComponentsInChildren<Animator> (true);
			if (animators != null) {
				foreach (Animator ani in animators) {
					ani.updateMode = AnimatorUpdateMode.UnscaledTime;
				}
			}
		}

		//particle的时间控制
		private void UnScaleTimeParticle()
		{
			ParticleSystem[] particles = this.gameObject.GetComponentsInChildren<ParticleSystem> (true);
			if (particles != null) {
				foreach (ParticleSystem ps in particles) {
					ParticleSystem.MainModule mainModule = ps.main;
                    mainModule.useUnscaledTime = true;
				}
			}
		}

		private ActionCallBack InAction = null;
		private ActionCallBack OutAction = null;

		public System.Action CompleteQuitCallBack = null;
		private Vector3 initPosition;
	}
}
