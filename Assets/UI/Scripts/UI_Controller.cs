using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject HudPanel;
    public GameObject PausePanel;
    public GameObject UpgradePanel;
    public GameObject RecapPanel;
    public bool isMulitplayer = true;
    public Text TimeText;
    public Text healthText;
    public Text GoalText;
    public Text ammoText;
    public Text UpgradeText;
    public Text ArmorText;
    public Text TotalScrapText;
    public Text ScrapDroppedText;
    public Text ScoreText;
    public Slider slider;
    public Text VolumeText;
    int TotalScrap;
    int DroppedScrap;
    float FinalScore;
    bool pausetime = false;
    public float time = 600.0f;
    public GameObject Player;
    float playerHealth;
    float playerReserveAmmo;
    float playerAmmoInGun;
    float playerScrap;
    int UpgradePoints;
    int playerArmor;
    bool GameOver = false;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!PausePanel.activeSelf)
                UpgradePanel.SetActive(true);
            if (Player.GetComponent<ShepardController>().isWeaponEquiped)
                Player.GetComponentInChildren<GunController>().enabled = false;
        }
        if (UpgradePanel.activeSelf) { 
        
        }
            if (Input.GetKeyDown("escape"))
        {
            if (!UpgradePanel.activeSelf)
                Pause();
            else {
                UpgradePanel.SetActive(false);
                Resume();
            }
        }
        if (!pausetime || isMulitplayer) {
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            else {
                if(!GameOver)
                    DisplayStats();
            }
        }

    }

    private void FixedUpdate()
    {
        playerArmor = Player.GetComponent<PlayerStats>().armor;
        playerHealth = Player.GetComponent<PlayerStats>().currentHealth;
        playerReserveAmmo = Player.GetComponent<PlayerStats>().reserveAmmo;
        playerAmmoInGun = Player.GetComponent<PlayerStats>().ammoInGun;
        playerScrap = Player.GetComponent<PlayerStats>().scrapCount;
        Player.GetComponent<PlayerStats>().CalculateUpgradePoints();
        UpgradePoints = Player.GetComponent<PlayerStats>().upgradePoints;

        TimeText.text = "Time Remaining: " + time.ToString("0.0");
        ammoText.text = "Ammo: " + playerAmmoInGun.ToString() + "/" + playerReserveAmmo.ToString();
        GoalText.text = "Scrap: " + playerScrap.ToString();
        healthText.text = "Health: " + playerHealth.ToString();
        UpgradeText.text = "Upgrade Points: " + UpgradePoints.ToString();
        ArmorText.text = "Armor: " + playerArmor.ToString();

        if (playerHealth <= 0) {
            Player.GetComponent<PlayerStats>().Respawn(Player.transform.position);
        }
    }

    private void Pause()
    {
        if (!pausetime) {
            if (Player.GetComponent<ShepardController>().isWeaponEquiped)
                Player.GetComponentInChildren<GunController>().enabled = false;
            pausetime = true;
            PausePanel.SetActive(true);
        }
    }

    public void UpgradeGun() {
        if (UpgradePoints > 0) {
            Player.GetComponent<PlayerStats>().UpgradeWeapon();
            Player.GetComponent<PlayerStats>().scrapCount = Player.GetComponent<PlayerStats>().scrapCount - 10;
        }
    }
    public void UpgradeHealth() {
        if (UpgradePoints > 0)
        {
            Player.GetComponent<PlayerStats>().IncreaseHealth();
            Player.GetComponent<PlayerStats>().scrapCount = Player.GetComponent<PlayerStats>().scrapCount - 10;
        }
    }
    public void UpgradeSpeed()
    {
        if (UpgradePoints > 0)
        {
            Player.GetComponent<PlayerStats>().IncreaseSpeed();
            Player.GetComponent<PlayerStats>().scrapCount = Player.GetComponent<PlayerStats>().scrapCount - 10;
        }
    }
    public void UpgradeArmor() {

            Player.GetComponent<PlayerStats>().MakeArmor();

    }

    public void DisplayStats() {
        GameOver = true;
        RecapPanel.SetActive(true);
        TotalScrap = Player.GetComponent<PlayerStats>().totalScrap;
        DroppedScrap = Player.GetComponent<PlayerStats>().ScrapDropped;
        Player.GetComponent<PlayerStats>().UpdateScore();
        FinalScore = Player.GetComponent<PlayerStats>().score;
        Player.GetComponentInChildren<GunController>().enabled = false;
        Player.GetComponentInChildren<ShepardController>().enabled = false;

        ScrapDroppedText.text = "Total Scrap Dropped: " + DroppedScrap.ToString();
        TotalScrapText.text = "Total Scrap Collected: " + TotalScrap.ToString();
        ScoreText.text = "Final Score: " + FinalScore.ToString("0.00");
    
    }
    public void Resume()
    {
        if(Player.GetComponent<ShepardController>().isWeaponEquiped)
             Player.GetComponentInChildren<GunController>().enabled = true;
        UpgradePanel.SetActive(false);
        PausePanel.SetActive(false);
        pausetime = false;
    }

    public void UpdateVolume() {
        AudioListener.volume = slider.value;
        VolumeText.text = slider.value.ToString("0.00");
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = (false);
#else
        Application.Quit();
#endif   
    }
    public void ToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
