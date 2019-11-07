using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {


    #region Fields
    //static public DataManager S;
    private string urlLoadSong = "http://tbm2.com/music/run_music_search.php";
    #endregion

    #region Events and Delegates
    public delegate void ResponseFromServer(string hl, string content, string error);
    public static event ResponseFromServer OnResponseFromServer;
    public delegate void ReceivedSongDataFromServer(Song s);
    public static event ReceivedSongDataFromServer OnReceivedSongDataFromServer;
    #endregion

    public IEnumerator GetSongDataListFromServer(int songIndex)
    {
        string songName = "s2707";
        //string songName = "a167";
        /*switch (songIndex)
        {
            case 0:
                songName = "א";
                break;
            case 1:
                songName = "Leyenda";
                break;
            case 2:
                songName = "ff7";
                break;
            default:
                songName = "א";
                break;
        }*/
        WWWForm form = new WWWForm();
        form.AddField("songName", songName);
        WWW w = new WWW(urlLoadSong, form);
        yield return w;
        if (string.IsNullOrEmpty(w.error))
        {
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(w.text);
            if (string.IsNullOrEmpty(serverResponse.error))
            {
                Song songInfoContainer = JsonUtility.FromJson<Song>(serverResponse.song);
                if (OnReceivedSongDataFromServer != null)
                {
                    // OnReceivedSongDataFromServer(songInfoContainer);

                    ///////////
                    //TESTING
                    //Song testSong = GetTestSong();
                    Song testSong2 = GetExampleSong();
                    OnReceivedSongDataFromServer(testSong2);
                    //////////
                }

            }
            else
            {
                if (OnResponseFromServer != null)
                    OnResponseFromServer("תקלה במציאת שיר", "שם שיר לא קיים", serverResponse.error);
                Debug.Log(serverResponse.error);
            }
        }
        else
        {
            OnResponseFromServer("תקלת תקשורת", "חיבור לשרת נכשל", w.error);
            Debug.Log(w.error);
        }
            
    }

    /////////////////////////////////
    ////TEST
    Song GetTestSong()
    {
        Song testSong = new Song();
        testSong.songName = "qwerty";
        testSong.maxScore = 30;
        for (int i = 0; i < testSong.maxScore; i++)
        {
            testSong.notes.Add(GenerateRandomNote(testSong.notes, i));
        }
        return testSong;
    }
    
    Note GenerateRandomNote(List<Note> noteList, int index)
    {
        Note n = new Note();
        float[] lengths = new float[] { 4f, 6f, 8f, 10f, 12f };
        n.length = lengths[Random.Range(0, 3)];
        if (index == 0)
            n.tileTime = 1;
        else
        {
            if (noteList[index - 1].length <= n.length)
                n.tileTime = noteList[index - 1].tileTime + ((1f / 4f) * (noteList[index - 1].length / 4f));// + (n.length - noteList[index - 1].length) / n.length / 4f);
            else if (noteList[index - 1].length > n.length)
                n.tileTime = noteList[index - 1].tileTime + ((1f / 4f) * (noteList[index - 1].length / 4f));// - (noteList[index - 1].length - n.length) / noteList[index - 1].length / 4f);
        }
            
        
            
        //n.soundName = Random.Range(0, 7);
        n.rewardValue = 1;
        n.penaltyValue = 1;
        n.speed = 16;
        n.column = Random.Range(0, 4);
       
        return n;
    }

    public Song GetExampleSong()
    {
        Song s = new Song();
        s.songName = "Fur Elise";
        s.maxScore = 100;

        float LENGTH = NotePanel.TILE_SIZE;

        s.notes.Add(new Note("E5", 2, LENGTH*1f, 0f, 0f));
        s.notes.Add(new Note("D#5", 1, LENGTH * 1f, s.notes[s.notes.Count-1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("G#4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 1, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 0, LENGTH * 3f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));

        s.notes.Add(new Note("E5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("G#4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D#5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("D5", 3, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 1, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 3, LENGTH * 2f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("E4", 0, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("C5", 2, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("B4", 1, LENGTH * 1f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        s.notes.Add(new Note("A4", 0, LENGTH * 3f, s.notes[s.notes.Count - 1].length, s.notes[s.notes.Count - 1].tileTime));
        return s;
    }
    /////////////////////////////////
}



