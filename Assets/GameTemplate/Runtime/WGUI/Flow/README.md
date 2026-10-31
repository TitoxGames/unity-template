# WGUI Flow — Luồng UI dùng lại cho game puzzle

Điều hướng màn hình tập trung một chỗ. Màn hình chỉ **yêu cầu** (`UIFlow.Play()`, `UIFlow.Retry()`…);
`UIFlowController` quyết định ẩn/hiện màn nào, chạy transition và gọi game qua `IGameFlowHooks`.

```
Loading ──► Home ──Play──► Gameplay ──Win()──► Win ──Next──► Gameplay
              ▲               │  ▲                │
              │               │  └────Retry───────┤
              │               └──Lose()──► Lose ──┘ (Retry)
              └──────────────Home──────────────────┘
```

| Request | Cho phép từ | Transition | Hook gọi khi màn hình bị che |
|---|---|---|---|
| `Play()` | Home | có | `StartLevel()` |
| `Retry()` | Gameplay, Win, Lose | có | `RetryLevel()` |
| `Next()` | Win | có | `NextLevel()` |
| `Home()` | Gameplay, Win, Lose | có | `QuitLevel()` |
| `Win()` / `Lose()` | Gameplay | không | — |

Mỗi request trả `false` khi bị từ chối (sai state hoặc đang có transition) để nút bấm tự bật lại.
Chỉ một request chạy tại một thời điểm (`UIFlow.IsBusy`).

## Thành phần

| Loại | File | Vai trò |
|---|---|---|
| Điều phối | `UIFlowController`, `UIFlow` | State machine + static API cho màn hình và gameplay. |
| Cấu hình | `UIFlowConfig` (`Create > WGUI > UI Flow Config`) | Loading screen, transition screen, danh sách màn hình mỗi state. |
| Điểm nối game | `IGameFlowHooks` | Start / Retry / Next / Quit level, level hiện tại, bỏ qua Home lúc mở app. |
| | `LevelManagerFlowHooks` | Cài đặt mặc định trên `WPuzzle.LevelManager`. |
| | `IUIFeedback` → `UIFeedback.Current` | Âm click và haptic. |
| | `IAdsGate` → `UIAds.Current` | Rewarded / interstitial. |
| Màn hình gốc | `LoadingScreenBase`, `IrisTransitionScreen`, `HomeScreenBase`, `WinScreenBase`, `LoseScreenBase`, `SettingScreenBase` | Dùng thẳng hoặc kế thừa. |
| Component | `../Components` | `SettingToggle`, `ButtonContentTint`, `PanelAnimation`, `StaggeredReveal`, `IrisHole`, `MultiplierSlider`, `ScaleToCoverParent`, `ExtendToScreenBottom`, `BackgroundFullScreen`, `RewardIconRiseEffect`. |

## Tích hợp vào game mới

1. **Prefab màn hình**: mỗi màn là prefab có component kế thừa `UIBase` (dùng thẳng các lớp `*Base` hoặc subclass),
   đánh dấu Addressable và thêm vào `UIConfigSO` của `UIManager`.
   - Transition screen: `ShowAnimationType` / `HideAnimationType` = `None`, `SortingOrder` cao (vd. 5000).
   - Mỗi lớp gốc chỉ được có **một** subclass trong `UIConfigSO`; khi đó `UIManager.Show<WinScreenBase>()`
     tự mở subclass của game.
2. **UIFlowConfig**: tạo asset, gán `Loading Screen`, `Transition Screen`, và danh sách màn hình cho từng state, ví dụ:

   | State | Screens |
   |---|---|
   | Home | HomeScreen, TopBar, HomeBottom |
   | Gameplay | PlayScreen, TopBar |
   | Win | PlayScreen, TopBar, WinScreen |
   | Lose | PlayScreen, LoseScreen |

   Màn hình có ở cả state cũ và mới sẽ **giữ nguyên** (không nháy). Popup mở ngoài flow (Settings, Shop) tự đóng
   khi đổi state nếu bật `Close Popups On State Change`.
3. **Scene**: thêm GameObject có `UIFlowController` (gán config + hooks). `Run On Start` bật thì flow tự chạy Loading → Home.
4. **Hooks**: dùng `LevelManagerFlowHooks` nếu game dùng `LevelManager`, hoặc tự viết:

   ```csharp
   public sealed class MyGameFlowHooks : IGameFlowHooks
   {
       public bool SkipHomeOnLaunch => Save.ClearedLevels == 0;
       public int CurrentLevel => Save.CurrentLevel;
       public void StartLevel() => LevelController.Instance.Load(CurrentLevel);
       public void RetryLevel() => LevelController.Instance.Reload();
       public void NextLevel() => LevelController.Instance.Load(CurrentLevel);
       public void QuitLevel() => LevelController.Instance.Unload();
   }
   // Khởi động (trước khi UIFlowController.Start chạy):
   UIFlow.SetHooks(new MyGameFlowHooks());
   ```
5. **Feedback & Ads** (tùy chọn, không gán thì im lặng / không có quảng cáo):

   ```csharp
   UIFeedback.Current = new MyUIFeedback();   // PlayClick → SoundManager, PlayHaptic → plugin rung (tự kiểm tra Setting Haptic)
   UIAds.Current = new MyAdsGate();           // bọc AdsService của game
   ```
6. **Gameplay báo kết quả**: lưu phần thưởng xong thì gọi `UIFlow.Win()`; thua thì `UIFlow.Lose()`.

## Mở rộng màn hình

```csharp
public class MyWinScreen : WinScreenBase
{
    [SerializeField] private CoinFlyEffect coinFly;

    // Chạy trước interstitial và trước khi chuyển level.
    protected override UniTask BeforeContinueAsync() => coinFly.PlayAsync(...);
}
```

- `HomeScreenBase.RefreshLevel(int)` — đổi cách hiển thị level / nút Play theo độ khó.
- `WinScreenBase.Continue()` — gọi từ nút phụ (vd. nhân thưởng bằng rewarded ad xong thì tiếp tục).
- `SettingScreenBase.SetLevelButtonsVisible(bool)` — thêm nút riêng (Restore Purchases…).
- `LoadingScreenBase.WaitUntilReadyAsync` — chờ SDK / preload thật trước khi rời Loading.
- Lắng nghe đổi state: `EventBus<UIFlowStateChangedEvent>.Subscribe(...)`.

## Lưu ý

- Hook được gọi **trong lúc transition che màn hình** — load / unload level ở đó để người chơi không thấy.
- Không gọi `UIManager.Show` cho màn hình thuộc state từ code màn hình khác; hãy đổi `UIFlowConfig` hoặc gọi request.
  Popup độc lập (Settings, Shop, xác nhận…) vẫn mở bằng `UIManager.Show<T>()` như cũ.
- Các component trong `Components` giữ nguyên GUID từ dự án Cats In Crates: khi một game chuyển sang dùng bản trong
  template, **xóa bản copy trong game cùng lúc** để không trùng GUID.
