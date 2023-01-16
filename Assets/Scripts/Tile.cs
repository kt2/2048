using DG.Tweening;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


namespace game2048
{
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private GameObject _container;
        [SerializeField]
        private Image _bg;
        [SerializeField]
        private TMPro.TextMeshProUGUI _label;
        [SerializeField]
        private GameSettings _gameSettings;

        public Vector3 Position
        {
            get; set;
        }
        public int Value
        {
            get; private set;
        }

        public bool CanMerge
        {
            get; set;
        }

        public Vector3? AnimPosition
        {
            get; set;
        }

        Vector3? _newPosition = null;
        public Vector3 CurrentPosition
        {
            get
            {
                return (_newPosition == null) ? Position : (Vector3)_newPosition;
            }
            set
            {
                _newPosition = value;
            }
        }

        int? _newValue = null;

        public int? NewValue
        {
            get
            {
                return _newValue;
            }
        }

        public int CurrentValue
        {
            get
            {
                return (_newValue == null) ? Value : (int)_newValue;
            }
            private set
            {
                _newValue = value;
            }
        }

        public void SetPosition(Vector3 pos)
        {
            CurrentPosition = pos;
        }
        public void DoVanish()
        {
            Position = CurrentPosition;
            transform.position = Position;
            _container.SetActive(false);
        }

        public void SetValue(int val)
        {
            CurrentValue = val;
        }

        public void DoAnim(Sequence seq)
        {
            Sequence newSeq = DOTween.Sequence();
            if (_newPosition != null)
            {
                if (_newValue != null && _newValue == 0)
                {
                    if (AnimPosition != null)
                    {
                        newSeq.Append(transform.DOMove((Vector3)AnimPosition, _gameSettings.AnimationDuration));
                        newSeq.Append(transform.DOScale(Vector3.zero, _gameSettings.AnimationDuration).OnComplete(() => DoVanish()));
                    }
                    else
                    {
                        newSeq.Join(transform.DOScale(Vector3.zero, _gameSettings.AnimationDuration).OnComplete(() => DoVanish()));
                    }
                }
                else
                {
                    Position = CurrentPosition;
                    newSeq.Join(transform.DOMove(Position, _gameSettings.AnimationDuration));
                }
            }
            if (_newValue != null)
            {
                var oldValue = Value;

                Value = CurrentValue;

                if (Value != 0)
                {
                    newSeq.OnComplete(() => DoUpdate(oldValue, newSeq));
                }
            }
            seq.Join(newSeq);
        }

        public void PutNewValue(Sequence seq)
        {
            Value = CurrentValue;
            _label.text = Value.ToString();
            TileColor newColor = _gameSettings.TileColors.FirstOrDefault(color => color.value == Value) ?? new TileColor();
            _label.color = newColor.fgColor;
            _bg.color = newColor.bgColor;
            transform.localScale = new Vector3(0, 0, 0);
            _container.SetActive(true);
            seq.Append(transform.DOScale(new Vector3(1, 1, 1), _gameSettings.AnimationDuration));
        }

        public void DoUpdate(int oldValue, Sequence seq)
        {
            _label.text = Value.ToString();
            TileColor newColor = _gameSettings.TileColors.FirstOrDefault(color => color.value == Value) ?? new TileColor();
            _label.color = newColor.fgColor;
            _bg.color = newColor.bgColor;
            if (oldValue != 0) {
                if (Value > oldValue)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                    Sequence nSeq = DOTween.Sequence();
                    nSeq.Append(transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), _gameSettings.AnimationDuration / 2));
                    nSeq.Append(transform.DOScale(new Vector3(1, 1, 1), _gameSettings.AnimationDuration / 2));
                    nSeq.Play();
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }
            _container.SetActive(true);
        }

        public void ResetStatus()
        {
            _newPosition = null;
            _newValue = null;
            AnimPosition = null;
            CanMerge = true;
        }
    }
}
