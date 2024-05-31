﻿
using System;
using UnityEngine;


namespace Runner.Player
{
    [System.Serializable]

    public class PlayerDatas
    {
        [SerializeField] private int MaxHealth = 1;
        [HideInInspector] public int CurrentHealth;

        [SerializeField] private  float StartSpeed = 5f;
        [HideInInspector] public float CurrentSpeed;

        private int CurrentScore;
        public int Score => CurrentScore;

        public event Action<int> OnScroreChange;

        public void InitPlayerDatas()
        {
            CurrentHealth = MaxHealth;
            CurrentSpeed = StartSpeed;
            CurrentScore = 0;
        }

        public void AddScore(int addValue)
        {
            CurrentScore += addValue;
            OnScroreChange?.Invoke(CurrentScore);
        }

        public void ResetSpeed()
        {
            CurrentSpeed = StartSpeed;
        }

        public void IncreaseSpeed(float value)
        {
            CurrentSpeed += value;
        }
    }
}