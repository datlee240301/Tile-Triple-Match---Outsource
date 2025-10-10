using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("Data")] public int id;
    public int layerIndex;
    public Vector2Int grid;
    public bool isInTray { get; private set; }

    [Header("Refs")] [SerializeField] SpriteRenderer iconSR;
    public Collider2D col { get; private set; }

    private Action<Tile> onClicked;
    private BoardGenerator board;
    [SerializeField] float scaleInTray;
    [SerializeField] GameObject starEffect;
    GameManager gameManager;
    SoundManager soundManager;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        ResizeTile();
    }

    public void Init(BoardGenerator board, int id, int layer, Vector2Int grid, Sprite icon, Action<Tile> onClicked)
    {
        this.board = board;
        this.id = id;
        this.layerIndex = layer;
        this.grid = grid;
        this.onClicked = onClicked;

        if (iconSR) iconSR.sprite = icon;

        // order theo layerIndex (layer 0 = 1, layer 1 = 2, ...)
        if (iconSR) iconSR.sortingOrder = layerIndex + 1;

        isInTray = false;
    }

    public bool IsSelectable() => !isInTray && !board.HasCoveringTile(this);

    public void SetActiveState(bool canSelect)
    {
        if (!iconSR) return;
        iconSR.color = canSelect ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
    }

    public void MoveToTray(Vector3 trayPos, Action onArrive = null)
    {
        isInTray = true;
        transform.DOKill();
        transform.DOMove(trayPos, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => { onArrive?.Invoke(); });

        // ✅ nhỏ lại khi vào khay
        transform.DOScale(scaleInTray, 0.2f);

        if (iconSR) iconSR.sortingOrder = 4; // luôn nổi trong khay
    }

    public void MoveToBoard(Vector3 pos, Action onArrive = null)
    {
        isInTray = false;
        transform.DOKill();
        transform.DOMove(pos, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => { onArrive?.Invoke(); });

        // ✅ to lại khi trả về board
        transform.DOScale(1f, 0.2f);

        if (iconSR) iconSR.sortingOrder = layerIndex + 1;
    }


    public void DestroyAnim(Action onComplete)
    {
        transform.DOKill();

        // Nếu có prefab starEffect → tạo ra trước khi tile biến mất
        if (starEffect != null)
        {
            GameObject fx = Instantiate(starEffect, transform.position, Quaternion.identity);
            // tự hủy effect sau 2 giây (tuỳ prefab của bạn)
            Destroy(fx, 2f);
        }

        transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }


    private bool isPointerOverUI;

    private void Update()
    {
        isPointerOverUI = IsPointerOverUIElement();
    }

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Graphic>() != null && !result.gameObject.CompareTag("bg"))
            {
                return true;
            }
        }

        return false;
    }

    void ResizeTile()
    {
        if (gameManager != null)
        {
            if(gameManager.currentLevelId ==1)
                scaleInTray = 0.5f;
            else
                scaleInTray = 0.75f;
        }
    }

    private void OnMouseDown()
    {
        if (isPointerOverUI) return;

        // Lấy điểm click theo world
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p2 = new Vector2(wp.x, wp.y);

        // Lấy tất cả collider 2D tại điểm click
        var hits = Physics2D.OverlapPointAll(p2);
        Tile best = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var tile = hits[i] ? hits[i].GetComponent<Tile>() : null;
            if (tile == null) continue;

            // Bỏ qua tile trong khay hoặc đang bị che
            if (tile.isInTray) continue;
            if (tile.board != null && tile.board.HasCoveringTile(tile)) continue;

            // Ưu tiên layerIndex cao hơn, rồi tới sortingOrder
            int sOrder = tile.iconSR ? tile.iconSR.sortingOrder : 0;
            int score = tile.layerIndex * 10000 + sOrder; // trọng số layer lớn hơn

            if (score > bestScore)
            {
                bestScore = score;
                best = tile;
            }
        }

        // Nếu tìm được tile "top" hợp lệ → click nó
        if (best != null)
        {
            if (soundManager != null) soundManager.PlayClick1Tile();
            best.onClicked?.Invoke(best);
        }
        // Nếu không có tile hợp lệ (ví dụ đang bị UI che) thì bỏ qua
    }

}