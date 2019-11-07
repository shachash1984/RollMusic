using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePanel : MonoBehaviour {

    public Note note;
    public string fileName;
    public string directoryPath;
    public int index;
    private Dropdown octaveDropdown;
    private Dropdown soundDropdown;
    private InputField scoreInputField;
    private InputField penaltyInputField;
    private InputField speedInputField;
    private Dropdown columnDropdown;
    public Dropdown lengthDropdown;
    public const float TILE_SIZE = 5f;
    public const float WAIT_TIME = 3.5f;

    private void Awake()
    {
        Init();
    }

    private void OnMouseDown()
    {
        ControlPanelManager.S.SelectPanel(this);
    }

    private void Init()
    {
        octaveDropdown = transform.GetChild(0).GetComponent<Dropdown>();
        soundDropdown = transform.GetChild(2).GetComponent<Dropdown>();
        scoreInputField = transform.GetChild(4).GetComponent<InputField>();
        penaltyInputField = transform.GetChild(6).GetComponent<InputField>();
        speedInputField = transform.GetChild(8).GetComponent<InputField>();
        columnDropdown = transform.GetChild(10).GetComponent<Dropdown>();
        lengthDropdown = transform.GetChild(12).GetComponent<Dropdown>();
    }

    public void AssignInfo()
    {
        /*switch (lengthDropdown.value)
        {
            case 1:
                note.length = TILE_SIZE * 1.5f;
                break;
            case 2:
                note.length = TILE_SIZE * 2f;
                break;
            case 3:
                note.length = TILE_SIZE * 2.5f;
                break;
            case 4:
                note.length = TILE_SIZE * 3f;
                break;
            default:
                note.length = TILE_SIZE;
                break;
        }*/
        /////Needs to be tested
        note.length = float.Parse(lengthDropdown.options[lengthDropdown.value].text) * TILE_SIZE;
        //////////
        if (index == 0)
            note.tileTime = 1;
        else
        {
            if (ControlPanelManager.S.song.notes[index - 1].length <= note.length)
                note.tileTime = ControlPanelManager.S.song.notes[index - 1].tileTime + ((1f / WAIT_TIME) * (ControlPanelManager.S.song.notes[index - 1].length / TILE_SIZE));// + (note.length - ControlPanelManager.S.song.notes[index - 1].length) / note.length / TILE_SIZE);
            else if (ControlPanelManager.S.song.notes[index - 1].length > note.length)
                note.tileTime = ControlPanelManager.S.song.notes[index - 1].tileTime + ((1f / WAIT_TIME) * (ControlPanelManager.S.song.notes[index - 1].length / TILE_SIZE));// - (ControlPanelManager.S.song.notes[index - 1].length - note.length) / ControlPanelManager.S.song.notes[index - 1].length / TILE_SIZE);
        }
        note.octave = int.Parse(octaveDropdown.options[octaveDropdown.value].text);
        note.sound = soundDropdown.value;
        note.soundName = string.Format("{0}{1}",soundDropdown.options[soundDropdown.value].text, octaveDropdown.options[octaveDropdown.value].text);
        note.rewardValue = int.Parse(scoreInputField.text);
        note.penaltyValue = int.Parse(penaltyInputField.text);
        note.speed = int.Parse(speedInputField.text);
        note.column = columnDropdown.value;
    }

    public void DisplayInfo()
    {
        octaveDropdown.value = note.octave - 1;
        soundDropdown.value = note.sound;
        scoreInputField.text = note.rewardValue.ToString();
        penaltyInputField.text = note.penaltyValue.ToString();
        speedInputField.text = note.speed.ToString();
        columnDropdown.value = note.column + 1;
        if (note.length / TILE_SIZE == 1)
            lengthDropdown.value = 0;
        else if (note.length / TILE_SIZE == 1.5)
            lengthDropdown.value = 1;
        else if (note.length / TILE_SIZE == 2)
            lengthDropdown.value = 2;
        else if (note.length / TILE_SIZE == 2.5)
            lengthDropdown.value = 3;
        else if (note.length / TILE_SIZE == 3)
            lengthDropdown.value = 4;
    }

    public void DisableFields()
    {
        octaveDropdown.interactable = false;
        soundDropdown.interactable = false;
        scoreInputField.interactable = false;
        speedInputField.interactable = false;
        columnDropdown.interactable = false;
        lengthDropdown.interactable = false;
    }

    public void ChangeColor(Color c)
    {
        GetComponent<Image>().color = c;
    }
}

[System.Serializable]
public class Song
{
    public string songName;
    public int maxScore;
    [SerializeField] public List<Note> notes = new List<Note>();
}
