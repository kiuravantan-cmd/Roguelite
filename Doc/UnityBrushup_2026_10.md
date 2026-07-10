# 第10週：吸い込む快感とレベルアップ！「成長」の手触りを極める

## 本日の目標
今日は、ゲームの醍醐味である「成長」のシステムを作り、さらに「魔法のツール」を使って画面の演出を劇的に強化します！
1. 吸い込む快感：近づくと「シュバッ！」と飛んでくる経験値オーブを作る。
2. 成長のロジック：経験値ゲージとレベルアップのシステムを構築する。
3. UIの魔術（グラデーション）：プログラムの力だけでUIを綺麗なグラデーションに染め上げる。
4. 魔法のツール（DOTween）：プロご用達のツールを使い、たった1行でリロードUIを動かす。

## 1. 経験値オーブを作ろう（マグネット吸引）

ただ触れるだけではなく、近づくと自動でプレイヤーに吸い寄せられる気持ちいいオーブを作ります。
Scripts/InGame/Item フォルダを作成し、ExperienceOrb.cs を作ります。
ファイル名： ExperienceOrb.cs
``` cs

```

## 2. 敵からのドロップ処理
敵が死んだときに、さっき作ったオーブを落とすようにします。
ファイル名： EnemyState.cs
``` diff
```
【エディタでの作業】 Enemyプレハブの Experience Orb Prefab 枠に、先ほど作ったオーブのプレハブをセットします。

## 3. プレイヤーの経験値とレベルアップ処理
PlayerController.cs を開き、経験値の受け皿と、レベルアップの仕組みを追加します。

### 3-1. UIの配置
HierarchyのCanvasの中に、UIの Slider を作り、画面上に配置します。（Interactable のチェックを外し、色は自由に変更してください）
同じくCanvasの中に TextMeshPro を作り、画面の真ん中に大きく配置します。

### 3-2. 経験値の取得
ファイル名： PlayerController.cs
``` diff
```

### 3-3. レベルアップ処理
ファイル名： PlayerController.cs
``` diff
```
コードを保存したら、PlayerControllerの枠にSliderとTextをセットしてください。

## 4. 演出強化①：素材不要！プログラムで作るグラデーションUI
経験値バーを綺麗なグラデーションにしたいですが、画像素材がありません。
そこで、UIの頂点色をプログラムで塗り替える魔法のスクリプトを作ります！
Scripts/InGame/UI フォルダを作成し、UIGradient.cs を作ります。
ファイル名： UIGradient.cs
``` cs
```
【エディタでの作業】
Hierarchyで経験値Sliderの中の Fill Area > Fill を選択します。
今作った UIGradient.cs をアタッチし、Top Color と Bottom Color を好きな色（黄色とオレンジなど）に変更します。これだけでグラデーションになります！

## 5. リロード時間の可視化
リロード中、「あと何秒で撃てるか」が分かるサークル画像と、「リロード中…」というテキストを作ります。<br>
ここでは、アニメーションツール 「DOTween（ドゥートゥイーン）」 を導入し、面倒な計算をせずに「たった1行」でUIをアニメーションさせましょう！

### 5-1. サークルUIの作成
**【重要：2D Spriteパッケージの準備】** もし右クリックメニューに「Sprites > Circle」が見当たらない場合は、以下の手順で追加します。
1. 上部メニューの `Window > Package Manager` を開きます。
2. 左上を `Packages: Unity Registry` に変更し、右上の検索窓で `2D Sprite` を検索します。
3. `2D Sprite` を選んで右下の `Install` を押します。

**UIの階層（親子関係）を作って配置する** リロード中はサークルの下にテキストも一緒に出すため、全体をまとめる「親オブジェクト」を作ります。これを作っておくと、表示・非表示のプログラムがとても簡単になります。
1. Projectウィンドウの `MyProject` に `Sprites` というフォルダを作成します。
2. **今作ったSpritesフォルダ内で右クリック** ＞ `Create > 2D > Sprites > Circle` で真っ白な円画像を作ります。
3. HierarchyのCanvasの中で右クリック ＞ `Create Empty` を選び、空のオブジェクトを作って名前を `ReloadUI` にします。（これが全体をまとめる親玉になります。画面の中央に配置してください）
4. **今作った `ReloadUI` の上で右クリック** ＞ `UI > Image` を作成し、名前を `ReloadCircleImage` にします。
5. `ReloadCircleImage` の `Source Image` に白円をセットし、`Image Type` を `Filled、Fill Method` を `Radial 360` にします。（`Fill Amount` を動かすと時計のように欠けるのがわかります）
6. もう一度 `ReloadUI` の上で右クリック ＞ `UI > Text - TextMeshPro` を作成します。テキストの中身を **「リロード中…」** に書き換え、サークルの下になるように配置します。

### 5-2. DOTweenのインストールと初期設定
1. Asset Storeから `DOTween (HOTween v2)` をインポートします。
2. インポートが終わると緑色のウィンドウが開きます。もし開かない場合は、上部メニューの `Tools > Demigiant > DOTween Utility Panel` を開いてください。
3. そのウィンドウの中にある `Setup DOTween...` という緑色のボタンを押して、そのまま `Apply` を押します。これをやらないとエラーになります！

### 5-3. 魔法の1行でアニメーションさせる！
PlayerController にリロードUIの枠を追加し、リロード処理を書き換えます。
**ファイル名： `PlayerController.cs`**
``` diff
using Core.Interface;
using Core.MasterData;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using TPSRoguelite.InGame.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
+using UnityEngine.UI;
+using DG.Tweening;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        // 変数省略

        [Header("Weapon UI")]
        [SerializeField] private TextMeshProUGUI fireModeText;
        [SerializeField] private TextMeshProUGUI ammoText;

+       [Header("Reload UI")]
+
+       /// <summary>
+       /// リロード中のテキストと画像をまとめたオブジェクト
+       /// </summary>
+       [SerializeField] private GameObject reloadUI;
+
+       /// <summary>
+       /// リロード中、「あと何秒で撃てるか」が分かるサークル画像
+       /// </summary>
+       [SerializeField] private Image reloadCircleImage;

        private WeaponDataRecord currentWeapon;

        // 変数省略

        private void Start ()
        {
            gameObject.SetActive(false);
        }

        public void Setup ()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(1);
            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("currentWeaponが見つかりませんでした");
            }

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.started += OnFire;
            inputActions.Player.Fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません。");
            }

            UpdateWeaponUI();

+           if (reloadUI != null)
+           {
+               reloadUI.SetActive(false);
+           }

            gameObject.SetActive(true);
        }

        private void OnEnable ()
        {
            if (inputActions != null)
            {
                inputActions.Enable();
            }
        }

        // 関数省略

        private void OnReload (InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == currentWeapon.MaxAmmo)
            {
                return;
            }

            Reload();
        }

        private void Reload()
        {
            isReloading = true;

            if (reloadUI != null)
            {
                reloadUI.SetActive(true);
            }

            if (reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = 0f;
            }

            DOVirtual.Float(0f, 1f, currentWeapon.ReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);
        }

        private void DrawLaserPointer ()
        {
            if (laserLineRenderer == null || weponOrigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOrigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }

        // 関数省略

        private void UpdateCurrentAmmoUI()
        {
            if (ammoText != null)
            {
                ammoText.SetText($"{CurrentAmmo}/{currentWeapon.MaxAmmo}");
            }
        }

+       /// <summary>
+       /// レベルアップの文字を数秒間だけ表示して自動で消す
+       /// </summary>
+       private void UpdateReloadUI(float value)
+       {
+           if (reloadCircleImage != null)
+           {
+               reloadCircleImage.fillAmount = value;
+           }
+       }
+
+       /// <summary>
+       /// レベルアップの文字を数秒間だけ表示して自動で消す
+       /// </summary>
+       private void FinishReload()
+       {
+           if (reloadUI != null)
+           {
+               reloadUI.SetActive(false);
+           }
+
+           CurrentAmmo = currentWeapon.MaxAmmo;
+           UpdateCurrentAmmoUI();
+           isReloading = false;
+       }
    }
}
```
最後に、PlayerControllerの枠に ReloadUIObject と ReloadCircleImage をセットすれば、成長と手触りを兼ね備えた最強のゲームシステムが完成です！