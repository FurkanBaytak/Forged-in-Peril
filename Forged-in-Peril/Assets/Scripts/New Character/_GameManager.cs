using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _GameManager : MonoBehaviour
{
    public _Character selectedCharacter;
    public _Enemy enemyCharacter;
    public _Character playerCharacter;
    public _weapon enemyWeapon;
    public _CharacterTalent enemyTalent;
    public GameObject[] prefabs;
    public BattleAreaManager battleAreaManager;
    public DeathManager deathManager;

    public List<_WeaponMaterial> newWeaponMaterials;
    public List<_weapon> newWeapon;
    public int campIndex = -1;
    public int fightIndex = -1;
    public int endGameIndex = 55;

    public List<_WeaponMaterial> unlockedMaterials;
    public List<_weapon> unlockedWeapon;

    public _SelectionManager selectionManager;

    private bool isPlayerTurn = true;
    private bool isGameOver = false;
    public bool isReturning = false;
    public bool diedAndRestarted = false;

    [Header("Game Difficulty")]
    public int enemydiff = 0;
    public _weapon[] enemydiffweapons;
    public int gameDiff = 0;
    public Button EasyButton;
    public Button HardButton;

    [Range(1f, 10f)]
    public float gameSpeed = 1f;

    public static _GameManager Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;

    [Header("Character Health Bars")]
    public HealthBar characterHealthBar;
    public HealthBar enemyHealthBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        selectionManager = FindObjectOfType<_SelectionManager>();

        foreach (var mat in selectionManager.AvailableMaterials)
        {
            if (mat.type != _WeaponMaterial.weaponMaterialType.Iron)
            {
                mat.isLocked = true;
            }
        }
        foreach (var wpn in selectionManager.AvailableWeapons)
        {
            if (wpn.type != _weapon.WeaponType.Longsword && wpn.type != _weapon.WeaponType.Dagger)
            {
                wpn.isLocked = true;
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PreperationScene")
        {
            EasyButton.interactable = false;
            HardButton.interactable = true;
            selectionManager = FindObjectOfType<_SelectionManager>();
            Debug.Log("Gamemanager Not Active");
        }
        else if (scene.name == "BattleScene")
        {
            GameObject characterUIObj = GameObject.Find("PlayerBar");
            GameObject enemyUIObj = GameObject.Find("EnemyBar");
            if (characterUIObj != null)
            {
                characterHealthBar = characterUIObj.GetComponent<HealthBar>();
            }
            else
            {
                Debug.LogError("CharacterUI not found in BattleScene");
            }

            if (enemyUIObj != null)
            {
                enemyHealthBar = enemyUIObj.GetComponent<HealthBar>();
            }
            else
            {
                Debug.LogError("EnemyUI not found in BattleScene");
            }

            StartCoroutine(startBattle());
        }
        else if (scene.name == "MapScene")
        {
            Debug.Log("MapScene loaded");
        }
        else
        {
            Debug.Log("Gamemanager Not Active");
        }
    }

    private IEnumerator startBattle()
    {
        yield return new WaitForSeconds(0.5f);
        _Character character = _GameManager.Instance.selectedCharacter;
        battleAreaManager = FindAnyObjectByType<BattleAreaManager>();
        deathManager = FindAnyObjectByType<DeathManager>();
        CreateEnemy();
        CreateCharacter();
        ResetCharacterStats(character);

        if (character != null && enemyCharacter != null)
        {
            Debug.Log("---------------CHARACTER-----------------");
            character.DisplayCharacterStats();
            Debug.Log("---------------ENEMY-----------------");
            enemyCharacter.DisplayCharacterStats();

            if (characterHealthBar != null)
            {
                characterHealthBar.Initialize(character.Health, character.MaxHealth, character.characterName);
            }
            else
            {
                Debug.LogWarning("Character Health Bar is not assigned.");
            }

            if (enemyHealthBar != null)
            {
                enemyHealthBar.Initialize(enemyCharacter.Health, enemyCharacter.MaxHealth, enemyCharacter.characterName);
            }
            else
            {
                Debug.LogWarning("Enemy Health Bar is not assigned.");
            }

            if (LogManager.Instance != null)
            {
                LogManager.Instance.ClearLogs();
            }
        }
        else
        {
            Debug.LogWarning("No character found in GameManager!");
        }
        isGameOver = false;
        StartCoroutine(GameLoop());
    }

    public void UpdateCharacterHealth(int currentHealth)
    {
        if (characterHealthBar != null)
        {
            characterHealthBar.UpdateHealth(currentHealth);
        }
    }

    public void UpdateEnemyHealth(int currentHealth)
    {
        if (enemyHealthBar != null)
        {
            enemyHealthBar.UpdateHealth(currentHealth);
        }
    }

    private void ResetCharacterStats(_Character character)
    {
        character.momentumStack = 0;
        character.dullBladeStack = 0;
        character.NumbingColdStack = 0;
        character.prevTakingDamage = 1000;
    }

    public void CreateEnemy()
    {
        if (enemyCharacter != null)
        {
            Destroy(enemyCharacter.gameObject);
        }

        GameObject enemyObj = Instantiate(enemyPrefab, battleAreaManager.enemyPosition, battleAreaManager.enemyRotation);

        enemyCharacter = enemyObj.GetComponent<_Enemy>();

        // HARD
        Dictionary<int, Vector2Int> hardEnemyHealthRanges = new Dictionary<int, Vector2Int>()
        {
            {0, new Vector2Int(30, 35)},
            {1, new Vector2Int(35, 40)},
            {2, new Vector2Int(40, 45)},
            {3, new Vector2Int(45, 50)},
            {4, new Vector2Int(50, 55)},
            {5, new Vector2Int(60, 65)},
            {6, new Vector2Int(120, 121)}
        };

        // EASY
        Dictionary<int, Vector2Int> easyEnemyHealthRanges = new Dictionary<int, Vector2Int>()
        {
            {0, new Vector2Int(20, 25)},
            {1, new Vector2Int(25, 30)},
            {2, new Vector2Int(30, 35)},
            {3, new Vector2Int(35, 40)},
            {4, new Vector2Int(40, 45)},
            {5, new Vector2Int(45, 50)},
            {6, new Vector2Int(80, 81)}
        };

        enemyCharacter.characterName = "Enemy";

        if (gameDiff == 0) // Easy
        {
            if (easyEnemyHealthRanges.ContainsKey(enemydiff))
            {
                Vector2Int range = easyEnemyHealthRanges[enemydiff];

                enemyCharacter.Health = Random.Range(range.x, range.y);
                enemyCharacter.MaxHealth = enemyCharacter.Health;
            }
            enemyCharacter.MaxHealth = enemyCharacter.Health;
            enemyCharacter.strength = 1;
            enemyCharacter.dexterenity = 1;
            enemyCharacter.talent = enemyTalent;
            enemyCharacter._Weapon = enemydiffweapons[enemydiff];
            enemyCharacter._material = enemyCharacter._Weapon.material;
        }
        else if (gameDiff == 1) // Hard
        {
            if (hardEnemyHealthRanges.ContainsKey(enemydiff))
            {
                Vector2Int range = hardEnemyHealthRanges[enemydiff];

                enemyCharacter.Health = Random.Range(range.x, range.y);
                enemyCharacter.MaxHealth = enemyCharacter.Health;
            }
            enemyCharacter.MaxHealth = enemyCharacter.Health;
            enemyCharacter.strength = 1;
            enemyCharacter.dexterenity = 1;
            enemyCharacter.talent = enemyTalent;
            enemyCharacter._Weapon = enemydiffweapons[enemydiff];
            enemyCharacter._material = enemyCharacter._Weapon.material;
        }
        
    }
    public void CreateCharacter()
    {
        if (playerCharacter != null)
        {
            Destroy(playerCharacter.gameObject);
        }
        GameObject characterPrefab = prefabs[selectedCharacter.characterPrefabIndex];
        GameObject characterObj = Instantiate(characterPrefab, battleAreaManager.characterPosition, battleAreaManager.characterRotation);

        playerCharacter = characterObj.GetComponent<_Character>();

        playerCharacter.characterName = selectedCharacter.characterName;
        playerCharacter.Health = selectedCharacter.Health;
        playerCharacter.MaxHealth = selectedCharacter.MaxHealth;
        playerCharacter.strength = selectedCharacter.strength;
        playerCharacter.dexterenity = selectedCharacter.dexterenity;
        playerCharacter.talent = selectedCharacter.talent;
        playerCharacter._Weapon = selectedCharacter._Weapon;
        playerCharacter._material = selectedCharacter._material;
        playerCharacter.characterPrefabIndex = selectedCharacter.characterPrefabIndex;
        selectedCharacter._animator = playerCharacter._animator;
        selectedCharacter.normalDamageVFX = playerCharacter.normalDamageVFX;
        selectedCharacter.critDamageVFX = playerCharacter.critDamageVFX;
        selectedCharacter.dieVFX = playerCharacter.dieVFX;
        selectedCharacter.missVFX = playerCharacter.missVFX;
        selectedCharacter.attackSFX = playerCharacter.attackSFX;
        selectedCharacter.dieSFX = playerCharacter.dieSFX;
        selectedCharacter.winSFX = playerCharacter.winSFX;
        selectedCharacter.audioSource = playerCharacter.audioSource;
    }

    IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(2.5f);
        while (!isGameOver)
        {
            isPlayerTurn = true;
            while (IsCharacterAlive(selectedCharacter) && IsEnemyAlive(enemyCharacter))
            {
                if (isPlayerTurn)
                {
                    yield return StartCoroutine(PlayerTurn());
                }
                else
                {
                    yield return StartCoroutine(EnemyTurn());
                }
                isPlayerTurn = !isPlayerTurn;
            }

            if (!IsCharacterAlive(selectedCharacter))
            {
                yield return new WaitForSeconds(5f);
                EndGame(false);
                isGameOver = true;
            }
            else if (!IsEnemyAlive(enemyCharacter))
            {
                if (selectedCharacter.talent.type == _CharacterTalent.talentType.SelfHealer)
                {
                    selectedCharacter.Health = Mathf.Clamp(selectedCharacter.Health + 3, 0, selectedCharacter.MaxHealth);
                    UpdateCharacterHealth(selectedCharacter.Health);
                    LogManager.Instance.AppendSpecialLog("Talent Effect", "Self-Healer", "+3 Health restored");
                }
                if (selectedCharacter.talent.type == _CharacterTalent.talentType.Buffed)
                {
                    selectedCharacter.strength += 1;
                    LogManager.Instance.AppendSpecialLog("Talent Effect", "Buffed", "+1 Strength");
                }
                if (selectedCharacter.talent.type == _CharacterTalent.talentType.FastHands)
                {
                    selectedCharacter.dexterenity += 1;
                    LogManager.Instance.AppendSpecialLog("Talent Effect", "Fast Hands", "+1 Dexterity");
                }
                if (selectedCharacter.talent.type == _CharacterTalent.talentType.AimHigher)
                {
                    selectedCharacter._Weapon.minBaseDamage += 1;
                    selectedCharacter._Weapon.maxBaseDamage += 1;
                    LogManager.Instance.AppendSpecialLog("Talent Effect", "Aim Higher", "Min Weapon Base Damage +1 & Max Weapon Base Damage +1");
                }
                yield return new WaitForSeconds(5f);
                EndGame(true);
                isGameOver = true;
            }

            if (!isGameOver)
            {
                yield return new WaitForSeconds(1f / gameSpeed);
            }
        }
    }

    public bool IsCharacterAlive(_Character character)
    {
        if (character.Health > 0)
        {
            return true;
        }
        return false;
    }
    public bool IsEnemyAlive(_Enemy character)
    {
        if (character.Health > 0)
        {
            return true;
        }
        return false;
    }

    IEnumerator PlayerTurn()
    {
        var currentPlayer = selectedCharacter;
        var currentEnemy = enemyCharacter;

        if (currentPlayer != null && currentEnemy != null)
        {
            string debug = $"--- PLAYER TURN BATTLE INFO ---\n" +
                            $"{currentPlayer.characterName} Health: {currentPlayer.Health} \n" +
                            $"{currentEnemy.characterName} Health: {currentEnemy.Health} \n" +
                            $"{currentPlayer.characterName} attacks {currentEnemy.characterName}";
            Debug.Log(debug);
            yield return StartCoroutine(currentPlayer.Attack(currentEnemy));
        }
    }

    IEnumerator EnemyTurn()
    {
        var currentPlayer = selectedCharacter;
        var currentEnemy = enemyCharacter;

        if (currentEnemy != null && currentPlayer != null)
        {
            string debug = $"--- ENEMY TURN BATTLE INFO ---\n" +
                             $"{currentPlayer.characterName} Health: {currentPlayer.Health} \n" +
                             $"{currentEnemy.characterName} Health: {currentEnemy.Health} \n" +
                             $"{currentEnemy.characterName} attacks {currentPlayer.characterName}";
            Debug.Log(debug);
            yield return currentEnemy.Attack(currentPlayer);
        }
    }
    public void sceneChange()
    {
        DontDestroyOnLoad(_GameManager.Instance.gameObject);
        DontDestroyOnLoad(_GameManager.Instance.selectedCharacter.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
    }

    void EndGame(bool playerWon)
    {
        Debug.Log(MapManager.Instance.currentNodeIndex);
        if (playerWon && MapManager.Instance.currentNodeIndex == endGameIndex)
        {
            WinUIManager winUI = FindObjectOfType<WinUIManager>();
            if (winUI != null)
            {
                winUI.ShowVictoryScreen(_GameManager.Instance.selectedCharacter);
            }
            else
            {
                Debug.LogWarning("No WinUIManager found in scene!");
            }
        }

        else if (playerWon)
        {
            Debug.Log("Player Team Wins! Next battle...");

            if (newWeapon != null)
            {
                foreach (var weapon in newWeapon) { weapon.isLocked = false; unlockedWeapon.Add(weapon); }
                newWeapon.Clear();
            }
            if (newWeaponMaterials != null)
            {
                foreach (var weaponMaterial in newWeaponMaterials) { weaponMaterial.isLocked = false; unlockedMaterials.Add(weaponMaterial); }
                newWeaponMaterials.Clear();
            }

            if (campIndex != -1)
            {
                MapManager.Instance.campIndex = campIndex;
            }
            if (fightIndex != -1)
            {
                MapManager.Instance.fightIndex = fightIndex;

            }
            diedAndRestarted = false;
            sceneChange();
        }

        else
        {
            if (newWeapon != null)
            {
                newWeapon.Clear();
            }
            if (newWeaponMaterials != null)
            {
                newWeaponMaterials.Clear();
            }

            Debug.Log("Enemy Team Wins!");
            MapManager.Instance.fightIndex = -1;
            MapManager.Instance.fightNode.Clear();
            diedAndRestarted = true;
            selectionManager.selfDestroy();
            deathManager.ShowDeathPanels();
        }
    }

    public void onEasyButtonClicked()
    {
        EasyButton.interactable = false;
        HardButton.interactable = true;

        gameDiff = 0;
    }

    public void onHardButtonClicked()
    {
        HardButton.interactable = false;
        EasyButton.interactable = true;

        gameDiff = 1;
    }
}
