using System;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Hoạt ảnh bằng cách đổi sprite theo frame trên SpriteRenderer (thay cho Animator).
    /// - PlayLoop: phát lặp (vd đi bộ).
    /// - PlayOnce: phát 1 lần (vd tấn công), gọi onHit TẠI 1 frame chỉ định (gây sát thương / bắn đạn)
    ///   và onComplete khi anim kết thúc.
    /// </summary>
    public class SpriteFrameAnimator : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Sprite[] _frames;
        private float _frameTime;
        private float _timer;
        private int _index;
        private bool _loop;
        private bool _playing;

        private int _hitFrame = -1;
        private bool _hitFired;
        private Action _onHit;
        private Action _onComplete;

        private void Awake()
        {
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>Phát lặp (vd walk). Bỏ qua nếu đang lặp đúng bộ frame này (khỏi restart).</summary>
        public void PlayLoop(Sprite[] frames, float duration)
        {
            if (_playing && _loop && _frames == frames) return;
            Begin(frames, duration, true, -1, null, null);
        }

        /// <summary>Phát 1 lần (vd attack); gọi onHit tại frame hitFrame, onComplete khi xong.</summary>
        public void PlayOnce(Sprite[] frames, float duration, int hitFrame, Action onHit, Action onComplete)
        {
            Begin(frames, duration, false, hitFrame, onHit, onComplete);
        }

        public void Stop() => _playing = false;

        private void Begin(Sprite[] frames, float duration, bool loop, int hitFrame, Action onHit, Action onComplete)
        {
            _frames = frames;
            _loop = loop;
            _hitFrame = hitFrame;
            _onHit = onHit;
            _onComplete = onComplete;
            _index = 0;
            _timer = 0f;
            _hitFired = false;

            int count = frames != null ? frames.Length : 0;
            _frameTime = count > 0 ? Mathf.Max(0.0001f, duration / count) : duration;
            _playing = count > 0;

            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            if (_playing && _sr != null) _sr.sprite = frames[0];
            CheckHit(0);
        }

        private void Update()
        {
            if (!_playing) return;

            _timer += Time.deltaTime;
            while (_timer >= _frameTime)
            {
                _timer -= _frameTime;
                _index++;

                if (_index >= _frames.Length)
                {
                    if (_loop) { _index = 0; _hitFired = false; }
                    else
                    {
                        _index = _frames.Length - 1;
                        if (_sr != null) _sr.sprite = _frames[_index];
                        _playing = false;
                        _onComplete?.Invoke();
                        return;
                    }
                }

                if (_sr != null) _sr.sprite = _frames[_index];
                CheckHit(_index);
            }
        }

        private void CheckHit(int frame)
        {
            if (!_hitFired && _hitFrame >= 0 && frame >= _hitFrame)
            {
                _hitFired = true;
                _onHit?.Invoke();
            }
        }
    }
}
