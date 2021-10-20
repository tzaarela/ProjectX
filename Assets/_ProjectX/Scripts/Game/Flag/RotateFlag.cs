using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class RotateFlag : MonoBehaviour
{
    private Tweener tweener;

    private void OnEnable()
    {
        tweener = transform.DORotate(new Vector3(0, 180, 0), 2f, RotateMode.Fast).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }

	private void OnDisable()
	{
        if (tweener != null)
            tweener.Kill();
	}
}
