using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Components")]
    public PlayableDirector director;

    [Header("Character Dialogues")]
    [TextArea(2, 5)]
    public string[] thachSanhLines;

    [TextArea(2, 5)]
    public string[] lyThongLines;

    private void Start()
    {
        // For demonstration, we'll start playing the timeline if it's assigned
        // In a real game, you might trigger this from a collider or UI button
        if (director != null)
        {
            director.Play();
        }
    }

    // Called via Timeline Signal
    public void PlayThachSanhDialogue()
    {
        if (DialogueSystem.Instance != null && thachSanhLines.Length > 0)
        {
            DialogueSystem.Instance.StartDialogue(thachSanhLines);
            PauseCutscene();
        }
    }

    // Called via Timeline Signal
    public void PlayLyThongDialogue()
    {
        if (DialogueSystem.Instance != null && lyThongLines.Length > 0)
        {
            DialogueSystem.Instance.StartDialogue(lyThongLines);
            PauseCutscene();
        }
    }

    public void PauseCutscene()
    {
        if (director != null)
        {
            director.Pause();
            StartCoroutine(WaitForDialogueEnd());
        }
    }

    public void ResumeCutscene()
    {
        if (director != null)
        {
            director.Play();
        }
    }

    private IEnumerator WaitForDialogueEnd()
    {
        // Wait until the dialogue panel is deactivated (DialogueSystem handles closing it when out of sentences)
        while (DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            yield return null;
        }

        ResumeCutscene();
    }
}
