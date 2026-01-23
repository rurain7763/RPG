using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeBuilder", menuName = "Common/SkillTreeBuilder")]
public class SkillTreeBuilder : ScriptableObject
{
    [Serializable]
    public class NodeData
    {
        public SkillDataObject Skill;
        [SerializeReference, SubclassSelector] public ISkillCertificate Certificate;
        [SerializeReference] public List<int> ParentIndices = new();
        [SerializeReference] public List<int> ChildIndices = new();
        public Vector2 Position; // 에디터 윈도우에서의 시각적 위치 (편집기 전용)
    }

    public List<NodeData> AllNodes = new();

    public void AddNode(SkillDataObject skill, ISkillCertificate certificate)
    {
        NodeData newNodeData = new NodeData
        {
            Skill = skill,
            Certificate = certificate
        };

        AllNodes.Add(newNodeData);
    }

    public void RemoveNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= AllNodes.Count)
        {
            throw new ArgumentOutOfRangeException("Node index is out of range.");
        }

        for (int i = 0; i < AllNodes.Count; i++)
        {
            if (i == nodeIndex) continue;
            var nodeData = AllNodes[i];
            nodeData.ParentIndices.Remove(nodeIndex);
            nodeData.ChildIndices.Remove(nodeIndex);
        }

        AllNodes.RemoveAt(nodeIndex);
        // Update indices in remaining nodes
        for (int i = 0; i < AllNodes.Count; i++)
        {
            var nodeData = AllNodes[i];
            for (int j = 0; j < nodeData.ParentIndices.Count; j++)
            {
                if (nodeData.ParentIndices[j] > nodeIndex)
                {
                    nodeData.ParentIndices[j]--;
                }
            }
            for (int j = 0; j < nodeData.ChildIndices.Count; j++)
            {
                if (nodeData.ChildIndices[j] > nodeIndex)
                {
                    nodeData.ChildIndices[j]--;
                }
            }
        }
    }

    public void ConnectNode(int parentIndex, int childIndex)
    {
        if (parentIndex < 0 || parentIndex >= AllNodes.Count || childIndex < 0 || childIndex >= AllNodes.Count)
        {
            throw new ArgumentOutOfRangeException("Parent or child index is out of range.");
        }
        var parentNodeData = AllNodes[parentIndex];
        var childNodeData = AllNodes[childIndex];
        
        if (!parentNodeData.ChildIndices.Contains(childIndex))
        {
            parentNodeData.ChildIndices.Add(childIndex);
        }

        if (!childNodeData.ParentIndices.Contains(parentIndex))
        {
            childNodeData.ParentIndices.Add(parentIndex);
        }
    }

    public void DisconnectNode(int parentIndex, int childIndex)
    {
        if (parentIndex < 0 || parentIndex >= AllNodes.Count || childIndex < 0 || childIndex >= AllNodes.Count)
        {
            throw new ArgumentOutOfRangeException("Parent or child index is out of range.");
        }
        var parentNodeData = AllNodes[parentIndex];
        var childNodeData = AllNodes[childIndex];
        parentNodeData.ChildIndices.Remove(childIndex);
        childNodeData.ParentIndices.Remove(parentIndex);
    }

    public SkillTree Build()
    {
        SkillTree skillTree = new SkillTree();

        Dictionary<NodeData, SkillTreeNode> nodeMapping = new();
        for (int i = 0; i < AllNodes.Count; i++)
        {
            var nodeData = AllNodes[i];
            var newNode = skillTree.AddNode(nodeData.Skill, nodeData.Certificate);

            nodeMapping[nodeData] = newNode;
        }

        for (int i = 0; i < AllNodes.Count; i++)
        {
            var nodeData = AllNodes[i];
            var currentNode = nodeMapping[nodeData];

            foreach (var parentIndex in nodeData.ParentIndices)
            {
                var parentNodeData = AllNodes[parentIndex];
                var parentNode = nodeMapping[parentNodeData];
                skillTree.ConnectNode(parentNode, currentNode);
            }
        }

        return skillTree;
    }
}