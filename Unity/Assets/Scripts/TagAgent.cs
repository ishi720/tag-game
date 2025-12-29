using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TagAgent : Agent
{
    [Header("Agent Settings")]
    public bool isTagger = false;
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    
    [Header("References")]
    public TagAgent opponent;
    public Transform areaCenter;
    public float areaSize = 10f;
    
    private Rigidbody rb;
    private TagGameManager gameManager;
    
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<TagGameManager>();
    }
    
    public override void OnEpisodeBegin()
    {
        // ランダムな位置にスポーン
        Vector3 randomPos = areaCenter.position + new Vector3(
            Random.Range(-areaSize / 2, areaSize / 2),
            0.5f,
            Random.Range(-areaSize / 2, areaSize / 2)
        );
        transform.position = randomPos;
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // 自分の位置と速度（正規化）
        Vector3 localPos = (transform.position - areaCenter.position) / areaSize;
        sensor.AddObservation(localPos.x);
        sensor.AddObservation(localPos.z);
        sensor.AddObservation(rb.velocity.normalized);
        
        // 相手への相対位置
        if (opponent != null)
        {
            Vector3 toOpponent = opponent.transform.position - transform.position;
            Vector3 localDir = transform.InverseTransformDirection(toOpponent.normalized);
            sensor.AddObservation(localDir.x);
            sensor.AddObservation(localDir.z);
            sensor.AddObservation(Mathf.Clamp(toOpponent.magnitude / areaSize, 0, 1));
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
        
        // 自分が鬼かどうか
        sensor.AddObservation(isTagger ? 1f : 0f);
        
        // 壁までの距離（4方向）
        AddWallDistanceObservations(sensor);
    }
    
    private void AddWallDistanceObservations(VectorSensor sensor)
    {
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, areaSize, LayerMask.GetMask("Wall")))
            {
                sensor.AddObservation(hit.distance / areaSize);
            }
            else
            {
                sensor.AddObservation(1f);
            }
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 連続アクション: 前後移動、回転
        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];
        
        // 移動
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
        
        // 回転
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn, 0));
        
        // 報酬設計
        ApplyRewards();
        
        // 時間経過ペナルティ（鬼）/ ボーナス（逃げ）
        if (isTagger)
        {
            AddReward(-0.001f); // 早くタッチするインセンティブ
        }
        else
        {
            AddReward(0.001f); // 生存ボーナス
        }
    }
    
    private void ApplyRewards()
    {
        if (opponent == null) return;
        
        float distance = Vector3.Distance(transform.position, opponent.transform.position);
        
        if (isTagger)
        {
            // 鬼: 相手に近づくと報酬
            AddReward(-distance * 0.0001f);
        }
        else
        {
            // 逃げ: 相手から離れると報酬
            AddReward(distance * 0.0001f);
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 人間操作用（デバッグ）
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // タッチ判定
        if (collision.gameObject.TryGetComponent<TagAgent>(out TagAgent other))
        {
            if (isTagger && !other.isTagger)
            {
                // タッチ成功！
                AddReward(1f);
                other.AddReward(-1f);
                
                gameManager?.OnTagSuccess(this, other);
            }
        }
        
        // 壁に当たったらペナルティ
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            AddReward(-0.01f);
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        // 壁に張り付き続けるのを防ぐ
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            AddReward(-0.005f);
        }
    }
}
