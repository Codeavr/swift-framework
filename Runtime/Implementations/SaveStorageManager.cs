﻿using Newtonsoft.Json.Serialization;
using SwiftFramework.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace SwiftFramework.StorageManager
{
    [DefaultModule]
    internal class SaveStorageManager : Module, ISaveStorage
    {
        private const string NOT_LINKED = "not_linked";

        private const string DEFAULT_SAVE_ID = "default_save";

        private static string editorSavePath => Application.dataPath + "/save.json";

        private Dictionary<string, Dictionary<string, object>> linkedItems = new Dictionary<string, Dictionary<string, object>>();

        private Dictionary<string, object> notLinkedItems;

        private string saveId;

        private SaveItemsContainer container = new SaveItemsContainer();

        public event Action OnBeforeSave = () => { };
        public event Action OnAfterLoad = () => { };

        protected override IPromise GetInitPromise()
        {
            saveId = DEFAULT_SAVE_ID;

            App.Boot.OnPaused += Boot_OnPaused;

            LoadFromPlayerPrefs();

            return base.GetInitPromise();
        }

        private void LoadFromPlayerPrefs()
        {
            string json = PlayerPrefs.GetString(saveId, null);
#if UNITY_EDITOR
            json = File.Exists(editorSavePath) ? File.ReadAllText(editorSavePath) : null;
            if (json != null)
            {
                Debug.Log($"Original save file size: <b>{Encoding.ASCII.GetByteCount(json).ToFileSize() }</b>, compressed file size: <color=green><b>{Encoding.ASCII.GetByteCount(Json.Compress(json)).ToFileSize()}</b></color>");
            }
#endif
            if (string.IsNullOrEmpty(json) == false)
            {
                Parse(json);
            }

            if (linkedItems.ContainsKey(NOT_LINKED) == false)
            {
                linkedItems.Add(NOT_LINKED, new Dictionary<string, object>());
            }
            notLinkedItems = linkedItems[NOT_LINKED];
        }

        private void Parse(string json)
        {
            container = Json.Deserialize<SaveItemsContainer>(json);

            linkedItems = new Dictionary<string, Dictionary<string, object>>();

            foreach (var linked in container.items)
            {
                if (linked == null || string.IsNullOrEmpty(linked.link) || linked.item == null)
                {
                    continue;
                }
                if (linkedItems.TryGetValue(linked.link, out Dictionary<string, object> dict) == false)
                {
                    dict = new Dictionary<string, object>();
                    linkedItems.Add(linked.link, dict);
                }
                string itemKey = linked.item.GetType().FullName;
                if (dict.ContainsKey(itemKey) == false)
                {
                    dict.Add(itemKey, linked.item);
                }
            }
            
            OnAfterLoad();
        }

        private void Boot_OnPaused()
        {
            OnBeforeSave();
            WriteSave();
        }


        private void WriteSave()
        {
            container.id = saveId;
            container.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            container.version++;
            container.items = new List<LinkedItem>();

            foreach (var linked in linkedItems)
            {
                foreach (var item in linked.Value)
                {
                    container.items.Add(new LinkedItem() { link = linked.Key, item = item.Value });
                }
            }

            string json = Json.Serialize(container, Newtonsoft.Json.Formatting.None);
            PlayerPrefs.SetString(saveId, json);
            PlayerPrefs.Save();

#if UNITY_EDITOR
            File.WriteAllText(editorSavePath, Json.Serialize(container, Newtonsoft.Json.Formatting.Indented));
#endif
        }

        public string GetSaveJson()
        {
            return PlayerPrefs.GetString(saveId);
        }

        public T Load<T>()
        {
            return Load<T>(notLinkedItems);
        }

        public void OverrideSaveJson(string saveJson)
        {
            Parse(saveJson);
        }

        public void Save<T>(T data)
        {
            Save(data, notLinkedItems);
        }

        private T Load<T>(Dictionary<string, object> dict)
        {
            string type = typeof(T).FullName;
            if (dict.TryGetValue(type, out object value))
            {
                return (T)value;
            }
            return default;
        }

        private void Save<T>(T data, Dictionary<string, object> dict)
        {
            string type = typeof(T).FullName;
            if (dict.ContainsKey(type) == false)
            {
                dict.Add(type, data);
            }
            dict[type] = data;
        }

        public bool Exists<T>()
        {
            return Exists(typeof(T));
        }

        public bool Exists(Type type)
        {
            return notLinkedItems.ContainsKey(type.FullName);
        }

        public void SetSaveId(string id)
        {
            if (id != saveId)
            {
                saveId = id;
                LoadFromPlayerPrefs();
            }
        }

        public void RegisterState<T>(Func<T> state)
        {
            OnBeforeSave += () => 
            {
                Save(state());
            };
        }

        public void RegisterState<T>(Func<T> state, ILink link)
        {
            OnBeforeSave += () =>
            {
                Save(state(), link);
            };
        }

        public void Save<T>(T data, ILink link)
        {
            string path = link.GetPath();
            if (linkedItems.ContainsKey(path) == false)
            {
                linkedItems.Add(path, new Dictionary<string, object>());
            }
            if (linkedItems.TryGetValue(path, out Dictionary<string, object> dict))
            {
                Save(data, dict);
            }
        }

        public T Load<T>(ILink link)
        {
            if (linkedItems.TryGetValue(link.GetPath(), out Dictionary<string, object> dict))
            {
                return Load<T>(dict);
            }
            return default;
        }

        public T LoadOrCreateNew<T>(ILink link) where T : new()
        {
            T value = Load<T>(link);
            if(value != null)
            {
                return value;
            }
            return new T();
        }

        public T LoadOrCreateNew<T>() where T : new()
        {
            T value = Load<T>();

            if (value != null)
            {
                return value;
            }

            return new T();
        }

        public T LoadOrCreateNew<T>(Func<T> defaultValue)
        {
            T value = Load<T>();

            if (value != null)
            {
                return value;
            }

            return defaultValue();
        }

        public T LoadOrCreateNew<T>(ILink link, Func<T> defaultValue)
        {
            T value = Load<T>(link);
            if (value != null)
            {
                return value;
            }
            return defaultValue();
        }

        public bool Exists<T>(ILink link)
        {
            return Exists(typeof(T), link);
        }

        public bool Exists(Type type, ILink link)
        {
            if (linkedItems.TryGetValue(link.GetPath(), out Dictionary<string, object> dict))
            {
                return dict.ContainsKey(type.FullName);
            }
            return false;
        }

        public void Delete<T>()
        {
            Delete(typeof(T));
        }

        public void Delete(Type type)
        {
            if (Exists(type))
            {
                notLinkedItems.Remove(type.FullName);
            }
        }

        public void Delete<T>(ILink link)
        {
            Delete(typeof(T), link);
        }

        public void Delete(Type type, ILink link)
        {
            if (linkedItems.TryGetValue(link.GetPath(), out Dictionary<string, object> dict))
            {
                if (dict.ContainsKey(type.FullName))
                {
                    dict.Remove(type.FullName);
                }
            }
        }

        public T OverwriteScriptable<T>(T source) where T : ScriptableObject
        {
            if (Exists<T>())
            {
                T obj = Load<T>();
                var json = JsonUtility.ToJson(obj);
                JsonUtility.FromJsonOverwrite(json, source);
            }
            return source;
        }

        public void DeleteAll()
        {
            container = new SaveItemsContainer();
            notLinkedItems.Clear();
            linkedItems.Clear();
            WriteSave();
        }

        [Serializable]
        private class SaveItemsContainer
        {
            public int version;
            public long timestamp;
            public string id;
            public List<LinkedItem> items = new List<LinkedItem>();
        }

        [Serializable]
        private class LinkedItem
        {
            public string link;
            public object item;
        }
    }

}