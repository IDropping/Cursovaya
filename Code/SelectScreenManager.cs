using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static CharacterManager;
using static StateManager;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces>();
    public PotraitInfo[] potraitPrefabs;
    public int maxX;
    public int maxY;
    PotraitInfo[,] charGrid;

    public GameObject potraitCanvas;

    bool loadLevel;
    public bool bothPlayersSelected;

    CharacterManager charManager;

    #region Singleton
    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }
    #endregion

    void Start()
    {
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        charGrid = new PotraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        potraitPrefabs = potraitCanvas.GetComponentsInChildren<PotraitInfo>();

        for (int i = 0; i < potraitPrefabs.Length; i++)
        {
            potraitPrefabs[i].posX += x;
            potraitPrefabs[i].posY += y;

            charGrid[x, y] = potraitPrefabs[i];

            if (x < maxX - 1)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }

    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0; i < plInterfaces.Count; i++)
            {
                if (i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId))
                    {
                        plInterfaces[i].playerBase.hasCharacter = false;
                    }

                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(plInterfaces[i]);
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }
            }
        }
        if (bothPlayersSelected)
        {
            Debug.Log("loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;
        }
        else
        {
            if (charManager.players[0].hasCharacter
                && charManager.players[1].hasCharacter)
            {
                bothPlayersSelected = true;
            }
        }
    }

    void HandleSelectScreenInput(PlayerInterfaces p1, string playerId)
    {
        #region Grid Navigation

        float vertical = Input.GetAxis("Vertical" + playerId);

        if (vertical != 0)
        {
            if (!p1.hitInputOnce)
            {
                if (vertical > 0)
                {
                    p1.activeY = (p1.activeY > 0) ? p1.activeY - 1 : maxY - 1;
                }
                else
                {
                    p1.activeY = (p1.activeY < maxY - 1) ? p1.activeY + 1 : 0;
                }

                p1.hitInputOnce = true;
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if (horizontal != 0)
        {
            if (!p1.hitInputOnce)
            {
                if (horizontal > 0)
                {
                    p1.activeX = (p1.activeX > 0) ? p1.activeX - 1 : maxX - 1;
                }
                else
                {
                    p1.activeX = (p1.activeX < maxX - 1) ? p1.activeX + 1 : 0;
                }

                p1.timerToReset = 0;
                p1.hitInputOnce = true;
            }
        }

        if (vertical == 0 && horizontal == 0)
        {
            p1.hitInputOnce = false;
        }

        if (p1.hitInputOnce)
        {
            p1.timerToReset += Time.deltaTime;

            if (p1.timerToReset > 0.8f)
            {
                p1.hitInputOnce = false;
                p1.timerToReset = 0;
            }
        }

        #endregion

        if (Input.GetButtonUp("Fire1" + playerId))
        {
            p1.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            p1.playerBase.playerPrefab =
                charManager.returnCharacterWithID(p1.activePotrait.characterId).prefab;

            p1.playerBase.hasCharacter = true;
        }
    }

    IEnumerator LoadLevel()
    {
        for (int i = 0; i < charManager.players.Count; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.ai)
            {
                if (charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitPrefabs.Length);

                    charManager.players[i].playerPrefab =
                        charManager.returnCharacterWithID(potraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(potraitPrefabs[ranValue].characterId);
                }
            }                               
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("level", LoadSceneMode.Single);
    }

    void HandleSelectorPosition(PlayerInterfaces pl)
    {
        pl.selector.SetActive(true);

        pl.activePotrait = charGrid[pl.activeX, pl.activeY];

        Vector2 selectorPosition = pl.activePotrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x
            , potraitCanvas.transform.localPosition.y);

        pl.selector.transform.localPosition = selectorPosition;
    }


    void HandleCharacterPreview(PlayerInterfaces p1)
    {
        if (p1.previewPotrait != p1.activePotrait)
        {
            if (p1.createdCharacter != null)
            {
                Destroy(p1.createdCharacter);
            }

            GameObject go = Instantiate(
            CharacterManager.GetInstance().returnCharacterWithID(p1.activePotrait.characterId).prefab,
            p1.charVisPos.position, Quaternion.identity) as GameObject;

            p1.createdCharacter = go;

            p1.previewPotrait = p1.activePotrait;

            if (!string.Equals(p1.playerBase.playerId, charManager.players[0].playerId))
            {
               p1.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }

    [System.Serializable]
    public class PlayerInterfaces
    {
        public PotraitInfo activePotrait;
        public PotraitInfo previewPotrait;
        public GameObject selector;
        public Transform charVisPos;
        public GameObject createdCharacter;

        public int activeX;
        public int activeY;

        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;
    }
}
