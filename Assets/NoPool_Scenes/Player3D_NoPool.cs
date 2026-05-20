using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// オブジェクトプールを使わずにプレイヤー移動、視点操作、弾生成を行う比較用プレイヤークラス。
/// </summary>
public class Player3D_NoPool : MonoBehaviour
{
    // プレイヤーの移動速度。
    public float speed = 5f;

    // 発射時に毎回Instantiateする弾Prefab。
    public GameObject bulletPrefab;

    // 上下の視点回転を行うカメラまたは表示用Transform。
    public Transform viewTransform;

    // 弾を発射する位置。未設定の場合は視点またはプレイヤー本体を使う。
    public Transform muzzleTransform;

    // マウス入力による視点移動の感度。
    public float mouseSensitivity = 2f;

    // ゲームパッド右スティックによる視点移動の感度。
    public float controllerLookSensitivity = 120f;

    // 開始時にカーソルを画面中央に固定するかどうか。
    public bool lockCursor = true;

    // 射撃間隔を測るための経過時間。
    float timer;

    // カメラ上下回転の現在角度。
    private float _pitch;

    // 弾を発射できる間隔。
    public float interval = 0.3f;

    // Unity標準入力で射撃判定に使うボタン名。
    private const string FireButton = "Fire1";

    // 視点Transformとカーソル固定を初期化する。
    private void Start()
    {
        if (viewTransform == null &&
            Camera.main != null)
        {
            viewTransform = Camera.main.transform;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 毎フレーム、視点操作、移動、射撃入力を処理する。
    void Update()
    {
        UpdateLook();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        transform.position += move * (speed * Time.deltaTime);

        timer += Time.deltaTime;

        if (IsFirePressed() && timer > interval)
        {
            var fireTransform =
                muzzleTransform != null ? muzzleTransform :
                viewTransform != null ? viewTransform :
                transform;

            Instantiate(
                bulletPrefab,
                fireTransform.position + fireTransform.forward,
                Quaternion.LookRotation(fireTransform.forward, Vector3.up));

            timer = 0f;
        }
    }

    // キーボード、標準入力、ゲームパッドのいずれかで射撃入力が押されているかを返す。
    private static bool IsFirePressed()
    {
        return Input.GetKey(KeyCode.Space) ||
               Input.GetButton(FireButton) ||
               Input.GetKey(KeyCode.JoystickButton0) ||
               Input.GetKey(KeyCode.JoystickButton5) ||
               Input.GetKey(KeyCode.JoystickButton7);
    }

    // 横方向はプレイヤー本体、縦方向は視点Transformを回転させる。
    private void UpdateLook()
    {
        var look = GetLookInput();

        transform.Rotate(Vector3.up, look.x, Space.World);

        if (viewTransform == null)
        {
            return;
        }

        _pitch = Mathf.Clamp(_pitch - look.y, -80f, 80f);
        viewTransform.localRotation =
            Quaternion.Euler(_pitch, 0f, 0f);
    }

    // マウスとゲームパッド右スティックから視点移動量を取得する。
    private Vector2 GetLookInput()
    {
        var look =
            new Vector2(
                Input.GetAxis("Mouse X") * mouseSensitivity,
                Input.GetAxis("Mouse Y") * mouseSensitivity);

#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
        {
            look += Gamepad.current.rightStick.ReadValue() *
                    (controllerLookSensitivity * Time.deltaTime);
        }
#endif

        return look;
    }
}
