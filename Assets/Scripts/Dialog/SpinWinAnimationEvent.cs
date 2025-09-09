using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinWinAnimationEvent : MonoBehaviour
{

    public int index = 0;

	public List<GameObject> baoEffect;

    public List<GameObject> winEffect;


	public void OpenBigWinEffect()
	{
		if(index == 0 ) winEffect[index].SetActive(true);
	}

	public void OpenWinEffect()
	{
		if(index == 0 ) return;
		if(index < winEffect.Count) winEffect[index].SetActive(true);
	}

	public void OpenBaoEffect()
	{
		// baoEffect[index].SetActive(true);
	}


}
