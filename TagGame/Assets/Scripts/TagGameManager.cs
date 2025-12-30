using UnityEngine;
using Unity.MLAgents;

public class TagGameManager : MonoBehaviour
{
    [Header("Agents")]
    public TagAgent tagger;
    public TagAgent runner;
    
    [Header("Game Settings")]
    public float episodeTimeLimit = 30f;
    public bool swapRolesOnTag = true;
    
    private float episodeTimer;
    private int taggerWins = 0;
    private int runnerWins = 0;
    
    private void Start()
    {
        ResetEpisode();
    }
    
    private void FixedUpdate()
    {
        episodeTimer += Time.fixedDeltaTime;
        
        // 時間切れ判定
        if (episodeTimer >= episodeTimeLimit)
        {
            OnTimeOut();
        }
    }
    
    public void OnTagSuccess(TagAgent taggerAgent, TagAgent runnerAgent)
    {
        taggerWins++;
        Debug.Log($"Tag! Tagger wins. Score: Tagger {taggerWins} - Runner {runnerWins}");
        
        if (swapRolesOnTag)
        {
            SwapRoles();
        }
        
        ResetEpisode();
    }
    
    private void OnTimeOut()
    {
        runnerWins++;
        Debug.Log($"Time out! Runner survives. Score: Tagger {taggerWins} - Runner {runnerWins}");
        
        // ランナーにボーナス、タガーにペナルティ
        runner.AddReward(0.5f);
        tagger.AddReward(-0.5f);
        
        ResetEpisode();
    }
    
    private void SwapRoles()
    {
        // 役割を交換
        tagger.isTagger = false;
        runner.isTagger = true;
        
        // 参照も交換
        (tagger, runner) = (runner, tagger);
        
        // 見た目も更新（オプション）
        UpdateAgentVisuals();
    }
    
    private void UpdateAgentVisuals()
    {
        // 鬼を赤、逃げを青に
        var taggerRenderer = tagger.GetComponent<Renderer>();
        var runnerRenderer = runner.GetComponent<Renderer>();
        
        if (taggerRenderer) taggerRenderer.material.color = Color.red;
        if (runnerRenderer) runnerRenderer.material.color = Color.blue;
    }
    
    private void ResetEpisode()
    {
        episodeTimer = 0f;
        tagger.EndEpisode();
        runner.EndEpisode();
    }
    
    private void OnGUI()
    {
        // デバッグUI
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"Episode Time: {episodeTimer:F1}s / {episodeTimeLimit}s");
        GUILayout.Label($"Tagger Wins: {taggerWins}");
        GUILayout.Label($"Runner Wins: {runnerWins}");
        GUILayout.EndArea();
    }
}
