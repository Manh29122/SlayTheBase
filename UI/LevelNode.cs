using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Một node level trên map (là Image). Bấm vào (nếu đã mở) sẽ được chọn và phát
    /// chuỗi 3 ảnh trắng (animation). Mở khoá theo PREREQUISITE: phải hoàn thành các node
    /// tiên quyết thì node này mới mở (hỗ trợ map rẽ nhánh).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LevelNode : MonoBehaviour, IPointerClickHandler
    {
        public enum State { Locked, Unlocked, Selected }

        [Header("Level")]
        [Tooltip("ID DUY NHẤT của level (không cần theo thứ tự). Dùng để lưu tiến độ.")]
        [SerializeField] private int levelId;
        [Tooltip("Dữ liệu wave/level của node này (truyền sang Combat scene khi PLAY).")]
        [SerializeField] private WaveDefinition wave;

        [Header("Điều kiện mở khoá")]
        [Tooltip("Các node phải HOÀN THÀNH trước thì node này mới mở. Để trống = node khởi đầu (luôn mở).")]
        [SerializeField] private List<LevelNode> prerequisites = new();
        [Tooltip("BẬT: phải qua HẾT các prerequisite. TẮT: chỉ cần qua MỘT trong số đó (cho nhánh gộp).")]
        [SerializeField] private bool requireAll = true;

        [Header("Sprites")]
        [Tooltip("Ảnh khi node ĐÃ MỞ, chưa chọn (vd kim cương xanh).")]
        [SerializeField] private Sprite normalSprite;
        [Tooltip("Ảnh khi BỊ KHOÁ (tùy chọn; để trống = dùng normal + làm mờ).")]
        [SerializeField] private Sprite lockedSprite;
        [Tooltip("Ảnh khi ĐÃ QUA (tùy chọn; để trống = dùng normal).")]
        [SerializeField] private Sprite completedSprite;
        [Tooltip("Chuỗi 3 ảnh TRẮNG phát lặp khi node được chọn.")]
        [SerializeField] private Sprite[] selectedFrames;
        [SerializeField] private float frameRate = 8f;

        public int LevelId => levelId;
        public WaveDefinition Wave => wave;
        public State Current { get; private set; }

        private Image _image;
        private LevelMapController _controller;
        private Coroutine _anim;
        private bool _completed;

        private void Awake() => _image = GetComponent<Image>();

        public void Bind(LevelMapController controller) => _controller = controller;

        /// <summary>Điều kiện tiên quyết đã thoả → node được phép mở.</summary>
        public bool PrerequisitesMet()
        {
            if (prerequisites == null || prerequisites.Count == 0) return true;

            if (requireAll)
            {
                foreach (var p in prerequisites)
                    if (p == null || !LevelProgress.IsCompleted(p.LevelId)) return false;
                return true;
            }

            foreach (var p in prerequisites)
                if (p != null && LevelProgress.IsCompleted(p.LevelId)) return true;
            return false;
        }

        public void SetLocked()
        {
            StopAnim();
            Current = State.Locked;
            if (lockedSprite != null) { _image.sprite = lockedSprite; _image.color = Color.white; }
            else { _image.sprite = normalSprite; _image.color = new Color(1f, 1f, 1f, 0.35f); }
        }

        public void SetUnlocked(bool completed)
        {
            StopAnim();
            _completed = completed;
            Current = State.Unlocked;
            _image.color = Color.white;
            _image.sprite = (completed && completedSprite != null) ? completedSprite : normalSprite;
        }

        public void SetSelected()
        {
            Current = State.Selected;
            _image.color = Color.white;
            StartAnim();
        }

        public void Deselect() => SetUnlocked(_completed);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Current == State.Locked) return;
            if (_controller != null) _controller.OnNodeClicked(this);
        }

        private void StartAnim()
        {
            StopAnim();
            if (selectedFrames != null && selectedFrames.Length > 0)
                _anim = StartCoroutine(PlayFrames());
        }

        private void StopAnim()
        {
            if (_anim != null) { StopCoroutine(_anim); _anim = null; }
        }

        private IEnumerator PlayFrames()
        {
            float dt = frameRate > 0f ? 1f / frameRate : 0.12f;
            var wait = new WaitForSeconds(dt);
            int i = 0;
            while (true)
            {
                _image.sprite = selectedFrames[i % selectedFrames.Length];
                i++;
                yield return wait;
            }
        }
    }
}
