using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
namespace CustomDict
{
    [Serializable]
    //[CanEditMultipleObjects]
    //[ExecuteInEditMode]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<TKey> SD_Keys;
        public List<TValue> SD_Values;

        public SerializableDictionary()
        {
            SD_Keys = new List<TKey>();
            SD_Values = new List<TValue>();
            SyncInspectorFromDictionary();
        }
        /// <summary>
        /// 새로운 KeyValuePair을 추가하며, 인스펙터도 업데이트
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            SyncInspectorFromDictionary();
        }
        /// <summary>
        /// KeyValuePair을 삭제하며, 인스펙터도 업데이트
        /// </summary>
        /// <param name="key"></param>
        public new void Remove(TKey key)
        {
            base.Remove(key);
            SyncInspectorFromDictionary();
        }

        public void OnBeforeSerialize()
        {
        }
        /// <summary>
        /// 인스펙터를 딕셔너리로 초기화
        /// </summary>
        public void SyncInspectorFromDictionary()
        {
            //인스펙터 키 밸류 리스트 초기화
            SD_Keys.Clear();
            SD_Values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                SD_Keys.Add(pair.Key); SD_Values.Add(pair.Value);
            }
        }

        /// <summary>
        /// 딕셔너리를 인스펙터로 초기화
        /// </summary>
        public void SyncDictionaryFromInspector()
        {
            //딕셔너리 키 밸류 리스트 초기화
            foreach (var key in SD_Keys.ToList())
            {
                base.Remove(key);
            }

            for (int i = 0; i < SD_Keys.Count; i++)
            {
                //중복된 키가 있다면 에러 출력
                if (this.ContainsKey(SD_Keys[i]))
                {
                    Debug.LogError("중복된 키가 있습니다.");
                    break;
                }
                base.Add(SD_Keys[i], SD_Values[i]);
            }
        }

        public void OnAfterDeserialize()
        {
            //Debug.Log(this + string.Format("인스펙터 키 수 : {0} 값 수 : {1}", SD_Keys.Count, SD_Values.Count));

            //인스펙터의 Key Value가 KeyValuePair 형태를 띌 경우
            if (SD_Keys.Count == SD_Values.Count)
            {
                SyncDictionaryFromInspector();
            }
        }
    }


    [Serializable]
    public class CustomDictCurrentEquipment : SerializableDictionary<EquipmentSlot, string> { }

    [Serializable]
    public class CustomDictEquipmentSpriteRenderer : SerializableDictionary<EquipmentSlot, SpriteRenderer> { }

    [Serializable]
    public class CustomDictBodyColor : SerializableDictionary<EquipmentSlot, Color> { }

    [Serializable]
    public class CustomDictKeyMap : SerializableDictionary<KeyMapping, KeyCode> { }

    [Serializable]
    public class CustomDictSkill : SerializableDictionary<string, SkillSpec> { };

}