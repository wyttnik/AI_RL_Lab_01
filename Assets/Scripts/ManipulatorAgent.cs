using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using System;

public class ManipulatorAgent : Agent
{
    // Конец щупальца
    public Transform head;
    // Цель, которой необходимо коснуться
    public Transform target;
    // Настройки области спауна цели для обучения
    public Vector3 targetSpawnCenter = new Vector3(0, 1.7f, 0);
    public Vector3 targetSpawnScale = new Vector3(2f, 1.5f, 2f);
    public bool drawTargetGizmos = false;
    private JointController[] joints;
    //private float beginDist = 0;
    //private float prevBestDist = 0;
    public override void Initialize()
    {
        joints = GetComponentsInChildren<JointController>();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        continuousActionsOut[2] = -Input.GetAxis("Mouse X");
        continuousActionsOut[3] = Input.GetAxis("Mouse Y");
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        for (var i = 0; i < joints.Length; i++)
        {
            joints[i].Rotate(actionBuffers.ContinuousActions[i] * 5f);
        }
        //Debug.Log(Mathf.Abs(joints[2].transform.position.y - target.transform.position.y));
        //var jointToDiscDist = MathF.Abs(target.transform.position.y - joints[2].transform.position.y);

        /*if (jointToDiscDist < 0.24f)
        {
            SetReward(-0.01f);
        }*/

        //if ((joints[1].transform.position.y <= target.transform.position.y &&
        //    target.transform.position.y <= joints[2].transform.position.y) ||
        //    (joints[2].transform.position.y <= target.transform.position.y &&
        //    target.transform.position.y <= joints[1].transform.position.y))
        //{
        //    SetReward(-0.01f);
        //}

        var headToTarget = target.transform.position - head.transform.position;
        
        // var headDir = head.transform.up;
        var currDist = headToTarget.magnitude;

        //SetReward(Vector3.Dot(headToTargertDir, headDir)/2);

        //var joint0ToTargetDir = (
        //    new Vector3(target.transform.position.x, joints[0].transform.position.y, target.transform.position.z)
        //    - joints[0].transform.position).normalized;
        //var joint0Dir = joints[0].transform.right;

        //AddReward(Vector3.Dot(joint0ToTargetDir, joint0Dir)/3);

        if (currDist < 0.4f)
        {
            var headToTargetDir = headToTarget.normalized;
            var bonus = Mathf.Clamp01(Vector3.Dot(headToTargetDir, head.transform.up));
            AddReward(1.0f + bonus);
            EndEpisode();
        }
        else
        {
            AddReward(-0.01f);
        }
        //else
        //{
        //    if (currDist > prevBestDist)
        //    {
        //        AddReward(prevBestDist - currDist);
        //    }
        //    else
        //    {
        //        AddReward(beginDist - currDist);
        //        beginDist = currDist;
        //    }
        //}

        /*Debug.DrawLine(head.transform.position, head.transform.position + headDir * 3, Color.red, 1);
        Debug.DrawLine(head.transform.position, head.transform.position + headToTargertDir, Color.red, 1);*/


        /*Debug.DrawLine(joints[0].transform.position, joints[0].transform.position +
            joint0Dir * 3, Color.green, 1);
        Debug.DrawLine(joints[0].transform.position, 
            joints[0].transform.position + joint0ToTargetDir, Color.green, 1);

        Debug.Log(Vector3.Dot(joint0ToTargetDir,joint0Dir));*/

        /*Debug.DrawLine(joints[2].transform.position, joints[2].transform.position +
            joints[2].transform.up * 3, Color.red, 1);
        Debug.DrawLine(joints[2].transform.position, joints[2].transform.position +
            (new Vector3(joints[2].transform.position.x, target.transform.position.y, joints[2].transform.position.z)
            - joints[2].transform.position), Color.red, 1);*/
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(target.transform.position); // 3
        //sensor.AddObservation(head.transform.position); // 3
        for (var i = 0; i < joints.Length; i++)
            sensor.AddObservation(joints[i].GetRotation()); // 4

        //sensor.AddObservation(transform.position); // 3
        //sensor.AddObservation(joints[3].transform.position); // 3

        var headToTarget = target.transform.position - head.transform.position;
        sensor.AddObservation(headToTarget); // 3
        sensor.AddObservation(transform.position - target.transform.position); // 3

        //sensor.AddObservation(headToTarget.magnitude); // 1

        //var headToTargertDir = (target.transform.position - head.transform.position).normalized;
        //sensor.AddObservation(Vector3.Dot(headToTargertDir, head.transform.up)); // 1

        //var joint0ToTargetDir = (
        //    new Vector3(target.transform.position.x, joints[0].transform.position.y, target.transform.position.z)
        //    - joints[0].transform.position).normalized;
        //sensor.AddObservation(Vector3.Dot(joint0ToTargetDir, joints[0].transform.right)); // 1

        //var joint2ToTargetDir = (
        //    new Vector3(joints[2].transform.position.x, target.transform.position.y, joints[2].transform.position.z)
        //    - joints[2].transform.position).normalized;
        //sensor.AddObservation(Vector3.Dot(joint2ToTargetDir, joints[2].transform.up)); // 1

        //sensor.AddObservation(joints[2].transform.position); // 3
        //sensor.AddObservation(joints[1].transform.position); // 3
        //sensor.AddObservation(joints[2].transform.position - target.transform.position); // 3
        //sensor.AddObservation(transform.position - target.transform.position); // 3
        //sensor.AddObservation(joints[2].transform.position.y); // 1
        //sensor.AddObservation(joints[1].transform.position.y); // 1
    }
    public override void OnEpisodeBegin()
    {
        target.transform.position = randomTargetPosititon();
        //beginDist = (target.transform.position - head.transform.position).magnitude;
        //prevBestDist = beginDist;
    }

    private Vector3 randomTargetPosititon()
    {
        var point = Random.insideUnitSphere;
        point.Scale(targetSpawnScale);
        point += targetSpawnCenter;
        return transform.position + point;
    }
    public void OnDrawGizmosSelected()
    {
        if (!drawTargetGizmos) return;
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        for (var i = 0; i < 100; i++)
        {
            Gizmos.DrawWireSphere(randomTargetPosititon(), 0.1f);
        }
    }
}
