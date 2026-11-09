using System;
using TitoX.Core;

namespace WData
{
    public abstract class DataBlock<TManager, TData>
        where TManager : DataBlock<TManager, TData>, new()
        where TData : class, new()
    {
        private static TManager _instance;
        private bool _dirty;

        protected TData Data { get; private set; }

        public static TManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = new TManager();

                _instance.Data = DataFileHandler.LoadFromDevice<TData>(_instance.FileName);

                if (_instance.Data == null)
                {
                    _instance.Data = new TData();
                    _instance.Save();
                }

                _instance.Init();

                return _instance;
            }
        }

     
        protected virtual string FileName => typeof(TData).Name;

        protected virtual void Init()
        {
            AppLifeCycle.Instance.OnApplicationPauseEvent += OnApplicationPause;
            AppLifeCycle.Instance.OnApplicationQuitEvent += OnApplicationQuit;
        }
        
        protected void MarkDirty()
        {
            _dirty = true;
        }
        
        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause)
                Save();
        }

        protected virtual void OnApplicationQuit()
        {
            Save();
        }

        public void Save(bool force = false)
        {
            if (!force && !_dirty)
                return;

            DataFileHandler.SaveToDevice(Data, FileName);

            _dirty = false;
        }

        public void Delete()
        {
            DataFileHandler.DeleteInDevice(FileName);

            Data = new TData();

            Save();
        }
    }
}