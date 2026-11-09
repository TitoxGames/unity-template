using System.Collections.Generic;
using UnityEngine;

namespace Tito.Services.IAP
{
    [CreateAssetMenu(
        fileName = "IAPCatalog",
        menuName = "TitoX/IAP/IAP Catalog")]
    public class IAPCatalog : ScriptableObject
    {
        [SerializeField]
        private List<IAPProduct> _products = new();

        public IReadOnlyList<IAPProduct> Products => _products;

        public IAPProduct GetProduct(string id)
        {
            return _products.Find(x => x.Id == id);
        }

        public bool TryGetProduct(string id, out IAPProduct product)
        {
            product = GetProduct(id);
            return product != null;
        }

        public bool Contains(string id)
        {
            return _products.Exists(x => x.Id == id);
        }
    }
}