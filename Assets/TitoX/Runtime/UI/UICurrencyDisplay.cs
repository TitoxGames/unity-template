using System;
using AssetKits.ParticleImage;
using TitoX.Core.EventBus;
using TitoX.GameData;
using TitoX.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TitoX.Core;

namespace TitoX.UI
{
    public class UICurrencyDisplay: MonoBehaviour
    {
        [SerializeField] private string prependString;
        [SerializeField] private string postpendString;
        [SerializeField] private CurrencyConfigSO currencyConfig;
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private ParticleImage currencySpawner;
        [SerializeField] private AudioSource collectSfx;
        [SerializeField, Min(1)]
        private int maxParticles = 25;

        private double _remainingPaydown;

        public UnityEvent OnCurrencyUpdated;

        private void Start()
        {
            if(currencySpawner) currencySpawner.onAnyParticleFinished.AddListener(PayDownOutstanding);
            EventBus<PlayerCurrencyChangedEvent>.Subscribe(OnCurrencyChanged);
        }

        private void OnEnable()
        {
            UpdateDisplayValue(currencyConfig.CurrencyType, GetCoinDisplayValue());
        }

        private double GetCoinDisplayValue()
        {
            if(currencyConfig.CurrencyType == CurrencyType.Coin)
                return Player.Instance.Currency.Coin;
            return Player.Instance.Currency.Gem;
        }

        private void OnCurrencyChanged(PlayerCurrencyChangedEvent obj)
        {
            UpdateDisplayValue(obj.CurrencyType, obj.Value);
        }
        
        private void OnApplicationQuit()
        {
            if (currencySpawner == null || currencySpawner.particleCount == 0)
                return;

            currencySpawner.Clear();
            PayDownOutstanding();
        }
        
        private void OnDisable()
        {
            if (_remainingPaydown > 0)
            {
                Player.Instance.Currency.AddCoin(_remainingPaydown);
                Player.Instance.Save(true);
            }

            _remainingPaydown = 0f;
            if(currencySpawner) currencySpawner.Stop(true);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (currencySpawner == null || currencySpawner.particleCount == 0)
                return;

            if(currencySpawner) currencySpawner.Clear();
            PayDownOutstanding();
        }

        private void OnDestroy()
        {
            EventBus<PlayerCurrencyChangedEvent>.Unsubscribe(OnCurrencyChanged);

            if (_remainingPaydown > 0)
            {
                Player.Instance.Currency.AddCoin(_remainingPaydown);
                Player.Instance.Save(true);
            }
            _remainingPaydown = 0f;
        }
        
        public void BurstCurrency(double totalPayout, Vector3 position)
        {
            int num = (int)Math.Min(Math.Ceiling(totalPayout), maxParticles);
            _remainingPaydown += totalPayout;

            if (!gameObject.activeInHierarchy)
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    OnDisable();
                    return;
                }
            }

            if (currencySpawner == null)
            {
                return;
            }

            currencySpawner.SetBurst(0, 0f, Mathf.Min(num, 35));
            currencySpawner.transform.position = position;
            currencySpawner.sprite = currencyConfig.Sprite;
            currencySpawner.Play();
            currencySpawner.onLastParticleFinished.AddListener(OnLastParticleFinished);
        }
        
        private void OnLastParticleFinished()
        {
            Player.Instance.Save(true);
            if(currencySpawner) currencySpawner.onLastParticleFinished.RemoveListener(OnLastParticleFinished);
            OnCurrencyUpdated?.Invoke();
        }

        private void PayDownOutstanding()
        {
            double paydown;
            
            if (currencySpawner == null)
            {
                return;
            }

            if (currencySpawner.particles.Count == 0)
            {
                paydown = _remainingPaydown;
            }
            else
            {
                paydown = _remainingPaydown / (float)currencySpawner.particles.Count;
            }

            _remainingPaydown -= paydown;
            switch (currencyConfig.CurrencyType)
            {
                case CurrencyType.Coin:
                    Player.Instance.Currency.AddCoin(paydown);
                    break;

                case CurrencyType.Gem:
                    Player.Instance.Currency.AddGem(paydown);
                    break;
            }
            if(collectSfx) collectSfx.Play();
        }
        
        private void UpdateDisplayValue(CurrencyType type, double value)
        {
            if (currencyText != null && type == currencyConfig.CurrencyType)
                currencyText.SetText(prependString + NumberConverter.ReturnIdleMoneyString(value, 2, "", false) + postpendString);
        }
    }
}