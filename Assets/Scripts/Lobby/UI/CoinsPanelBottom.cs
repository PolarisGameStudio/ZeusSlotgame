using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Libs;
using UnityEngine.SceneManagement;

namespace Classic
{
	public class CoinsPanelBottom : CoinsPanel
	{
		public override void CaculateTxt()
		{
			if (coinText == null) 
			{
				if (tweener != null) 
				{
					tweener.Kill ();
				}
				return;
			}
			if(initNum ==0)
			{
				coinText.text = "0";
			}else 
			{
				coinText.text = string.Format("<sprite name=\"coin\"> {0}",Utils.Utilities.GetBigNumberShow(initNum));
			}
		}
    }
}
