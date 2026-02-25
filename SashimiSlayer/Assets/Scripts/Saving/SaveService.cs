using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Saving
{
    public class SaveService
    {
        private const string SaveFileName = "SashimiSlayerSaveData.json";
        private const string SaveBackupFileName = "SashimiSlayerSaveData-Backup.json";

        private string _saveFilePath;
        private string _backupSaveFilePath;

        private SaveModel _saveModel;

        private bool _isAboutToSave;
        public SaveModel SaveModel => _saveModel;

        public float ScreenShakeRatio
        {
            get => _saveModel.ScreenShakeRatio;
            set
            {
                _saveModel.ScreenShakeRatio = value;
                Save();
            }
        }

        public float MasterVolume
        {
            get => _saveModel.MasterVolume;
            set
            {
                _saveModel.MasterVolume = value;
                Save();
            }
        }

        public float SfxVolume
        {
            get => _saveModel.SfxVolume;
            set
            {
                _saveModel.SfxVolume = value;
                Save();
            }
        }

        public float MusicVolume
        {
            get => _saveModel.MusicVolume;
            set
            {
                _saveModel.MusicVolume = value;
                Save();
            }
        }

        public float SwordAimMultiplier
        {
            get => _saveModel.SwordAimMultiplier;
            set
            {
                _saveModel.SwordAimMultiplier = value;
                Save();
            }
        }

        public float SwordAngleOffset
        {
            get => _saveModel.SwordAngleOffset;
            set
            {
                _saveModel.SwordAngleOffset = value;
                Save();
            }
        }

        public bool InvertSwordAim
        {
            get => _saveModel.InvertSwordAim;
            set
            {
                _saveModel.InvertSwordAim = value;
                Save();
            }
        }

        public int UpAxis
        {
            get => _saveModel.UpAxis;
            set
            {
                _saveModel.UpAxis = value;
                Save();
            }
        }

        public bool InvertParryDirection
        {
            get => _saveModel.InvertParryDirection;
            set
            {
                _saveModel.InvertParryDirection = value;
                Save();
            }
        }

        public string LastSerialPortName
        {
            get => _saveModel.LastConnectedSerialPortName;
            set
            {
                _saveModel.LastConnectedSerialPortName = value;
                Save();
            }
        }

        public string InputBindingOverrides
        {
            get => _saveModel.InputBindingOverrides;
            set
            {
                _saveModel.InputBindingOverrides = value;
                Save();
            }
        }

        public bool RumbleFeedbackEnabled
        {
            get => _saveModel.RumbleFeedbackEnabled;
            set
            {
                _saveModel.RumbleFeedbackEnabled = value;
                Save();
            }
        }

        public SaveService()
        {
            string persistentDataPath = Application.persistentDataPath;

            _saveFilePath = Path.Combine(persistentDataPath, SaveFileName);
            _backupSaveFilePath = Path.Combine(persistentDataPath, SaveBackupFileName);

            _saveModel = TryLoadSaveModel();
        }

        private SaveModel TryLoadSaveModel()
        {
            try
            {
                return JsonUtility.FromJson<SaveModel>(File.ReadAllText(_saveFilePath));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogWarning("Failed to load save file, loading backup instead.");

                return TryLoadBackupSaveModel();
            }
        }

        private SaveModel TryLoadBackupSaveModel()
        {
            try
            {
                return JsonUtility.FromJson<SaveModel>(File.ReadAllText(_backupSaveFilePath));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new SaveModel();
            }
        }

        private void Save()
        {
            SaveModelDelayed().Forget();
        }

        /// <summary>
        ///     Saves after a frame, to avoid many writes on the same frame (mainly when the UI loads)
        /// </summary>
        private async UniTaskVoid SaveModelDelayed()
        {
            if (_isAboutToSave)
            {
                return;
            }

            _isAboutToSave = true;

            await UniTask.DelayFrame(1);

            DoSaveImmediately();

            _isAboutToSave = false;
        }

        private void DoSaveImmediately()
        {
            Debug.Log("Saving save file...");
            try
            {
                // Backup old save file
                if (File.Exists(_saveFilePath))
                {
                    File.Copy(_saveFilePath, _backupSaveFilePath, true);
                }

                // Write new save file
                File.WriteAllText(_saveFilePath, JsonUtility.ToJson(_saveModel, true));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SetHighScore(HighScoreSaveModel model)
        {
            List<HighScoreSaveModel> highScores = _saveModel.HighScores;

            var found = false;
            for (var i = 0; i < highScores.Count; i++)
            {
                HighScoreSaveModel existingHs = highScores[i];

                if (existingHs.NameKey == model.NameKey)
                {
                    highScores[i] = model;
                    found = true;
                }
            }

            // Doesn't exist, add it in
            if (!found)
            {
                highScores.Add(model);
            }

            Save();
        }

        public bool GetHighScore(string nameKey, out HighScoreSaveModel model)
        {
            List<HighScoreSaveModel> highScores = _saveModel.HighScores;
            for (var i = 0; i < highScores.Count; i++)
            {
                HighScoreSaveModel existingHs = highScores[i];

                if (existingHs.NameKey == nameKey)
                {
                    model = existingHs;
                    return true;
                }
            }

            model = default;
            return false;
        }

        public void WipeHighScore()
        {
            Debug.Log("Wiping high scores...");
            _saveModel.HighScores.Clear();
            Save();
        }
    }
}