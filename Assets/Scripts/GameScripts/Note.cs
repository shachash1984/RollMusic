using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public string error;
    public string songName;
    public int maxScore;
    public string song;
}

[System.Serializable]
public class Note  {

    public float tileTime;
    public int sound;
    public string soundName;
    public int octave;
    public int rewardValue;
    public int penaltyValue;
    public float speed;
    public int column;
    public float length;

    public Note()
    {

    }

    public Note(float _time, int _sound, string _soundName, int _octave, int _reward, int _penalty, float _speed, int _col, float _length)
    {
        tileTime = _time;
        sound = _sound;
        soundName = _soundName;
        octave = _octave;
        rewardValue = _reward;
        penaltyValue = _penalty;
        speed = _speed;
        column = _col;
        length = _length;
    }

    public Note(string _soundName, int _col, float _length, float _prevLength, float _prevTime)
    {
        length = _length;
        soundName = _soundName;
        column = _col;
        if (_prevLength == 0f)
            tileTime = 1;
        else
        {
            if (_prevLength <= length)
                tileTime = _prevTime + ((1f / NotePanel.TILE_SIZE) * (_prevLength / NotePanel.TILE_SIZE));
            else if (_prevLength > length)
                tileTime = _prevTime + ((1f / NotePanel.TILE_SIZE) * (_prevLength / NotePanel.TILE_SIZE));
        }
        rewardValue = 1;
        penaltyValue = 1;
        speed = 24.5f;
    }

    public override bool Equals(object obj)
    {
        Note n = obj as Note;
        if (n != null)
            return column == n.column && soundName == n.soundName && tileTime == n.tileTime;
        return false;
    }
    public override string ToString()
    {
        return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}", tileTime, soundName, rewardValue, penaltyValue, speed, column, length);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
