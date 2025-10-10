using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Tray")] public Transform trayRoot;
    public int trayCapacity = 7;

    [Header("Tray UI")] [SerializeField] private UnityEngine.UI.Image[] traySlots;
    [SerializeField] private Sprite slotSprite;
    [SerializeField] private Sprite lockSprite;

    [Header("Buttons")] public UnityEngine.UI.Button btnUndo, btnShuffle, btnHint, btnAddTraySlot;

    private readonly List<Tile> tray = new();
    private List<Tile> allTiles;
    [SerializeField] UiPanelDotween winPanelObject, losePanelObject, notifyPanelObject;

    // ---------------- HINT ----------------
    [Header("Hint UI")] [SerializeField] GameObject hintPricePanel, hintNumberPanel;
    [SerializeField] TextMeshProUGUI hintNumberText;
    [SerializeField] private int hintCost = 20;
    private int hintCount;

    // ---------------- ADD SLOT ----------------
    [Header("Add Slot UI")] [SerializeField]
    GameObject addSlotPricePanel, addSlotNumberPanel;

    [SerializeField] TextMeshProUGUI addSlotNumberText;
    [SerializeField] private int addSlotCost = 50;
    private int addSlotCount;

    // ---------------- SHUFFLE ----------------
    [Header("Shuffle UI")] [SerializeField]
    GameObject shufflePricePanel, shuffleNumberPanel;

    [SerializeField] TextMeshProUGUI shuffleNumberText;
    [SerializeField] private int shuffleCost = 30;
    private int shuffleCount;

    // ---------------- UNDO ----------------
    [Header("Undo UI")] [SerializeField] GameObject undoPricePanel, undoNumberPanel;
    [SerializeField] TextMeshProUGUI undoNumberText, levelNumberText;
    [SerializeField] private int undoCost = 10;
    private int undoCount;
    [SerializeField] GameObject[] fireworksEffects;
    SoundManager soundManager;
    MusicManager musicManager;
    [Header("Gameplay Settings")]
    [SerializeField] private float clickCooldown = 0.2f; // ⏱ Thời gian chờ sau mỗi click
    private float lastClickTime = -999f;

    private struct UndoStep
    {
        public Tile tile;
        public Vector3 prevPos;
    }

    private readonly Stack<UndoStep> undoStack = new();

    public IEnumerable<Tile> AllCurrentTiles => allTiles?.Where(t => t != null) ?? System.Linq.Enumerable.Empty<Tile>();

    private bool isShuffling = false;
    private bool isHintRunning = false;
    UIManager uiManager;
    public int currentLevelId;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();
        musicManager = FindObjectOfType<MusicManager>();

        if (traySlots != null && traySlots.Length >= 8)
        {
            for (int i = 0; i < traySlots.Length; i++)
                traySlots[i].sprite = slotSprite;
            traySlots[7].sprite = lockSprite;
        }

        // Init Hint
        if (!PlayerPrefs.HasKey("HintCount"))
            PlayerPrefs.SetInt("HintCount", 3);
        hintCount = PlayerPrefs.GetInt("HintCount", 3);
        UpdateHintUI();

        // Init AddSlot
        if (!PlayerPrefs.HasKey("AddSlotCount"))
            PlayerPrefs.SetInt("AddSlotCount", 1);
        addSlotCount = PlayerPrefs.GetInt("AddSlotCount", 1);
        UpdateAddSlotUI();

        // Init Shuffle
        if (!PlayerPrefs.HasKey("ShuffleCount"))
            PlayerPrefs.SetInt("ShuffleCount", 1);
        shuffleCount = PlayerPrefs.GetInt("ShuffleCount", 1);
        UpdateShuffleUI();

        // Init Undo
        if (!PlayerPrefs.HasKey("UndoCount"))
            PlayerPrefs.SetInt("UndoCount", 3);
        undoCount = PlayerPrefs.GetInt("UndoCount", 3);
        UpdateUndoUI();

        if (btnUndo) btnUndo.onClick.AddListener(OnUndoButton);
        if (btnShuffle) btnShuffle.onClick.AddListener(OnShuffleButton);
        if (btnHint) btnHint.onClick.AddListener(OnHintButton);
        if (btnAddTraySlot) btnAddTraySlot.onClick.AddListener(AddTraySlot);

        // Build level
        if (PlayerPrefs.GetInt(StringManager.pressLevelButton) == 0)
        {
            int Level = PlayerPrefs.GetInt(StringManager.currentLevelId);
            FindObjectOfType<BoardGenerator>().BuildLevel("level" + Level);
            if (levelNumberText != null)
                levelNumberText.text = "Level " + Level;
            currentLevelId = Level;
        }
        else
        {
            int level = PlayerPrefs.GetInt(StringManager.currentLevelIdLevelButton);
            FindObjectOfType<BoardGenerator>().BuildLevel("level" + level);
            if (levelNumberText != null)
                levelNumberText.text = "Level " + level;
            currentLevelId = level;
        }
    }

    public void BindTiles(List<Tile> tiles) => allTiles = tiles;

    public void OnTileClicked(Tile t)
    {
        if (isShuffling || isHintRunning) return;
        if (tray.Count >= trayCapacity) return;

        // ✅ kiểm tra cooldown
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        undoStack.Push(new UndoStep { tile = t, prevPos = t.transform.position });
        InsertTileToTray(t);
    }


    Vector3 GetTrayPos(int idx)
    {
        if (traySlots != null && traySlots.Length > idx && traySlots[idx] != null)
            return traySlots[idx].transform.position;
        return trayRoot.position;
    }

    void CheckTripleAndPruneUndo()
    {
        var groups = tray.GroupBy(x => x.id).Where(g => g.Count() >= 3).ToList();
        if (groups.Count > 0)
        {
            foreach (var g in groups)
            {
                int need = 3;
                List<Tile> toDestroy = new List<Tile>();

                // lấy 3 tile cùng loại (ưu tiên tile mới nhất + các tile trước nó)
                for (int i = tray.Count - 1; i >= 0 && need > 0; i--)
                {
                    if (tray[i].id == g.Key)
                    {
                        var tile = tray[i];
                        tray.RemoveAt(i);
                        allTiles.Remove(tile);
                        RemoveUndoStepsOf(tile);
                        toDestroy.Add(tile);
                        need--;
                    }
                }

                // phá huỷ từng tile
                foreach (var tile in toDestroy)
                {
                    tile.DestroyAnim(() =>
                    {
                        // khi tile cuối cùng phá xong → cập nhật lại vị trí tray
                        if (tile == toDestroy.Last())
                        {
                            for (int i = 0; i < tray.Count; i++)
                                tray[i].MoveToTray(GetTrayPos(i));
                        }
                    });
                }

                soundManager.PlayMatch3TileSound();
            }
        }

        PruneDeadSteps();

        if (allTiles.Count == 0) ShowWinPanel();
        if (tray.Count >= trayCapacity && groups.Count == 0) ShowLosePanel();
    }


    void RemoveUndoStepsOf(Tile tile)
    {
        if (undoStack.Count == 0) return;
        var tmp = new Stack<UndoStep>(undoStack.Count);
        while (undoStack.Count > 0)
        {
            var s = undoStack.Pop();
            if (s.tile != tile) tmp.Push(s);
        }

        while (tmp.Count > 0) undoStack.Push(tmp.Pop());
    }

    void PruneDeadSteps()
    {
        if (undoStack.Count == 0) return;
        var tmp = new Stack<UndoStep>(undoStack.Count);
        while (undoStack.Count > 0)
        {
            var s = undoStack.Pop();
            if (s.tile != null) tmp.Push(s);
        }

        while (tmp.Count > 0) undoStack.Push(tmp.Pop());
    }

    // ======================== UNDO ========================
    void OnUndoButton()
    {
        if (isShuffling || isHintRunning) return;

        if (undoCount > 0)
        {
            if (tray.Count == 0) return;

            // ✅ lấy tile ở ngoài cùng bên phải (cuối tray)
            var targetTile = tray[tray.Count - 1];

            // tìm step của tile đó trong undoStack
            UndoStep? foundStep = null;
            var tmp = new Stack<UndoStep>();
            while (undoStack.Count > 0)
            {
                var s = undoStack.Pop();
                if (s.tile == targetTile && foundStep == null)
                {
                    foundStep = s;
                    continue;
                }

                tmp.Push(s);
            }

            while (tmp.Count > 0) undoStack.Push(tmp.Pop());

            if (foundStep == null) return;

            // đưa tile về lại board
            tray.RemoveAt(tray.Count - 1);
            targetTile.MoveToBoard(foundStep.Value.prevPos);

            // trừ lượt undo
            undoCount--;
            PlayerPrefs.SetInt("UndoCount", undoCount);
            UpdateUndoUI();
        }
        else
        {
            int tickets = PlayerPrefs.GetInt(StringManager.ticketNumber, 0);
            if (tickets >= undoCost)
            {
                uiManager.MinusTicket(undoCost);
                undoCount += 3;
                PlayerPrefs.SetInt("UndoCount", undoCount);
                UpdateUndoUI();
                Debug.Log("Đã mua 1 lượt Undo. Nhấn lần nữa để dùng.");
            }
            else notifyPanelObject.PanelFadeIn();
        }
    }


    void UpdateUndoUI()
    {
        if (undoCount > 0)
        {
            undoNumberPanel.SetActive(true);
            undoPricePanel.SetActive(false);
            undoNumberText.text = undoCount.ToString();
        }
        else
        {
            undoNumberPanel.SetActive(false);
            undoPricePanel.SetActive(true);
        }
    }

    // ======================== HINT ========================
    void OnHintButton()
    {
        if (isHintRunning || isShuffling) return;

        if (hintCount > 0)
        {
            if (HintTrayPair()) return; // ✅ Ưu tiên 1
            if (HintBoardPairWithTray()) return; // ✅ Ưu tiên 2

            // ❌ fallback về chức năng hint cũ
            var group = allTiles.Where(t => t != null && !t.isInTray && t.IsSelectable())
                .GroupBy(t => t.id).FirstOrDefault(g => g.Count() >= 3);

            if (group == null)
            {
                Debug.Log("Không có gì để ăn!");
                return;
            }

            hintCount--;
            PlayerPrefs.SetInt("HintCount", hintCount);
            UpdateHintUI();

            isHintRunning = true;
            StartCoroutine(HintSequence(group.Take(3).ToList()));
        }
        else
        {
            int tickets = PlayerPrefs.GetInt(StringManager.ticketNumber, 0);
            if (tickets >= hintCost)
            {
                uiManager.MinusTicket(hintCost);
                hintCount = 3;
                PlayerPrefs.SetInt("HintCount", hintCount);
                UpdateHintUI();
            }
            else notifyPanelObject.PanelFadeIn();
        }
    }


    void UpdateHintUI()
    {
        if (hintCount > 0)
        {
            hintNumberPanel.SetActive(true);
            hintPricePanel.SetActive(false);
            hintNumberText.text = hintCount.ToString();
        }
        else
        {
            hintNumberPanel.SetActive(false);
            hintPricePanel.SetActive(true);
        }
    }

    // ======================== ADD SLOT ========================
    void AddTraySlot()
    {
        if (trayCapacity >= 8) return;

        if (addSlotCount > 0)
        {
            trayCapacity = 8;
            if (traySlots != null && traySlots.Length >= 8 && traySlots[7] != null)
                traySlots[7].sprite = slotSprite;

            addSlotCount--;
            PlayerPrefs.SetInt("AddSlotCount", addSlotCount);
            UpdateAddSlotUI();
        }
        else
        {
            int tickets = PlayerPrefs.GetInt(StringManager.ticketNumber, 0);
            if (tickets >= addSlotCost)
            {
                uiManager.MinusTicket(addSlotCost);
                trayCapacity = 8;
                if (traySlots != null && traySlots.Length >= 8 && traySlots[7] != null)
                    traySlots[7].sprite = slotSprite;

                addSlotCount += 2;
                PlayerPrefs.SetInt("AddSlotCount", addSlotCount);
                UpdateAddSlotUI();
            }
            else notifyPanelObject.PanelFadeIn();
        }
    }

    void UpdateAddSlotUI()
    {
        if (addSlotCount > 0)
        {
            addSlotNumberPanel.SetActive(true);
            addSlotPricePanel.SetActive(false);
            addSlotNumberText.text = addSlotCount.ToString();
        }
        else
        {
            addSlotNumberPanel.SetActive(false);
            addSlotPricePanel.SetActive(true);
        }
    }

    // ======================== SHUFFLE ========================
    void OnShuffleButton()
    {
        if (isShuffling || isHintRunning) return;

        if (shuffleCount > 0)
        {
            shuffleCount--;
            PlayerPrefs.SetInt("ShuffleCount", shuffleCount);
            UpdateShuffleUI();
            DoShuffle();
        }
        else
        {
            int tickets = PlayerPrefs.GetInt(StringManager.ticketNumber, 0);
            if (tickets >= shuffleCost)
            {
                uiManager.MinusTicket(shuffleCost);
                shuffleCount += 3;
                PlayerPrefs.SetInt("ShuffleCount", shuffleCount);
                UpdateShuffleUI();
                Debug.Log("Đã mua 1 lượt Shuffle. Nhấn lần nữa để dùng.");
            }
            else notifyPanelObject.PanelFadeIn();
        }
    }

    void UpdateShuffleUI()
    {
        if (shuffleCount > 0)
        {
            shuffleNumberPanel.SetActive(true);
            shufflePricePanel.SetActive(false);
            shuffleNumberText.text = shuffleCount.ToString();
        }
        else
        {
            shuffleNumberPanel.SetActive(false);
            shufflePricePanel.SetActive(true);
        }
    }

    void DoShuffle()
    {
        isShuffling = true;
        var boardTiles = allTiles.Where(t => t != null && !t.isInTray).ToList();
        if (boardTiles.Count <= 1)
        {
            isShuffling = false;
            return;
        }

        var groups = boardTiles.GroupBy(t => t.layerIndex);
        int tweenCount = 0;

        foreach (var group in groups)
        {
            var layerTiles = group.ToList();
            if (layerTiles.Count <= 1) continue;

            var poses = layerTiles.Select(t => t.transform.position).ToList();
            poses.Shuffle();

            for (int i = 0; i < layerTiles.Count; i++)
            {
                tweenCount++;
                layerTiles[i].MoveToBoard(poses[i], () =>
                {
                    tweenCount--;
                    if (tweenCount <= 0) isShuffling = false;
                });
            }
        }

        if (tweenCount == 0) isShuffling = false;
    }

    // ======================== HINT SEQUENCE ========================
    System.Collections.IEnumerator HintSequence(List<Tile> tiles)
    {
        foreach (var t in tiles)
        {
            bool arrived = false;
            InsertTileToTray(t);
            // chờ tile bay xong (MoveToTray gọi callback)
            yield return new WaitForSeconds(0.3f);
        }

        isHintRunning = false;
    }


    // ✅ Hint loại 1: Tray có 2 tile giống nhau + board có 1 tile còn lại
    bool HintTrayPair()
    {
        var pair = tray.GroupBy(x => x.id).FirstOrDefault(g => g.Count() == 2);
        if (pair == null) return false;

        int targetId = pair.Key;
        var candidate = allTiles.FirstOrDefault(t => t != null && !t.isInTray && t.id == targetId && t.IsSelectable());
        if (candidate == null) return false;

        hintCount--;
        PlayerPrefs.SetInt("HintCount", hintCount);
        UpdateHintUI();

        isHintRunning = true;
        StartCoroutine(HintSequence(new List<Tile> { candidate }));
        return true;
    }

// ✅ Hint loại 2: Board có 2 tile giống nhau + tray có 1 tile cùng loại
    bool HintBoardPairWithTray()
    {
        var trayIds = tray.Select(t => t.id).ToHashSet();

        foreach (int id in trayIds)
        {
            var candidates = allTiles.Where(t => t != null && !t.isInTray && t.id == id && t.IsSelectable()).Take(2)
                .ToList();
            if (candidates.Count == 2)
            {
                hintCount--;
                PlayerPrefs.SetInt("HintCount", hintCount);
                UpdateHintUI();

                isHintRunning = true;
                StartCoroutine(HintSequence(candidates));
                return true;
            }
        }

        return false;
    }

    void InsertTileToTray(Tile t)
    {
        // Tìm vị trí cuối nhóm cùng loại
        int insertIndex = -1;
        for (int i = 0; i < tray.Count; i++)
        {
            if (tray[i].id == t.id)
            {
                insertIndex = i;
                while (insertIndex + 1 < tray.Count && tray[insertIndex + 1].id == t.id)
                    insertIndex++;
                break;
            }
        }

        if (insertIndex == -1)
        {
            // ❌ Không có nhóm cùng loại -> thêm vào cuối
            tray.Add(t);
            Vector3 slot = GetTrayPos(tray.Count - 1);
            t.MoveToTray(slot, () => { CheckTripleAndPruneUndo(); });
        }
        else
        {
            int targetIndex = insertIndex + 1;

            // ✅ dịch các tile phía sau sang phải
            for (int i = tray.Count - 1; i >= targetIndex; i--)
            {
                int newIndex = i + 1;
                if (newIndex < trayCapacity)
                    tray[i].MoveToTray(GetTrayPos(newIndex));
            }

            // Chèn tile vào đúng chỗ
            tray.Insert(targetIndex, t);
            Vector3 slot = GetTrayPos(targetIndex);
            t.MoveToTray(slot, () => { CheckTripleAndPruneUndo(); });
        }
    }


    void ShowWinPanel()
    {
        if (winPanelObject != null)
        {
            PlayerPrefs.SetInt(StringManager.hasPlayLevel1, 1);
            if (PlayerPrefs.GetInt(StringManager.pressPlayButton) == 1 &&
                PlayerPrefs.GetInt(StringManager.currentLevelId) != 15)
                PlayerPrefs.SetInt(StringManager.currentLevelId, PlayerPrefs.GetInt(StringManager.currentLevelId) + 1);
            if (PlayerPrefs.GetInt(StringManager.pressLevelButton) == 1 &&
                PlayerPrefs.GetInt(StringManager.currentLevelIdLevelButton) != 15)
            {
                if (PlayerPrefs.GetInt(StringManager.currentLevelId) ==
                    PlayerPrefs.GetInt(StringManager.currentLevelIdLevelButton))
                {
                    PlayerPrefs.SetInt(StringManager.currentLevelId,
                        PlayerPrefs.GetInt(StringManager.currentLevelId) + 1);
                }

                PlayerPrefs.SetInt(StringManager.currentLevelIdLevelButton,
                    PlayerPrefs.GetInt(StringManager.currentLevelIdLevelButton) + 1);
            }

            winPanelObject.PanelFadeIn();
            uiManager.BuyTicket(50);
            soundManager.PlayWinSound();
            musicManager.audioSource.volume = 0;
        }

        foreach (var fx in fireworksEffects)
            if (fx != null)
                fx.SetActive(true);
    }

    void ShowLosePanel()
    {
        musicManager.audioSource.volume = 0;
        soundManager.PlayLoseSound();
        if (losePanelObject != null) losePanelObject.PanelFadeIn();
    }
}

public static class ShuffleExt
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}