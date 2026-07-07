using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PianoGame
{
    /// <summary>
    /// Hoạt ảnh cho UI Image: đổi sprite tuần tự theo 1 list cho trước.
    /// Gắn lên object có <see cref="Image"/>. Đặt <see cref="duration"/> = thời gian chạy hết 1 lượt.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIAnimation : MonoBehaviour
    {
        [Tooltip("Danh sách sprite của hoạt ảnh (theo thứ tự).")]
        public List<Sprite> frames = new();
        [Tooltip("Thời gian chạy HẾT 1 lượt qua tất cả frame (giây).")]
        [Min(0.01f)] public float duration = 1f;
        [Tooltip("Lặp lại liên tục.")]
        public bool loop = true;
        [Tooltip("Tự chạy khi object bật.")]
        public bool playOnEnable = true;
        [Tooltip("Dùng thời gian KHÔNG bị Time.timeScale ảnh hưởng (UI nên bật).")]
        public bool useUnscaledTime = true;

        private Image _image;
        private int _index;
        private float _timer;
        private bool _playing;

        /// <summary>Đang chạy hay không.</summary>
        public bool IsPlaying => _playing;

        private void Awake() => _image = GetComponent<Image>();

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        /// <summary>Bắt đầu hoạt ảnh (chạy lại từ frame đầu).</summary>
        public void Play()
        {
            _index = 0;
            _timer = 0f;
            _playing = frames != null && frames.Count > 0;
            ApplyFrame();
        }

        /// <summary>Dừng tại frame hiện tại.</summary>
        public void Stop() => _playing = false;

        /// <summary>Dừng và quay về frame đầu.</summary>
        public void StopAndReset()
        {
            _playing = false;
            _index = 0;
            ApplyFrame();
        }

        /// <summary>Đổi list frame bằng code (tuỳ chọn chạy luôn).</summary>
        public void SetFrames(List<Sprite> newFrames, bool autoPlay = true)
        {
            frames = newFrames ?? new List<Sprite>();
            if (autoPlay) Play();
        }

        private void Update()
        {
            if (!_playing || frames == null || frames.Count == 0) return;

            float frameTime = duration / frames.Count; // thời gian mỗi frame
            if (frameTime <= 0f) return;

            _timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (_timer >= frameTime)
            {
                _timer -= frameTime;
                _index++;
                if (_index >= frames.Count)
                {
                    if (loop)
                    {
                        _index = 0;
                    }
                    else
                    {
                        _index = frames.Count - 1;
                        _playing = false;
                        ApplyFrame();
                        return;
                    }
                }
                ApplyFrame();
            }
        }

        private void ApplyFrame()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null || frames == null || frames.Count == 0) return;
            int idx = Mathf.Clamp(_index, 0, frames.Count - 1);
            if (frames[idx] != null) _image.sprite = frames[idx];
        }
    }
}
