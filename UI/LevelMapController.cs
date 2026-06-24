using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Quản lý màn chọn level: áp trạng thái khoá/mở theo PREREQUISITE của từng node,
    /// xử lý chọn node (chỉ 1 node được chọn), và nút PLAY load đúng level vào Combat scene.
    /// </summary>
    public class LevelMapController : MonoBehaviour
    {
        [Tooltip("Để trống = tự gom tất cả LevelNode trong children.")]
        [SerializeField] private List<LevelNode> nodes = new();
        [SerializeField] private Button playButton;
        [Tooltip("Tên Combat scene (PHẢI thêm vào Build Settings).")]
        [SerializeField] private string combatSceneName = "Combat";
        [Tooltip("Tự chọn sẵn 1 node có thể chơi khi vào map.")]
        [SerializeField] private bool autoSelect = true;

        private LevelNode _selected;

        private void Start()
        {
            if (nodes == null || nodes.Count == 0)
                nodes = new List<LevelNode>(GetComponentsInChildren<LevelNode>(true));

            LevelNode firstUnlocked = null;
            LevelNode firstPlayable = null; // mở nhưng chưa qua = "nên chơi tiếp"

            foreach (var node in nodes)
            {
                if (node == null) continue;
                node.Bind(this);

                if (node.PrerequisitesMet())
                {
                    bool done = LevelProgress.IsCompleted(node.LevelId);
                    node.SetUnlocked(done);
                    if (firstUnlocked == null) firstUnlocked = node;
                    if (!done && firstPlayable == null) firstPlayable = node;
                }
                else
                {
                    node.SetLocked();
                }
            }

            if (playButton != null) playButton.onClick.AddListener(OnPlay);

            var initial = firstPlayable != null ? firstPlayable : firstUnlocked;
            if (autoSelect && initial != null) OnNodeClicked(initial);
            else UpdatePlayButton();
        }

        /// <summary>Gọi từ LevelNode khi bị bấm.</summary>
        public void OnNodeClicked(LevelNode node)
        {
            if (node == null || node.Current == LevelNode.State.Locked) return;

            if (_selected != null && _selected != node) _selected.Deselect();
            _selected = node;
            _selected.SetSelected();
            UpdatePlayButton();
        }

        private void UpdatePlayButton()
        {
            if (playButton != null) playButton.interactable = _selected != null;
        }

        private void OnPlay()
        {
            if (_selected == null) return;
            LevelSession.Set(_selected.LevelId, _selected.Wave);
            SceneManager.LoadScene(combatSceneName);
        }
    }
}
