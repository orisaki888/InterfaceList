using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インターフェースをインスペクタから登録できるリストクラス
/// nullと対象のインターフェイスを実装していないコンポーネントも登録はできるがリストからは除外される
/// </summary>
/// <typeparam name="T">対象のインターフェース</typeparam>
[Serializable]
public class InterfaceList<T> : IReadOnlyList<T>, ISerializationCallbackReceiver where T : class
{
    // シリアライズされるのはこのComponentリストのみ
    [SerializeField, HideInInspector]
    private List<Component> _components;

    private List<T> _internalList = new List<T>();

    public int Count => _internalList.Count;

    public T this[int index] => _internalList[index];

    public InterfaceList()
    {
        // Tがインターフェースであることを実行時にチェック
        if (!typeof(T).IsInterface)
        {
            Debug.LogErrorFormat("{0} はインターフェースではありません。", typeof(T).FullName);
        }
    }


    /// <summary>   
    /// シリアライズ直前に呼ばれる
    /// </summary>
    public void OnBeforeSerialize()
    {
        //何もしない
    }

    /// <summary>
    /// デシリアライズ直後に呼ばれる
    /// </summary>
    public void OnAfterDeserialize()
    {
        if (_components == null || _components.Count == 0)
        {
            return;
        }

        _internalList.Clear();
        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];
            // nullチェック
            if (c.GetType() == typeof(Component))
            {
                continue;
            }

            //インターフェースを実装しているものだけ追加
            if (c is T isT)
            {
                _internalList.Add(isT);
            }
            else
            {
                Debug.LogWarning($"コンポーネント '{c.GetType().Name}' はインターフェース '{typeof(T).Name}' を実装していません。");
            }
        }
    }

    /// <summary>
    /// インターフェースを追加します。
    /// Component 派生クラス以外は追加できません。
    /// </summary>
    /// <param name="item"></param>
    public void AddInterface(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item is Component c)
        {
            _internalList.Add(item);
            _components.Add(c);
        }
        else
        {
            Debug.LogError($"InterfaceList<{typeof(T).Name}> に追加できるのは Component 派生クラスのみです。");
        }
    }

    /// <summary>
    /// インターフェースを追加します。
    /// Interface を実装していない Component は追加できません。
    /// </summary>
    /// <param name="item"></param>
    public void AddInterface(Component item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item is T)
        {
            _internalList.Add(item as T);
            _components.Add(item);
        }
        else
        {
            Debug.LogError($"コンポーネント '{item.GetType().Name}' はインターフェース '{typeof(T).Name}' を実装していません。");
        }
    }

    public void ForEach(Action<T> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        for (int i = 0; i < Count; i++)
        {
            action(this[i]);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _internalList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}