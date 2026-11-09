﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TitoX.Core.EventBus;
using Tito.Services.IAP.Events;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Tito.Services.IAP.Provider
{
    [CreateAssetMenu(fileName = "UnityIAPProvider", menuName = "TitoX/IAP/UnityIAPProvider")]
    public class UnityIAPProvider : IAPProvider
    {
        public override Action Initialized { get; set; }
        public override Action InitializeFailed { get; set; }

        private StoreController m_StoreController;
        private bool m_IsPurchaseInProgress;
        private bool m_AwaitingRestorePurchasesFetch;
        private string m_PendingRestoreMessage;
        private readonly HashSet<string> m_ProcessedTransactionIds = new HashSet<string>();

        public override async UniTask Initialize(IAPCatalog catalog)
        {
            var catalogProvider = new CatalogProvider();
            foreach (var product in catalog.Products)
            {
                if (product.Enabled)
                    catalogProvider.AddProduct(product.Id, Convert(product.Type));
            }

            m_StoreController = UnityIAPServices.StoreController();

            m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
            m_StoreController.OnStoreConnected += OnStoreConnected;
            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;
            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnPurchaseDeferred += OnPurchaseDeferred;

            await m_StoreController.Connect();
            catalogProvider.FetchProducts(list => m_StoreController.FetchProducts(list));
            Debug.Log("UnityIAPProvider: Initialized");
            IsInitialized = true;
            Initialized?.Invoke();
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            Debug.Log($"Purchase deferred: {order.Info}");
        }

        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            Debug.Log($"Purchase pending for product: {pendingOrder.Info}");

            foreach (var product in pendingOrder.CartOrdered.Items())
                Debug.Log($"Pending product: {product.CatalogListingId}, quantity: {product.Quantity}");

            ProcessPurchase(pendingOrder);
        }

        private void ProcessPurchase(PendingOrder pendingOrder)
        {
            if (pendingOrder == null || m_StoreController == null)
            {
                m_IsPurchaseInProgress = false;
                return;
            }

            var product = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product == null)
            {
                Debug.LogError("Purchase failed: Product is null.");
                m_IsPurchaseInProgress = false;
                return;
            }

            var transactionId = pendingOrder.Info?.TransactionID;
            if (!string.IsNullOrEmpty(transactionId) &&
                m_ProcessedTransactionIds.Contains(transactionId))
            {
                // Client already granted; still confirm so Google/Apple do not keep it pending.
                Debug.Log($"Confirming already-processed transaction: {transactionId}");
                m_StoreController.ConfirmPurchase(pendingOrder);
                m_IsPurchaseInProgress = false;
                return;
            }

            if (string.IsNullOrEmpty(pendingOrder.Info?.Receipt))
                Debug.LogWarning("Purchase receipt is empty; confirming pending order anyway.");

            m_StoreController.ConfirmPurchase(pendingOrder);
        }

        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            m_IsPurchaseInProgress = false;

            var cartItem = failedOrder?.CartOrdered?.Items()?.FirstOrDefault();
            var product = cartItem?.Product;
            var productId = product?.definition?.catalogListingId ?? cartItem?.CatalogListingId;
            var reason = failedOrder != null
                ? failedOrder.FailureReason
                : PurchaseFailureReason.Unknown;

            Debug.Log($"Purchase failed for product: {failedOrder?.Info}, reason: {reason}");

            if (!string.IsNullOrEmpty(productId) &&
                IsRecoverableOwnershipFailure(reason) &&
                IsNonConsumableProduct(product))
            {
                if (TryConfirmPendingForProduct(productId))
                {
                    Debug.Log(
                        $"UnityIAPProvider: Confirmed pending order after {reason} for {productId}");
                    return;
                }

                Debug.Log($"UnityIAPProvider: Treating {reason} as owned restore for {productId}");
                EventBus<PurchaseFailedEvent>.Post(new PurchaseFailedEvent(
                    productId,
                    PurchaseStatus.Failed,
                    reason.ToString()));
                return;
            }

            EventBus<PurchaseFailedEvent>.Post(new PurchaseFailedEvent(
                productId ?? "unknown",
                PurchaseStatus.Failed,
                reason.ToString()));
        }

        private static bool IsRecoverableOwnershipFailure(PurchaseFailureReason reason)
        {
            return reason == PurchaseFailureReason.DuplicateTransaction ||
                   reason == PurchaseFailureReason.ExistingPurchasePending;
        }

        private static bool IsNonConsumableProduct(Product product)
        {
            if (product == null)
                return true;

            var type = product.definition != null ? product.definition.type : product.type;
            return type != UnityEngine.Purchasing.ProductType.Consumable;
        }

        private bool TryConfirmPendingForProduct(string productId)
        {
            if (m_StoreController == null || string.IsNullOrEmpty(productId))
                return false;

            foreach (var order in m_StoreController.GetPurchases())
            {
                if (order is not PendingOrder pending)
                    continue;

                var item = pending.CartOrdered.Items().FirstOrDefault();
                if (item == null)
                    continue;

                var listingId = item.CatalogListingId ?? item.Product?.definition?.catalogListingId;
                if (!string.Equals(listingId, productId, StringComparison.Ordinal))
                    continue;

                ProcessPurchase(pending);
                return true;
            }

            return false;
        }

        private void OnStoreConnected()
        {
            Debug.Log("UnityIAPProvider: Store connected");
            m_IsPurchaseInProgress = false;
        }

        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failedOrder)
            {
                Debug.LogError(
                    $"Purchase failed for product: {failedOrder.Info}, reason: {failedOrder.FailureReason}");
                m_IsPurchaseInProgress = false;
                
                EventBus<PurchaseFailedEvent>.Post(new PurchaseFailedEvent(
                    failedOrder.CartOrdered?.Items().FirstOrDefault()?.Product?.definition?.catalogListingId ?? "unknown",
                    PurchaseStatus.Failed,
                    failedOrder.FailureReason.ToString()));
                return;
            }

            var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
            var productId = product?.definition?.catalogListingId;
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("Purchase confirmed but product id is missing.");
                m_IsPurchaseInProgress = false;
                
                EventBus<PurchaseFailedEvent>.Post(new PurchaseFailedEvent(
                    productId ?? "unknown",
                    PurchaseStatus.Failed,
                    "Purchase confirmed but product id is missing."));
                return;
            }

            var transactionId = order.Info?.TransactionID;
            if (!string.IsNullOrEmpty(transactionId))
                m_ProcessedTransactionIds.Add(transactionId);

            Debug.Log($"UnityIAPProvider: Purchase confirmed for {productId}");
            m_IsPurchaseInProgress = false;
            EventBus<PurchaseSuccessEvent>.Post(new PurchaseSuccessEvent(
                productId,
                transactionId,
                order.Info?.Receipt));
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription obj)
        {
            Debug.LogError($"UnityIAPProvider: Purchase fetch failed: {obj}");
            if (!m_AwaitingRestorePurchasesFetch)
                return;

            m_AwaitingRestorePurchasesFetch = false;
            // Store restore itself succeeded; still notify so the game can sync from current cache.
            EventBus<RestorePurchaseEvent>.Post(
                new RestorePurchaseEvent(true, m_PendingRestoreMessage));
        }

        private void OnPurchasesFetched(Orders orders)
        {
            Debug.Log("UnityIAPProvider: OnPurchasesFetched");

            // Finish any interrupted NonConsumable payments left pending on the store.
            if (orders?.PendingOrders != null)
            {
                foreach (var pending in orders.PendingOrders)
                    ProcessPurchase(pending);
            }

            if (!m_AwaitingRestorePurchasesFetch)
                return;

            m_AwaitingRestorePurchasesFetch = false;
            EventBus<RestorePurchaseEvent>.Post(
                new RestorePurchaseEvent(true, m_PendingRestoreMessage));
            Debug.Log("UnityIAPProvider: Restore purchases successful (purchases fetched)");
        }

        private void OnProductsFetchFailed(ProductFetchFailed obj)
        {
            Debug.LogError($"UnityIAPProvider: Product fetch failed: {obj}");
        }

        private void OnProductsFetched(List<Product> obj)
        {
            m_StoreController.FetchPurchases();
            Debug.Log("UnityIAPProvider: Products fetched");
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            Debug.LogError($"Store disconnected: {description.Message}");
            InitializeFailed?.Invoke();
        }

        public override UniTask<PurchaseResult> Purchase(string productId)
        {
            var purchase = new PurchaseResult();
            if (m_IsPurchaseInProgress)
            {
                Debug.LogWarning(
                    "Purchase already in progress. Please wait for the current purchase to complete.");
                purchase.Status = PurchaseStatus.Failed;
                purchase.Error = "Purchase already in progress.";
                return UniTask.FromResult(purchase);
            }

            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                purchase.Status = PurchaseStatus.Failed;
                purchase.Error = "StoreController is not initialized.";
                return UniTask.FromResult(purchase);
            }

            if (!IsInitialized)
            {
                purchase.Status = PurchaseStatus.NotInitialized;
                return UniTask.FromResult(purchase);
            }

            var product = m_StoreController.GetProductById(productId);
            m_IsPurchaseInProgress = true;
            if (product != null)
            {
                m_StoreController.PurchaseProduct(product);
                purchase.Status = PurchaseStatus.Pending;
            }
            else
            {
                m_IsPurchaseInProgress = false;
                purchase.Status = PurchaseStatus.ProductNotFound;
                Debug.LogError($"Product with ID {productId} not found in the store.");
            }

            return UniTask.FromResult(purchase);
        }

        public override UniTask RestorePurchases()
        {
            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                return UniTask.CompletedTask;
            }

            m_StoreController.RestoreTransactions((success, error) =>
            {
                if (!success)
                {
                    m_AwaitingRestorePurchasesFetch = false;
                    EventBus<RestorePurchaseEvent>.Post(new RestorePurchaseEvent(false, error));
                    Debug.LogError($"UnityIAPProvider: Restore purchases failed: {error}");
                    return;
                }

                // Refresh purchase cache first so IsPurchased() is accurate for game sync.
                m_PendingRestoreMessage = error;
                m_AwaitingRestorePurchasesFetch = true;
                m_StoreController.FetchPurchases();
            });
            return UniTask.CompletedTask;
        }

        public override bool IsPurchased(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                return false;
            }

            if (string.IsNullOrEmpty(productId))
                return false;

            foreach (var purchase in m_StoreController.GetPurchases())
            {
                if (purchase?.Info?.PurchasedProductInfo != null)
                {
                    foreach (var purchasedProductInfo in purchase.Info.PurchasedProductInfo)
                    {
                        if (purchasedProductInfo.productId == productId)
                            return true;
                    }
                }

                if (purchase?.CartOrdered == null)
                    continue;

                foreach (var item in purchase.CartOrdered.Items())
                {
                    if (item == null)
                        continue;

                    if (string.Equals(item.CatalogListingId, productId, StringComparison.Ordinal))
                        return true;

                    var definition = item.Product?.definition;
                    if (definition == null)
                        continue;

                    if (string.Equals(definition.catalogListingId, productId, StringComparison.Ordinal) ||
                        string.Equals(definition.id, productId, StringComparison.Ordinal) ||
                        string.Equals(definition.storeSpecificId, productId, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        public override string GetLocalizedPrice(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                return string.Empty;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
                return product.metadata.localizedPriceString;

            Debug.LogWarning($"Product with ID {productId} not found.");
            return string.Empty;
        }

        public override decimal GetPrice(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                return 0m;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
                return product.metadata.localizedPrice;

            Debug.LogWarning($"Product with ID {productId} not found.");
            return 0m;
        }

        public override string GetCurrencyCode(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError(
                    "StoreController is not initialized. Please initialize the IAP provider first.");
                return string.Empty;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
                return product.metadata.isoCurrencyCode;

            Debug.LogWarning($"Product with ID {productId} not found.");
            return string.Empty;
        }

        private UnityEngine.Purchasing.ProductType Convert(ProductType type)
        {
            return type switch
            {
                ProductType.Consumable => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                ProductType.Subscription => UnityEngine.Purchasing.ProductType.Subscription,
                _ => UnityEngine.Purchasing.ProductType.Consumable
            };
        }
    }
}