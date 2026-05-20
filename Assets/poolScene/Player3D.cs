using System.Collections;
using poolScene.ItemData.WeaponData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace poolScene
{
    /// <summary>
    /// プレイヤーの移動、視点操作、射撃、被ダメージ、武器装備を担当するクラス。
    /// </summary>
    public class Player3D : MonoBehaviour
    {
        // プレイヤーの移動速度。
        public float speed = 5f;

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
        private float _timer;

        // 発射する弾を取得する弾プール。
        public BulletPool bulletPool;

        // 射撃時に再生する効果音。
        public AudioClip shotSound;

        // プレイヤーの最大HP。
        public int maxHp = 3;

        // 現在のHP。
        private int _currentHp;

        // ダメージ後の無敵時間中かどうか。
        private bool _isInvincible;

        // ダメージ後に無敵になる秒数。
        public float invincibleTime = 1f;

        // 無敵点滅で表示を切り替えるRenderer。
        private Renderer _playerRenderer;

        // 初期装備として設定する武器データ。
        public WeaponData equippedWeapon;

        // ランダム補正などを含めた現在装備中の武器性能。
        private WeaponInstance _equippedWeaponInstance;

        // カメラ上下回転の現在角度。
        private float _pitch;

        // Unity標準入力で射撃判定に使うボタン名。
        private const string FireButton = "Fire1";


        // HP、Renderer、視点、カーソル、初期武器を準備する。
        private void Start()
        {
            _currentHp = maxHp;
            GameManager3D.Instance.UpdateHp(_currentHp);
            _playerRenderer = GetComponentInChildren<Renderer>();

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

            if (equippedWeapon != null)
            {
                _equippedWeaponInstance =
                    WeaponInstance.CreateDefault(equippedWeapon);
            }
        }

        /// <summary>
        /// プレイヤーにダメージを与え、HPが0以下ならゲームオーバーにする。
        /// </summary>
        /// <param name="damage">受けるダメージ量。</param>
        public void TakeDamage(int damage)
        {
            if (_isInvincible) return;

            _currentHp -= damage;

            GameManager3D.Instance.UpdateHp(_currentHp);

            if (_currentHp <= 0)
            {
                GameManager3D.Instance.GameOver();
            }

            StartCoroutine(InvincibleCoroutine());
        }

        // 無敵時間中にRendererを点滅させ、連続ダメージを防ぐ。
        private IEnumerator InvincibleCoroutine()
        {
            _isInvincible = true;

            var elapsed = 0f;

            while (elapsed < invincibleTime)
            {
                _playerRenderer.enabled = false;

                yield return new WaitForSeconds(0.1f);

                _playerRenderer.enabled = true;

                yield return new WaitForSeconds(0.1f);

                elapsed += 0.2f;
            }

            _playerRenderer.enabled = true;

            _isInvincible = false;
        }

        // 毎フレーム、ゲーム中のみ視点操作、移動、射撃入力を処理する。
        private void Update()
        {
            if (GameManager3D.Instance.isGameOver) return;
            UpdateLook();
            var x = Input.GetAxis("Horizontal");

            var z = Input.GetAxis("Vertical");

            var move =
                transform.right * x +
                transform.forward * z;

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            transform.position += move * (speed * Time.deltaTime);

            _timer += Time.deltaTime;

            if (_equippedWeaponInstance == null ||
                !IsFirePressed() ||
                !(_timer > _equippedWeaponInstance.fireInterval)) return;

            FireWeapon();

            AudioManager.Instance.PlaySe(shotSound);

            _timer = 0f;
        }

        // 装備中の武器設定に従って1発または複数発の弾を発射する。
        private void FireWeapon()
        {
            var projectileCount =
                Mathf.Max(1, _equippedWeaponInstance.projectileCount);
            var fireTransform =
                muzzleTransform != null ? muzzleTransform :
                viewTransform != null ? viewTransform :
                transform;

            for (var i = 0; i < projectileCount; i++)
            {
                FireProjectile(
                    fireTransform,
                    GetProjectileRotation(fireTransform, i, projectileCount));
            }
        }

        // 弾プールから弾を取得し、指定された角度と武器性能で初期化して発射する。
        // fireTransform: 発射位置の基準。
        // projectileRotation: 弾の発射方向。
        private void FireProjectile(
            Transform fireTransform,
            Quaternion projectileRotation)
        {
            BulletPoolEntry bullet =
                bulletPool.GetBullet(
                    _equippedWeaponInstance.bulletData);

            if (bullet == null)
            {
                return;
            }

            bullet.bullet3D.Initialize(
                _equippedWeaponInstance.bulletData,
                _equippedWeaponInstance.damage,
                _equippedWeaponInstance.bulletSpeed,
                _equippedWeaponInstance.bulletLifetime);

            bullet.gameObject.transform.position =
                fireTransform.position + projectileRotation * Vector3.forward;

            bullet.gameObject.transform.rotation =
                projectileRotation;

            bullet.gameObject.SetActive(true);
        }

        /// <summary>
        /// 武器データから標準性能の武器インスタンスを作成して装備する。
        /// </summary>
        /// <param name="weapon">装備する武器データ。</param>
        public void EquipWeapon(WeaponData weapon)
        {
            EquipWeapon(WeaponInstance.CreateDefault(weapon));
        }

        /// <summary>
        /// 生成済みの武器インスタンスを現在の装備として設定する。
        /// </summary>
        /// <param name="weapon">装備する武器インスタンス。</param>
        public void EquipWeapon(WeaponInstance weapon)
        {
            if (weapon == null)
            {
                return;
            }

            _equippedWeaponInstance = weapon;
            equippedWeapon = weapon.data;

            Debug.Log(
                weapon.displayName +
                " equipped");
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

        // 複数弾の場合に左右へ拡散する発射角度を計算する。
        // fireTransform: 発射方向の基準。
        // projectileIndex: 発射する弾の番号。
        // projectileCount: 同時に発射する弾数。
        // 戻り値: 計算された弾の回転。
        private Quaternion GetProjectileRotation(
            Transform fireTransform,
            int projectileIndex,
            int projectileCount)
        {
            var baseRotation =
                Quaternion.LookRotation(fireTransform.forward, Vector3.up);

            if (projectileCount <= 1 ||
                _equippedWeaponInstance.spreadAngle <= 0f)
            {
                return baseRotation;
            }

            var centerOffset =
                (projectileCount - 1) * 0.5f;
            var yaw =
                (projectileIndex - centerOffset) *
                _equippedWeaponInstance.spreadAngle;

            return Quaternion.AngleAxis(yaw, Vector3.up) * baseRotation;
        }
    }
}
