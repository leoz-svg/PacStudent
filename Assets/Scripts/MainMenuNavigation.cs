using UnityEngine;
using UnityEngine.EventSystems;
public class MainMenuNavigation:MonoBehaviour {
 public GameObject homePage,levelPage;
 public UnityEngine.UI.Button playButton,backButton,settingsButton,firstLevelButton;
 public MusicSettingsPanel settings;
 void Awake(){playButton.onClick.AddListener(OpenLevels);backButton.onClick.AddListener(Back);Show(false);}
 void Update(){if(Input.GetKeyDown(KeyCode.Escape)&&levelPage.activeSelf&&!settings.overlay.activeSelf)Back();}
 public void OpenLevels(){Show(true);}
 public void Back(){Show(false);}
 void Show(bool levels){homePage.SetActive(!levels);levelPage.SetActive(levels);if(EventSystem.current)EventSystem.current.SetSelectedGameObject(levels?firstLevelButton.gameObject:playButton.gameObject);}
 void OnDestroy(){if(playButton)playButton.onClick.RemoveListener(OpenLevels);if(backButton)backButton.onClick.RemoveListener(Back);}
}
