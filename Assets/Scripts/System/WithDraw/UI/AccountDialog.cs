using System;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.U2D;

namespace Classic
{
    public class AccountDialog:UIDialog
    {
        public Image image;
        public TMP_InputField inputField;
        public Button ensureBtn;
        private RedeemItemData _redeemItemData;
        private int platformIndex;
        private int money;
        public GameObject notice;
        private Tweener tween;
        protected override void Awake()
        {
            base.Awake();
            UGUIEventListener.Get(ensureBtn.gameObject).onClick = EnsureBtnClick;
            WithDrawManager.Instance.ResetShowAccountTag();
        }

        public void SetUIData(RedeemItemData data)
        {
            _redeemItemData = data;
            platformIndex = _redeemItemData.platSpIndex;
            money = (int)_redeemItemData.RewardCash;
        }

        public override void Refresh()
        {
            base.Refresh();
            AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
            {
                if (result != null)
                {
                    Sprite sp = result.GetSprite(platformIndex.ToString());
                    if (sp != null)
                    {
                        image.sprite = sp;
                    }
                }
            });
        }
        
        private void EnsureBtnClick(GameObject go)
        {
            if (CheckAccountValid())
            {
                this.Close();
                WithDrawManager.Instance.ShowAccountEnsureDialog(inputField.text,_redeemItemData);
            }
            else
            {
                if (tween!=null)
                {
                    tween.Kill();
                    tween = null;
                }
                notice.gameObject.SetActive(true);
                notice.transform.localPosition = new Vector3(0, -100f, 0);
                tween = notice.transform.DOLocalMoveY(0f, 2f)
                    .OnComplete(() => {
                        if (notice!=null)
                        {
                            notice.gameObject.SetActive(false);
                            notice.transform.localPosition = new Vector3(0, -100f, 0);
                        }
                        tween.Kill();
                        tween = null;
                    });
            }
        }

        private bool CheckAccountValid()
        {
            string account = inputField.text;
            //googlepay 和 paypal校验手机号
            if (platformIndex==6 || platformIndex == 8)
            {
                //校验邮箱
                return IsEmail(account);
            }
            //校验手机号
            return IsValidNumberString(account);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (tween!=null)
            {
                tween.Kill();
            }
            tween = null;
        }


        public bool IsEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // 邮箱格式正则表达式
            string pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(email);
        }
        public bool IsValidNumberString(string input)
        {
            // ^\d{6,}$ ：数字且长度至少6位
            return Regex.IsMatch(input, @"^\d{6,}$");
        }
    }
}