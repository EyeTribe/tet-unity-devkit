/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EyeTribe.Unity
{
    /// <summary>
    /// Handles loading of scenes in a push-pop stack manner. Class is Singleton and will automatically create a GO for itself 
    /// once instanciated.
    /// </summary>
    public class LevelManager : UnitySingleton<LevelManager>
    {
        private Stack<string> _Scenes = new Stack<string>();

        // Ensure non-instantiation
        protected LevelManager() { }

        public void LoadNextLevel(String levelName)
        {
            if (CheckSceneExists(levelName))
            {
                _Scenes.Push(SceneManager.GetActiveScene().name);
                SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
            }
        }

        public void LoadNextLevel(int index)
        {
            if (CheckSceneExists(index))
            {
                _Scenes.Push(SceneManager.GetActiveScene().name);
                SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
            }
        }

        private bool CheckSceneExists(int index)
        {
            if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
                return true;
            return false;
        }

        private bool CheckSceneExists(string levelName)
        {
            Scene s;
            for (int i = SceneManager.sceneCount; --i >= 0; )
            {
                s = SceneManager.GetSceneAt(i);
                if (s.name.CompareTo(levelName) == 0)
                    return true;
            }
            return false;
        }

        public void LoadPreviousLevelOrExit()
        {
            if (_Scenes.Count > 0)
            {
                string previousScene = _Scenes.Pop();
                SceneManager.LoadSceneAsync(previousScene, LoadSceneMode.Single);
            }
            else
            {
#if UNITY_EDITOR
                if (Application.isEditor)
                    UnityEditor.EditorApplication.isPlaying = false;
                else
#endif
                    Application.Quit();
            }
        }
    }
}
