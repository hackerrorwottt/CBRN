using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBRN.Network
{
    [Serializable]
    public class QueuedSessionWrapper
    {
        public List<string> serializedPayloads = new List<string>();
    }

    /// <summary>
    /// Local offline cache for session logs.
    /// If college Wi-Fi or cellular drops during a VR session, logs are safely serialized
    /// to persistent device storage and automatically dequeued and posted when connectivity returns.
    /// </summary>
    public class OfflineQueue : MonoBehaviour
    {
        private const string PrefsKey = "CBRN_Offline_Session_Queue";

        private QueuedSessionWrapper _queueWrapper;

        private void Awake()
        {
            LoadQueue();
        }

        private void LoadQueue()
        {
            string rawJson = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(rawJson))
            {
                try
                {
                    _queueWrapper = JsonUtility.FromJson<QueuedSessionWrapper>(rawJson);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[OfflineQueue] Error parsing offline cache: {e.Message}");
                    _queueWrapper = new QueuedSessionWrapper();
                }
            }
            else
            {
                _queueWrapper = new QueuedSessionWrapper();
            }
        }

        private void SaveQueue()
        {
            string rawJson = JsonUtility.ToJson(_queueWrapper);
            PlayerPrefs.SetString(PrefsKey, rawJson);
            PlayerPrefs.Save();
        }

        public void Enqueue(SessionUploadPayload payload)
        {
            if (payload == null) return;

            string json = JsonUtility.ToJson(payload);
            _queueWrapper.serializedPayloads.Add(json);
            SaveQueue();

            Debug.Log($"[OfflineQueue] Session enqueued for offline sync. Total cached: {_queueWrapper.serializedPayloads.Count}");
        }

        public List<SessionUploadPayload> GetPendingPayloads()
        {
            List<SessionUploadPayload> list = new List<SessionUploadPayload>();
            foreach (var json in _queueWrapper.serializedPayloads)
            {
                try
                {
                    list.Add(JsonUtility.FromJson<SessionUploadPayload>(json));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[OfflineQueue] Failed to deserialize queued session: {e.Message}");
                }
            }
            return list;
        }

        public void ClearPending()
        {
            _queueWrapper.serializedPayloads.Clear();
            SaveQueue();
            Debug.Log("[OfflineQueue] Offline sync cache cleared.");
        }
    }
}
