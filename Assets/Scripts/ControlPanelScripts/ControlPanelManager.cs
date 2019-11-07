using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UPersian;
using System;

public class ControlPanelManager : MonoBehaviour {

    static public ControlPanelManager S;
    static NotePanel selectedPanel;
    public GameObject NotePanelPrefab;
    public Song song;
    public List<NotePanel> notePanels = new List<NotePanel>();
    public InputField songNameInputField;
    public InputField songMaxScoreInputField;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private GameObject _messagePanel;
    [SerializeField] private Text _messageHeadline;
    [SerializeField] private Text _messageContent;
    [SerializeField] private Text _errorContent;
    [SerializeField] private Text _noteCounter;

    private string songName;
    private string urlSaveNewSong = "http://tbm2.com/music/run_music_register.php";//"https://dawntaylorgames.com/runmusic/run_music_register.php";//
    private string urlLoadSong = "http://tbm2.com/music/run_music_search.php";//"https://dawntaylorgames.com/runmusic/run_music_search.php";//
    private string urlUpdateSong = "http://tbm2.com/music/run_music_update.php";//"https://dawntaylorgames.com/runmusic/run_music_update.php";//



    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        DismissMessage();
    }

    private void Start()
    {
        UpdateNoteCounter();
    }

    private void DisplayMessage(string hl, string content, string error = "")
    {
        _messageHeadline.text = Reverse(hl);
        _messageContent.text = Reverse(content);
        _errorContent.text = error;
        _messagePanel.SetActive(true);
    }

    public static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public void DismissMessage()
    {
        _messagePanel.SetActive(false);
    }

    public void SaveSongToDB()
    {
        StartCoroutine(SaveSongToDBCoroutine());
    }

    IEnumerator SaveSongToDBCoroutine()
    {
        this.song = new Song();
        if (!string.IsNullOrEmpty(songNameInputField.text))
            this.song.songName = songNameInputField.text;
        else
        {
            DisplayMessage("שם ריק", "נא להזין שם");
            StopCoroutine(SaveSongToDBCoroutine());
        }
        if (!string.IsNullOrEmpty(songMaxScoreInputField.text))
            this.song.maxScore = int.Parse(songMaxScoreInputField.text);
        else
        {
            DisplayMessage("ניקוד מקסימלי ריק", "נא להזין סכום נקודות לניצחון");
            StopCoroutine(SaveSongToDBCoroutine());
        }
        string songString = "";
        foreach (NotePanel sip in notePanels)
        {
            sip.AssignInfo();
            this.song.notes.Add(sip.note);
        }
        songString = JsonUtility.ToJson(this.song);
        Debug.Log(songString);
        WWWForm form = new WWWForm();
        form.AddField("songName", this.song.songName);
        form.AddField("maxScore", this.song.maxScore);
        form.AddField("song", songString);

        WWW w = new WWW(urlSaveNewSong, form);
        yield return w;
        if (string.IsNullOrEmpty(w.error))
        {
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(w.text);
            if (string.IsNullOrEmpty(serverResponse.error))
                DisplayMessage("הצלחה", "שיר נשמר בהצלחה");
            else
                DisplayMessage("תקלת שמירה", "שיר קיים כבר");
        }
        else
            DisplayMessage("תקלת תקשורת", "אין תקשורת עם השרת", w.error);
    }

    public void LoadSongFromDB()
    {
        StartCoroutine(LoadSongFromDBCoroutine());
    }

    IEnumerator LoadSongFromDBCoroutine()
    {
        song = new Song();
        if (!string.IsNullOrEmpty(songNameInputField.text))
            songName = songNameInputField.text;
        else
        {
            DisplayMessage("שם ריק", "נא להזין שם");
            StopCoroutine(LoadSongFromDBCoroutine());
        }
        WWWForm form = new WWWForm();
        form.AddField("songName", songName);
        WWW w = new WWW(urlLoadSong, form);
        yield return w;

        if (string.IsNullOrEmpty(w.error))
        {
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(w.text);
            Debug.Log(w.text);
            if (string.IsNullOrEmpty(serverResponse.error))
            {
                song = JsonUtility.FromJson<Song>(serverResponse.song);
                Debug.Log("name: " + song.songName); //+ song.notes.Count);
                DisplayLoadedSong(); 
            }
            else
                DisplayMessage("תקלת טעינה", "שיר לא נמצא");
        }
        else
            DisplayMessage("תקלת תקשורת", "אין תקשורת עם השרת", w.error);
    }

    public void UpdateSongInDB()
    {
        StartCoroutine(UpdateSongInDBCoroutine());
    }

    IEnumerator UpdateSongInDBCoroutine()
    {
        this.song = new Song();
        if (!string.IsNullOrEmpty(songNameInputField.text))
            this.song.songName = songNameInputField.text;
        else
        {
            DisplayMessage("שם ריק", "נא להזין שם");
            StopCoroutine(SaveSongToDBCoroutine());
        }
        if (!string.IsNullOrEmpty(songMaxScoreInputField.text))
            this.song.maxScore = int.Parse(songMaxScoreInputField.text);
        else
        {
            DisplayMessage("ניקוד מקסימלי ריק", "נא להזין סכום נקודות לניצחון");
            StopCoroutine(SaveSongToDBCoroutine());
        }
        
        string songString = "";
        foreach (NotePanel sip in notePanels)
        {
            sip.AssignInfo();
            if (!this.song.notes.Contains(sip.note))
                this.song.notes.Add(sip.note);
        }
        songString = JsonUtility.ToJson(this.song);
        WWWForm form = new WWWForm();
        form.AddField("songName", this.song.songName);
        form.AddField("maxScore", this.song.maxScore);
        form.AddField("song", songString);
        Debug.Log(string.Format("songName: {0}, maxScore: {1}, song:{2}", this.song.songName, this.song.maxScore, songString));
        WWW w = new WWW(urlUpdateSong, form);
        
        yield return w;
        
        if (string.IsNullOrEmpty(w.error))
        {
            ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(w.text);
            if (string.IsNullOrEmpty(serverResponse.error))
                DisplayMessage("הצלחה", "שיר עודכן בהצלחה");
            else
                DisplayMessage("תקלת עדכון ", "שיר לא עודכן");
        }
        else
            DisplayMessage("תקלת תקשורת", "אין תקשורת עם השרת", w.error);
    }

    public void DisplayLoadedSong()
    {
        songMaxScoreInputField.text = song.maxScore.ToString();
        foreach (Note si in song.notes)
        {
            AddPanel();
            NotePanel sip = notePanels[notePanels.Count - 1];
            PopulatePanelFields(sip, si);
            sip.DisplayInfo();
        }
        Debug.Log("notePanels.Count: " + notePanels.Count);
    }

    public void AddPanel()
    {
        GameObject newPanel = Instantiate(NotePanelPrefab, _listContent.position, Quaternion.identity, _listContent);
        Vector3 wantedPos = _listContent.position;
        newPanel.transform.localPosition = wantedPos;
        NotePanel sip = newPanel.GetComponent<NotePanel>();
        notePanels.Add(sip);
        notePanels[notePanels.Count-1].index = notePanels.IndexOf(sip);
        UpdateNoteCounter();
    }

    public void PopulatePanelFields(NotePanel sip, Note si)
    {
        sip.note = si;
        sip.DisplayInfo();
    }

    public void RemoveSelectedPanel()
    {
        if (selectedPanel)
        {
            
            notePanels.Remove(selectedPanel);
            Destroy(selectedPanel.gameObject);
            selectedPanel = null;
            UpdateNoteCounter();
        }
        else
            DisplayMessage("התראה", "לא נבחר פאנל להסרה");
    }

    public void SelectPanel(NotePanel sip)
    {
        if (selectedPanel)
            selectedPanel.ChangeColor(Color.white);
        selectedPanel = sip;
        selectedPanel.ChangeColor(Color.green);
    }

    public void AllignPanel(NotePanel sip)
    {
        if (notePanels.Count > 1)
            sip.transform.localPosition = notePanels[notePanels.IndexOf(sip) - 1].transform.localPosition;
    }

    public void Clear()
    {
        foreach (NotePanel sip in notePanels)
        {
            Destroy(sip.gameObject);
        }
        notePanels.Clear();
        song = null;
        songName = "";
        songNameInputField.text = "";
        songMaxScoreInputField.text = "";
    }

    public void UpdateNoteCounter()
    {
        _noteCounter.text = notePanels.Count.ToString();
    }
}
