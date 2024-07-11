using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeElementController : MonoBehaviour
{
    [SerializeField] private Button thisButton = null;
    [SerializeField] private Image thisImage = null;
    [SerializeField] private Sprite aImage = null;
    [SerializeField] private Sprite bImage = null;

    private Action<int> callBack = null;
    private int index;

    private float animationDuration = 0.5f; // 애니메이션 지속 시간

    private void Awake()
    {
        thisButton.onClick.AddListener(OnClickButton);
    }

    public void SetButton(Action<int> _callBack)
    {
        callBack = null;
        callBack = _callBack;
    }

    public async UniTask ChangeTicTacToeElement(int _select)
    {
        switch (_select)
        {
            case 0:
                thisImage.sprite = aImage;
                thisImage.color = Color.white;
                thisButton.enabled = false;
                thisImage.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBounce);
                thisImage.DOFade(1, animationDuration);
                break;
            case 1:
                thisImage.sprite = bImage;
                thisImage.color = Color.white;
                thisButton.enabled = false;
                thisImage.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBounce);
                thisImage.DOFade(1, animationDuration);
                break;
            default:
                thisImage.color = Color.clear;
                thisButton.enabled = true;
                thisImage.transform.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack);
                thisImage.DOFade(0, animationDuration);
                break;
        }

        await UniTask.Delay((int)(animationDuration * 1000));
    }

    public void ResetTicTacToeElement()
    {
        thisImage.sprite = aImage;
        thisImage.color = Color.white;
        thisButton.enabled = false;

        thisImage.transform.localScale = Vector3.zero;
    }

    private void OnClickButton()
    {
        callBack?.Invoke(index);
    }

    public Button GetButton { get { return thisButton; } }
    public int SetIndex { get { return index; } set { index = value; } }
    public Sprite SetSprite { set { thisImage.sprite = value; } }
    public Image SetImage { get { return thisImage; } set { thisImage = value; } }
}
