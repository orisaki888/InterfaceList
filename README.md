# InterfaceList

インスペクターから特定のインターフェイスを実装したコンポーネントを登録できるシリアライズ対応のリスト型です。

## できること

- インスペクターで `GameObject` や `Component` をドラッグ&ドロップして登録
- 対象インターフェイスを実装していない要素は警告表示され、実行時には除外
- 実行時は `IReadOnlyList<T>` として列挙可能。未実装や null は内部リストから除外

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

- GameObject を直接ドラッグした場合、同一オブジェクト上で最初に見つかった該当コンポーネントが自動で割り当てられます。
- 誤って未対応のコンポーネントを入れた場合は、同一 GameObject 上の該当コンポーネントに置換します。見つからなければ null になります。
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

## 追加方法

1. Unity の Package Manager を開く
1. 右上の [+] メニューから「Install package from git URL...」を選択
1. 次の URL を貼り付けてインストール

```text
https://github.com/orisaki888/InterfaceList.git
```
