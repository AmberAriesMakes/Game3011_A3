using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MatchProject
{


    public class Score : MonoBehaviour
    {
        public static Score Instance { get; private set; }


        private int _score;
        public int Scores
        {
            get => _score;
            set
            {
                if (_score == value) return;
                _score = value;
                scoretext.SetText($"Score = {_score}");
            }

        }
        [SerializeField] private TextMeshProUGUI scoretext;
        private void Awake() => Instance = this;

    }

}