using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    [Header("Canvas References")]
    public Canvas gameCanvas;
    public Canvas winCanvas;
    public Canvas gameOverCanvas;

    [Header("Game Objects to Reset")]
    public List<GameObject> gameObjectsToReset = new List<GameObject>();

    [Header("Scripts to Reset")]
    public SpriteColorController spriteColorController;
    public MouseMovement mouseMovement;
    public CameraControlTest cameraController;

    [Header("Reset Settings")]
    public float uiDisplayTime = 3f;
    public bool autoRestart = true;

    // Armazena estados iniciais dos GameObjects
    private Dictionary<GameObject, GameObjectState> initialStates = new Dictionary<GameObject, GameObjectState>();

    [System.Serializable]
    private class GameObjectState
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool activeState;
        public Color spriteColor;
        public bool hasSpriteRenderer;

        public GameObjectState(GameObject obj)
        {
            Transform t = obj.transform;
            position = t.position;
            rotation = t.eulerAngles;
            scale = t.localScale;
            activeState = obj.activeInHierarchy;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                spriteColor = sr.color;
                hasSpriteRenderer = true;
            }
            else
            {
                hasSpriteRenderer = false;
            }
        }
    }

    private static GameStateManager instance;
    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameStateManager>();
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        // Salva estados iniciais de todos os GameObjects
        SaveInitialStates();

        // Configura Canvas inicial
        SetActiveCanvas(gameCanvas);

        // Conecta eventos dos scripts
        ConnectToGameEvents();

        Debug.Log("Game State Manager initialized!");
    }

    private void SaveInitialStates()
    {
        initialStates.Clear();

        // Auto-detecta GameObjects importantes se a lista estiver vazia
        if (gameObjectsToReset.Count == 0)
        {
            AutoDetectGameObjects();
        }

        foreach (GameObject obj in gameObjectsToReset)
        {
            if (obj != null)
            {
                initialStates[obj] = new GameObjectState(obj);
            }
        }

        Debug.Log($"Saved initial states for {initialStates.Count} GameObjects");
    }

    private void AutoDetectGameObjects()
    {
        // Auto-detecta sprites na cena
        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            if (!gameObjectsToReset.Contains(sprite.gameObject))
            {
                gameObjectsToReset.Add(sprite.gameObject);
            }
        }

        // Auto-detecta sistemas de partículas
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            if (!gameObjectsToReset.Contains(ps.gameObject))
            {
                gameObjectsToReset.Add(ps.gameObject);
            }
        }
    }

    private void ConnectToGameEvents()
    {
        // Modifica o SpriteColorController para usar este manager
        if (spriteColorController != null)
        {
            // Substitui as chamadas de PauseForOutcome
            StartCoroutine(MonitorGameConditions());
        }
    }

    private IEnumerator MonitorGameConditions()
    {
        while (true)
        {
            if (spriteColorController != null)
            {
                // Verifica condição de vitória
                if (CheckWinCondition())
                {
                    OnWinCondition();
                    yield break;
                }

                // Verifica condição de derrota
                if (CheckLoseCondition())
                {
                    OnLoseCondition();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool CheckWinCondition()
    {
        // Implementa lógica baseada no seu SpriteColorController
        // Retorna true se todos sprites estão na fase final por tempo suficiente
        return false; // Substitua pela lógica real
    }

    private bool CheckLoseCondition()
    {
        // Implementa lógica baseada no seu SpriteColorController  
        // Retorna true se voltou ao início e ficou tempo suficiente
        return false; // Substitua pela lógica real
    }

    public void OnWinCondition()
    {
        StartCoroutine(HandleWinSequence());
    }

    public void OnLoseCondition()
    {
        StartCoroutine(HandleLoseSequence());
    }

    private IEnumerator HandleWinSequence()
    {
        Debug.Log("WIN CONDITION TRIGGERED!");

        // 1. Esconde GameObjects do jogo
        HideGameObjects();

        // 2. Mostra Canvas de Vitória
        SetActiveCanvas(winCanvas);

        // 3. Espera o tempo definido
        yield return new WaitForSeconds(uiDisplayTime);

        // 4. Restaura estado inicial
        if (autoRestart)
        {
            RestartGame();
        }
    }

    private IEnumerator HandleLoseSequence()
    {
        Debug.Log("LOSE CONDITION TRIGGERED!");

        // 1. Esconde GameObjects do jogo
        HideGameObjects();

        // 2. Mostra Canvas de Game Over
        SetActiveCanvas(gameOverCanvas);

        // 3. Espera o tempo definido
        yield return new WaitForSeconds(uiDisplayTime);

        // 4. Restaura estado inicial
        if (autoRestart)
        {
            RestartGame();
        }
    }

    private void HideGameObjects()
    {
        foreach (GameObject obj in gameObjectsToReset)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private void ShowGameObjects()
    {
        foreach (GameObject obj in gameObjectsToReset)
        {
            if (obj != null && initialStates.ContainsKey(obj))
            {
                obj.SetActive(initialStates[obj].activeState);
            }
        }
    }

    public void RestartGame()
    {
        StartCoroutine(RestartGameSequence());
    }

    private IEnumerator RestartGameSequence()
    {
        Debug.Log("Restarting game...");

        // 1. Volta para Canvas do jogo
        SetActiveCanvas(gameCanvas);

        // 2. Restaura todos os GameObjects ao estado inicial
        RestoreInitialStates();

        // 3. Mostra GameObjects novamente
        ShowGameObjects();

        // 4. Reinicia scripts
        yield return StartCoroutine(RestartGameScripts());

        // 5. Reinicia monitoramento
        StartCoroutine(MonitorGameConditions());

        Debug.Log("Game restarted successfully!");
    }

    private void RestoreInitialStates()
    {
        foreach (var kvp in initialStates)
        {
            GameObject obj = kvp.Key;
            GameObjectState state = kvp.Value;

            if (obj != null)
            {
                // Restaura transform
                Transform t = obj.transform;
                t.position = state.position;
                t.eulerAngles = state.rotation;
                t.localScale = state.scale;

                // Restaura cor do sprite se houver
                if (state.hasSpriteRenderer)
                {
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = state.spriteColor;
                    }
                }
            }
        }
    }

    private IEnumerator RestartGameScripts()
    {
        // Reinicia SpriteColorController
        if (spriteColorController != null)
        {
            spriteColorController.enabled = false;
            yield return null;
            spriteColorController.enabled = true;
        }

        // Reinicia MouseMovement
        if (mouseMovement != null)
        {
            mouseMovement.enabled = false;
            yield return null;
            mouseMovement.enabled = true;
        }

        // Reinicia CameraController
        if (cameraController != null)
        {
            cameraController.RestartCamera();
        }
    }

    private void SetActiveCanvas(Canvas targetCanvas)
    {
        // Desativa todos os canvas
        if (gameCanvas != null) gameCanvas.gameObject.SetActive(false);
        if (winCanvas != null) winCanvas.gameObject.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.gameObject.SetActive(false);

        // Ativa o canvas alvo
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
        }
    }

    // Métodos públicos para botões da UI
    public void RestartFromUI()
    {
        RestartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}