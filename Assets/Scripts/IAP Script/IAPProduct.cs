using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MyPurchaseID
{
    public const string Pack1 = "com.tilematch.pack1";
    public const string Pack2 = "com.tilematch.pack2";
    public const string Pack3 = "com.tilematch.pack3";
    public const string Pack4 = "com.tilematch.pack4";
    public const string Pack5 = "com.tilematch.pack";
    public const string Pack6 = "com.tilematch.pack6";
    public const string Pack7 = "com.tilematch.pack7";
    public const string Pack8 = "com.tilematch.pack8";
    public const string Pack9 = "com.tilematch.pack9";
}

public class IAPProduct : MonoBehaviour
{
    [SerializeField] private string _purchaseID;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _discount;
    [SerializeField] private Sprite _icon;

    public string PurchaseID => _purchaseID;

    public delegate void PurchaseEvent(Product Model, Action OnComplete);

    public event PurchaseEvent OnPurchase;
    private Product _model;
    UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        RegisterPurchase();
        RegisterEventButton();
    }

    protected virtual void RegisterPurchase()
    {
        StartCoroutine(IAPManager.Instance.CreateHandleProduct(this));
    }

    public void Setup(Product product, string code, string price)
    {
        _model = product;
        if (_price != null)
        {
            _price.text = price;
        }

        if (_discount != null)
        {
            if (code.Equals("VND"))
            {
                var round = Mathf.Round(float.Parse(price) + float.Parse(price) * .4f);
                _discount.text = code + " " + round;
            }
            else
            {
                var priceFormat = $"{float.Parse(price) + float.Parse(price) * .4f:0.00}";
                _discount.text = code + " " + priceFormat;
            }
        }
    }

    private void RegisterEventButton()
    {
        _purchaseButton.onClick.AddListener(() => { Purchase(); });
    }

    private void Purchase()
    {
        OnPurchase?.Invoke(_model, HandlePurchaseComplete);
    }

    private void HandlePurchaseComplete()
    {
        switch (_purchaseID)
        {
            // case MyPurchaseID.RemoveAds:
            //     RemoveAdsPack();
            //     break;
            case MyPurchaseID.Pack1:
                uiManager.BuyTicket(100);
                break;
            case MyPurchaseID.Pack2:
                uiManager.BuyTicket(300);
                break;
            case MyPurchaseID.Pack3:
                uiManager.BuyTicket(600);
                break;
            case MyPurchaseID.Pack4:
                uiManager.BuyTicket(800);
                break;
            case MyPurchaseID.Pack5:
                uiManager.BuyTicket(1200);
                break;
            case MyPurchaseID.Pack6:
                uiManager.BuyTicket(1800);
                break;
            case MyPurchaseID.Pack7:
                uiManager.BuyTicket(2200);
                break;
            case MyPurchaseID.Pack8:
                uiManager.BuyTicket(3000);
                break;
            case MyPurchaseID.Pack9:
                uiManager.BuyTicket(5000);
                break;
        }

        if (_icon != null)
        {
            _purchaseButton.gameObject.GetComponent<Image>().sprite = _icon;
            _purchaseButton.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            _purchaseButton.interactable = false;
        }
    }
}