using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(InterfaceList<>), true)]
public class InterfaceListDrawer : PropertyDrawer
{
    private const float HelpBoxExtraSpacing = 2f;
    

    /// <summary>
    /// Component が指定のインターフェース型を実装しているかどうか。
    /// </summary>
    private static bool ImplementsInterface(Type interfaceType, Component comp)
    {
        if (interfaceType == null || comp == null) return false;
        var compType = comp.GetType();
        // 通常の判定
        if (interfaceType.IsAssignableFrom(compType)) return true;
        // 念のため明示的に取得したインターフェース一覧を確認（アセンブリ差異やジェネリック一時不一致の診断用）
        var ifaceNames = compType.GetInterfaces();
        return ifaceNames.Any(i => i == interfaceType);
    }

    /// <summary>
    /// Drawerが取り扱うインターフェースを取得。
    /// </summary>
    private Type GetInterfaceType() => fieldInfo.FieldType.GetGenericArguments()[0];

    /// <summary>
    /// 事前チェックで最初の不正要素メッセージを取得（過剰なログを避けるため 1 件のみ）。
    /// </summary>
    private static bool TryGetPreErrorMessage(SerializedProperty componentsProperty, Type interfaceType, out string message)
    {
        message = null;
        if (componentsProperty == null || !componentsProperty.isArray) return false;

        int size = componentsProperty.arraySize;
        for (int i = 0; i < size; i++)
        {
            var element = componentsProperty.GetArrayElementAtIndex(i);
            var val = element.objectReferenceValue;
            if (val == null) continue;

            if (val is Component c)
            {
                if (!ImplementsInterface(interfaceType, c))
                {
                    message = $"要素[{i}]: '{c.name}' は '{interfaceType.Name}' を実装していません。";
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 指定メッセージの HelpBox 高さを計算。
    /// </summary>
    private static float CalcHelpBoxHeight(string message, float width)
    {
        if (string.IsNullOrEmpty(message)) return 0f;
        var content = new GUIContent(message);
        return Mathf.Max(EditorGUIUtility.singleLineHeight * 2f, EditorStyles.helpBox.CalcHeight(content, width));
    }

    /// <summary>
    /// 1 要素をバリデーションし、必要に応じて参照の置き換える。
    /// </summary>
    private static void ValidateElement(SerializedProperty element, Type interfaceType)
    {
        // null は許容。検証不要。
        var assignedObject = element.objectReferenceValue;
        if (assignedObject == null) return;

        if (assignedObject is Component draggedComponent)
        {
            // その Component がインターフェースを実装しているか確認
            if (!ImplementsInterface(interfaceType, draggedComponent))
            {
                // 同じ GameObject に実装している別コンポーネントがあるか探索
                var g = draggedComponent.gameObject;
                var fallback = g.GetComponents<Component>()
                    .FirstOrDefault(c => c != null && ImplementsInterface(interfaceType, c));
                if (fallback != null)
                {
                    element.objectReferenceValue = fallback;
                }
            }
        }
        else if (assignedObject is GameObject go)
        {
            // GameObject が直接ドラッグされた場合のみ、自動で適切な Component を割り当てる（候補が1つ以上なら先頭）
            var candidates = go.GetComponents<Component>()
                .Where(c => c != null && interfaceType.IsAssignableFrom(c.GetType()))
                .ToArray();

            if (candidates.Length == 0)
            {
                element.objectReferenceValue = null;
                return;
            }

            element.objectReferenceValue = candidates[0];
        }
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var componentsProperty = property.FindPropertyRelative("_components");
        Type interfaceType = GetInterfaceType();

        EditorGUI.BeginProperty(position, label, property);
        
        // --- リスト描画前のエラーチェック（InfoBox表示）---
        if (componentsProperty.isArray && TryGetPreErrorMessage(componentsProperty, interfaceType, out var preErrorMsg))
        {
            float helpBoxHeight = CalcHelpBoxHeight(preErrorMsg, position.width);
            var helpRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
            EditorGUI.HelpBox(helpRect, preErrorMsg, MessageType.Warning);
            // リスト描画位置をHelpBoxの下にずらす
            position = new Rect(
                position.x,
                position.y + helpBoxHeight + HelpBoxExtraSpacing,
                position.width,
                Mathf.Max(0f, position.height - helpBoxHeight - HelpBoxExtraSpacing)
            );
        }

        // 通常のリスト描画
        EditorGUI.PropertyField(position, componentsProperty, label, true);
        
        // 各要素のバリデーション処理
        if (componentsProperty.isArray)
        {
            int size = componentsProperty.arraySize;
            for (int i = 0; i < size; i++)
            {
                var element = componentsProperty.GetArrayElementAtIndex(i);
                ValidateElement(element, interfaceType);
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var componentsProperty = property.FindPropertyRelative("_components");
        float baseHeight = EditorGUI.GetPropertyHeight(componentsProperty, true);

        // OnGUI と同様に、リスト描画前に出すエラーメッセージの高さを加算
        float extra = 0f;
        if (componentsProperty.isArray && TryGetPreErrorMessage(componentsProperty, GetInterfaceType(), out var preErrorMsg))
        {
            // 幅は概ねのビュー幅を採用（PropertyDrawerでは正確なRectはまだないため）
            float approxWidth = Mathf.Max(100f, EditorGUIUtility.currentViewWidth - 40f);
            extra = CalcHelpBoxHeight(preErrorMsg, approxWidth) + HelpBoxExtraSpacing; // 少し余白
        }

        return baseHeight + extra;
    }
}