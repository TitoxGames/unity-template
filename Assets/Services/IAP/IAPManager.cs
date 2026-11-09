using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TitoX.Core;

namespace Tito.Services.IAP
{
    [DefaultExecutionOrder(-100)]
    public class IAPManager: Singleton<IAPManager>
    {
        [Header("IAP Providers")] 
        [SerializeField]
        private IAPProvider provider;
        
        [SerializeField] private IAPCatalog catalog;
        
        [SerializeField] private bool initializeOnStart;
        
        public UnityEvent OnInitialized;
        public UnityEvent OnInitializationFailed;

        private void Start()
        {
            if (initializeOnStart)
            {
                provider.Initialize(catalog);
            }
            
            provider.Initialized += OnProviderInitialized;
            provider.InitializeFailed += OnProviderInitializeFailed;
        }

        private void OnProviderInitialized()
        {
            OnInitialized?.Invoke();
        }

        private void OnProviderInitializeFailed()
        {
            OnInitializationFailed?.Invoke();
        }
        
        public UniTask<PurchaseResult> Purchase(string productId)
        {
            return provider.Purchase(productId);
        }

        public UniTask RestorePurchases()
        {
            return provider.RestorePurchases();
        }

        public bool IsPurchased(string productId)
        {
            return provider.IsPurchased(productId);
        }

        public string GetLocalizedPrice(string productId)
        {
            return provider.GetLocalizedPrice(productId);
        }
        
        public decimal GetPrice(string productId)
        {
            return provider.GetPrice(productId);
        }

        public string GetCurrencyCode(string productId)
        {
            return provider.GetCurrencyCode(productId);
        }
    }
}