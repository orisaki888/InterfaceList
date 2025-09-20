# InterfaceList

インスペクターから特定のインターフェイスを実装したコンポーネントを登録できるシリアライズ対応のリスト型です。

## 追加方法

1. Unity の Package Manager を開く
1. 右上の [+] メニューから「Install package from git URL...」を選択
1. 次の URL を貼り付けてインストール

```text
https://github.com/orisaki888/InterfaceList.git
```


## 使い方

1. 対象のインターフェイスを用意

```csharp
public interface IInteractable
{
	void Interact();
}
```

1. `MonoBehaviour` に `InterfaceList<T>` フィールドを定義（`T` はインターフェイス型）

```csharp
using UnityEngine;

public class Interactor : MonoBehaviour
{
	[SerializeField]
	private InterfaceList<IInteractable> interactables;

	private void Start()
	{
		// 実行時の利用例（IReadOnlyList として列挙可能）
		foreach (var it in interactables)
		{
			it.Interact();
		}
	}
}
```

1. インスペクターで、`IInteractable` を実装したコンポーネントを持つオブジェクト/コンポーネントをリストに登録
- リストに未実装の要素が含まれていると、インスペクター上部に警告が表示されます。

## 実行時 API 概要

`InterfaceList<T>` は `IReadOnlyList<T>` を実装しています。

- `int Count` / `T this[int index]`
- `IEnumerator<T> GetEnumerator()`（`foreach` で列挙可能）
- `void ForEach(Action<T> action)`
- ランタイム追加用: `AddInterface(T item)` / `AddInterface(Component item)`
  - いずれも `Component` を基にした追加のみサポート（`T` はインターフェイス）。未実装の `Component` 追加はエラーになります。

## 注意点

- `T` はインターフェイス型のみ対応します。
- 実行時には、`null` と `T` を実装していないコンポーネントはリストから除外されます。
- AddInterfaceで追加できますが、基本的にはReadOnlyListとして利用することを想定しています。細かく要素を追加・削除したい場合は、別途`var list = new List<IInterface>(interfaceList)`などの方法でリストに変換してから操作してください。
  