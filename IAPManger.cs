using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

// Unity 내부에서 IAP 매니저를 활성화 시켜야한다.
public class IAPManger : MonoBehaviour, IStoreListener
{
    // 구매 버튼에서 스크립트를 불러오기 위한 싱글턴
    private static IAPManger _instance;
    public static IAPManger Instance
    {
        get
        {
            // 인스턴스가 없는 경우에 접근하려 하면 인스턴스를 할당해준다.
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(IAPManger)) as IAPManger;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    // 아이템 ID : 등록 후 구글 스토어에도 같은 아이디를 등록해줘야한다.
    private string noAd = "com.ggulnimstudio.colorjump.noad";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        // 인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제한다.
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        // 아래의 함수를 사용하여 씬이 전환되더라도 선언되었던 인스턴스가 파괴되지 않는다.
        DontDestroyOnLoad(gameObject);

        if (storeController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }

    public void Start()
    {
        // IAP매니저가 활성화 될때까지 계속해서 호출해준다.
        StartCoroutine(IAPReady());
    }

    IEnumerator IAPReady()
    {
        while (!IsInitialized())
        {
            yield return null;
        }
        if (HadPurchased(noAd))
        {
            GameManager.Instance.noAd = true;
        }
        else
        {
            Ad.Instance.ShowBannerAd();
        }

    }
    public void InitializePurchasing()
    {
        if(IsInitialized())
        {
            return;
        }
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(noAd, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return storeController != null && extensionProvider != null;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        storeController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        extensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // Purchase 함수를 실행시 아이템 아이디를 비교하고  구매 성공시 해당 이벤트를 실행시킨다.
        if(args.purchasedProduct.definition.id == noAd)
        {
            Debug.Log("NoAd 구매 성공");
            GameManager.Instance.noAd = true;
            Ad.Instance.HideBannerAd();
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

    // 구매버튼에서 이 함수를 호출 해주어야한다.
    public void Purchase(string productId)
    {
        if (!IsInitialized()) return;

        var product = storeController.products.WithID(productId);

        if(product != null && product.availableToPurchase && !HadPurchased(productId))
        {
            storeController.InitiatePurchase(product);
        }
    }

    // 구매기록이 있는지를 확인하고 영수증을 결과를 반환한다.
    public bool HadPurchased(string productId)
    {
        if (!IsInitialized()) return false;

        var product = storeController.products.WithID(productId);

        if(product != null)
        {
            return product.hasReceipt;
        }
        return false;
    }

}

