using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeElementController : MonoBehaviour
{
    [SerializeField] private Button thisButton = null;
    [SerializeField] private Image thisImage = null;
    [SerializeField] private Sprite nonImage = null;
    [SerializeField] private Sprite aImage = null;
    [SerializeField] private Sprite bImage = null;

    private Action<int> callBack = null;
    private int index;

    private void Awake()
    {
        thisButton.onClick.AddListener(OnClickButton);
    }

    public void SetButton(Action<int> _callBack)
    {
        callBack = null;
        callBack = _callBack;
    }

    public void ChangeTicTacToeElement(int _select)
    {
        switch (_select)
        {
            case 0:
                thisImage.sprite = aImage;
                thisImage.color = Color.white;
                thisButton.enabled = false;
                break;
            case 1:
                thisImage.sprite = bImage;
                thisImage.color = Color.white;
                thisButton.enabled = false;
                break;
            default:
                thisImage.color = Color.clear;
                thisButton.enabled = true;
                break;
        }
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
