using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CustomDict
{
    [System.Serializable]
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
        /// ���ο� KeyValuePair�� �߰��ϸ�, �ν����͵� ������Ʈ
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            SyncInspectorFromDictionary();
        }
        /// <summary>
        /// KeyValuePair�� �����ϸ�, �ν����͵� ������Ʈ
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
        /// �ν����͸� ��ųʸ��� �ʱ�ȭ
        /// </summary>
        public void SyncInspectorFromDictionary()
        {
            //�ν����� Ű ��� ����Ʈ �ʱ�ȭ
            SD_Keys.Clear();
            SD_Values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                SD_Keys.Add(pair.Key); SD_Values.Add(pair.Value);
            }
        }

        /// <summary>
        /// ��ųʸ��� �ν����ͷ� �ʱ�ȭ
        /// </summary>
        public void SyncDictionaryFromInspector()
        {
            //��ųʸ� Ű ��� ����Ʈ �ʱ�ȭ
            foreach (var key in SD_Keys.ToList())
            {
                base.Remove(key);
            }

            for (int i = 0; i < SD_Keys.Count; i++)
            {
                //�ߺ��� Ű�� �ִٸ� ���� ���
                if (this.ContainsKey(SD_Keys[i]))
                {
                    Debug.LogError("�ߺ��� Ű�� �ֽ��ϴ�.");
                    break;
                }
                base.Add(SD_Keys[i], SD_Values[i]);
            }
        }

        public void OnAfterDeserialize()
        {
            //Debug.Log(this + string.Format("�ν����� Ű �� : {0} �� �� : {1}", SD_Keys.Count, SD_Values.Count));

            //�ν������� Key Value�� KeyValuePair ���¸� �� ���
            if (SD_Keys.Count == SD_Values.Count)
            {
                SyncDictionaryFromInspector();
            }
        }
    }


    [System.Serializable]
    public class SerializeDictCurrentEquipment : SerializableDictionary<EquipmentSlot, string> { }

    [System.Serializable]
    public class SerializeDictEquipmentSpriteRenderer : SerializableDictionary<EquipmentSlot, SpriteRenderer> { }

    [System.Serializable]
    public class SerializeDictBodyColor : SerializableDictionary<EquipmentSlot, Color> { }

}