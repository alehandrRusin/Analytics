using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics
{
    public class EventService : MonoBehaviour
    {
        private const float CooldownBeforeSend = 1f;
        private const string SaveDataKey = "EventService_SaveData";
        
        [SerializeField] private string serverUrl;
        
        public static EventService Instance { get; private set; }

        private List<EventData> _dataToSend;
        private List<EventData> _activeRequestsData;
        
        private IDisposable _timerDisposable;
        private CompositeDisposable _requestDisposables;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            _requestDisposables = new CompositeDisposable();
            _dataToSend = new List<EventData>();
            _activeRequestsData = new List<EventData>();

            if (TryLoadDataToSend(out var data))
            {
                _dataToSend = data;
                TrySendRequest();
            }
        }

        public void TrackEvent(string type, string data)
        {
            _dataToSend.Add(new EventData(type, data));
            
            if (_timerDisposable == null)
            {
                WaitForTimer(() =>
                {
                    TrySendRequest();

                    _timerDisposable?.Dispose();
                    _timerDisposable = null;
                });
            }
        }

        private void WaitForTimer(Action callback)
        {
            _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(CooldownBeforeSend)).Subscribe(_ =>
            {
                callback();
            }).AddTo(this);
        }

        private void TrySendRequest()
        {
            var currentData = new List<EventData>(_dataToSend);
            _activeRequestsData.AddRange(currentData);
            _dataToSend.Clear();

            string json = JsonUtility.ToJson(currentData);
            IDisposable disposable = RequestHandler.SendPostRequest(serverUrl, json, result =>
            {
                currentData.ForEach(x => _activeRequestsData.Remove(x));
                
                if (result != UnityWebRequest.Result.Success)
                {
                    _dataToSend.AddRange(currentData);
                }
            });
           _requestDisposables.Add(disposable);
        }

        private void SaveDataToSend()
        {
            List<EventData> saveData = new List<EventData>();
            saveData.AddRange(_dataToSend);
            saveData.AddRange(_activeRequestsData);
            
            string json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(SaveDataKey, json); 
        }

        private bool TryLoadDataToSend(out List<EventData> data)
        {
            var jsonString = PlayerPrefs.GetString(SaveDataKey);
            data = JsonConvert.DeserializeObject<List<EventData>>(jsonString);

            return data != null;
        }

        private void OnDestroy()
        {
            _requestDisposables?.Dispose();
            _timerDisposable?.Dispose();
            
            SaveDataToSend();
        }
    }
}
