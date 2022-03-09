﻿using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using TMPro;

namespace CleverCrow.Fluid.Dialogues.Examples
{
    public class DialogContainer : MonoBehaviour
    {
        private DialogueController _ctrl;
        private GameObject Oliver;

        [Header("DIALOGUE")]
        public DialogueGraph dialogue;
        public GameObjectOverride[] gameObjectOverrides;
        public GameObject speakerContainer;
        public Image portrait;
        public TextMeshProUGUI lines;
        public TextMeshProUGUI ActorName;
        public RectTransform choiceList;
        public ChoiceButton choicePrefab;
        public string totemLandCode;
        public string backToOliverCode;
        public View dialogCanvas;
        public View controlCanvas;
        private int organg_key;
        public int organg;
        [Header("DIALOGUE EFFECTS")]
        public float duration;
        public float strength;
        public int vibrato;
        public float randomness;
        private Animator cameraAnim;


        IEnumerator LoadLevel(string level)
        {
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(level, LoadSceneMode.Additive);
        }

        IEnumerator UnLoadLevel(string level)
        {
            yield return new WaitForEndOfFrame();
            SceneManager.UnloadSceneAsync(level, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

        private void Awake()
        {
            Load();
            var database = new DatabaseInstanceExtended();
            _ctrl = new DialogueController(database);
            _ctrl.Events.Speak.AddListener((actor, text) => {
                ClearChoices();
                ActorName.text = actor.DisplayName;
                portrait.sprite = actor.Portrait;
                lines.text = text;
                _ctrl.Next();
            });

            _ctrl.Events.Choice.AddListener((actor, text, choices) => {
                ClearChoices();
                ActorName.text = actor.DisplayName;
                portrait.sprite = actor.Portrait;
                lines.text = text;
                Tween();
                choices.ForEach(c => {
                    var choice = Instantiate(choicePrefab, choiceList);
                    choice.title.text = c.Text;
                    choice.clickEvent.AddListener(_ctrl.SelectChoice);
                });
            });

            _ctrl.Events.End.AddListener(() => {
                organg_key += 1;
                Save();
                dialogCanvas.Hide();
                StartCoroutine(DoorController.Instance.EndGame());

            });

            _ctrl.Events.NodeEnter.AddListener((node) => {
                Debug.Log($"Node Enter: {node.GetType()} - {node.UniqueId}");
                if (node.UniqueId == totemLandCode)
                {
                    StartCoroutine(LoadLevel("TotemLand"));
                }
                else if (node.UniqueId == backToOliverCode)
                {
                    StartCoroutine(UnLoadLevel("TotemLand"));
                }
            });
            _ctrl.Play(dialogue);
        }

        public void Start()
        {
            dialogCanvas.Hide();
            cameraAnim = GetComponent<Animator>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneunLoaded;
            PlayerPrefs.DeleteAll();
            if (PlayerPrefs.HasKey("organg_key" + organg_key))
            {
                Debug.Log("has " + organg_key + " key");
                Destroy(gameObject, 0.01f);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneunLoaded;
        }

        private void Tween()
        {
            portrait.GetComponent<Transform>().DOShakeScale(duration, strength, vibrato, randomness, false);
        }

        private void ClearChoices()
        {
            foreach (Transform child in choiceList)
            {
                Destroy(child.gameObject);
            }
        }

        private void Update()
        {
            _ctrl.Tick();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(LoadLevel());
        }

        private void OnSceneunLoaded(Scene scene)
        {
            StartCoroutine(unLoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            cameraAnim.SetBool("cutscene", false);
            CanvasFader.Instance.Fader(true);
            Oliver = GameObject.Find("MainPlayer");
            yield return new WaitForSeconds(2f);
            Oliver.SetActive(false);
            yield return new WaitForSeconds(1f);
            gameObject.GetComponent<Collider2D>().enabled = false;
            gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            CanvasFader.Instance.Fader(false);
        }

        private IEnumerator unLoadLevel()
        {
            CanvasFader.Instance.FadeImmediately();
            CanvasFader.Instance.Fader(true);
            yield return new WaitForSeconds(1.0f);
            cameraAnim.SetBool("cutscene", false);
            Oliver.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            CanvasFader.Instance.Fader(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                dialogCanvas.Show();
                controlCanvas.Hide();
                CountdownTimer.Instance.enabled = false;
                cameraAnim.SetBool("cutscene", true);
            }
        }

        public void Save()
        {
            PlayerPrefs.SetInt("organg_key" + organg, organg_key);
            Debug.Log(organg_key);
        }
        public void Load()
        {
            organg_key = PlayerPrefs.GetInt("organg_key" + organg);
        }
    }
}
