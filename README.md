# IDataObjectViewer
C#でWindowsのクリップボードとDrag&Dropを完全に操作することが目標

* 調査しながら書いてるので、適切にブランチ切ってない
* バグバグ注意
  * メタファイル関連は作業中
  * IStorageは未実装
  * COM側の実装依存と思われる場所あり。
  * COM側で例外を投げた場合は未調整
  * アンマネージ領域のメモリを保持し続けるやつは暫定。危険なので注意。