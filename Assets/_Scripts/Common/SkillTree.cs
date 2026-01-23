using System;
using System.Collections.Generic;

public interface ISkillCertificate
{
    bool IsUnlocked(SkillTreeNode node, Arguments args);
    bool CanUnlock(SkillTreeNode node, Arguments args);
    void Unlock(SkillTreeNode node, Arguments args);
}

[Serializable]
public class SkillTreeNode
{
    private SkillTree skillTree;

    private SkillDataObject skillData;
    private ISkillCertificate certificate;

    public SkillDataObject SkillData => skillData;
    public bool IsRoot => Parents.Count == 0;
    public IReadOnlyList<SkillTreeNode> Parents => skillTree.GetParents(this);
    public IReadOnlyList<SkillTreeNode> Children => skillTree.GetChildren(this);

    internal SkillTreeNode(SkillTree skillTree, SkillDataObject skillData, ISkillCertificate certificate)
    {
        this.skillTree = skillTree;
        this.skillData = skillData;
        this.certificate = certificate;
    }

    public bool IsUnlocked(Arguments args = null) => certificate.IsUnlocked(this, args);
    public bool CanUnlock(Arguments args = null) => certificate.CanUnlock(this, args);
    public void Unlock(Arguments args = null) => certificate.Unlock(this, args);
}

public class SkillTree
{
    class NodeConnection
    {
        public List<SkillTreeNode> Parents = new();
        public List<SkillTreeNode> Children = new();
    }

    private List<SkillTreeNode> nodes = new();
    private Dictionary<SkillTreeNode, NodeConnection> nodeConnections = new();

    public IReadOnlyList<SkillTreeNode> Nodes => nodes;

    private bool IsValidNode(SkillTreeNode node)
    {
        return nodeConnections.ContainsKey(node);
    }

    public SkillTreeNode AddNode(SkillDataObject skillData, ISkillCertificate certificate)
    {
        var newNode = new SkillTreeNode(this, skillData, certificate);
        nodes.Add(newNode);
        nodeConnections[newNode] = new NodeConnection();

        return newNode;
    }

    public SkillTreeNode GetNodeBySkillData(SkillDataObject skillData)
    {
        return nodes.Find(node => node.SkillData == skillData);
    }

    public void RemoveNode(SkillTreeNode node)
    {
        if (!IsValidNode(node))
        {
            throw new ArgumentException("The node to be removed does not exist in the skill tree.");
        }

        NodeConnection connection = nodeConnections[node];

        foreach (var parent in connection.Parents)
        {
            NodeConnection parentConn = nodeConnections[parent];
            parentConn.Children.Remove(node);
        }

        foreach (var child in connection.Children)
        {
            NodeConnection childConn = nodeConnections[child];
            childConn.Parents.Remove(node);
        }

        nodeConnections.Remove(node);
        nodes.Remove(node);
    }

    public void ConnectNode(SkillTreeNode parent, SkillTreeNode child)
    {
        if (!IsValidNode(parent) || !IsValidNode(child))
        {
            throw new ArgumentException("One or both nodes to be connected do not exist in the skill tree.");
        }

        NodeConnection parentConn = nodeConnections[parent];
        NodeConnection childConn = nodeConnections[child];

        if (FindChildNodeDFS(child, parent) || FindParentNodeDFS(parent, child))
        {
            throw new InvalidOperationException("Connecting these nodes would create a cycle in the skill tree.");
        }

        if (!parentConn.Children.Contains(child))
        {
            parentConn.Children.Add(child);
        }

        if (!childConn.Parents.Contains(parent))
        {
            childConn.Parents.Add(parent);
        }
    }

    private bool FindChildNodeDFS(SkillTreeNode current, SkillTreeNode target)
    {
        return FindNodeDFSImpl(current, target, new HashSet<SkillTreeNode>(), conn => conn.Children);
    }

    private bool FindParentNodeDFS(SkillTreeNode current, SkillTreeNode target)
    {
        return FindNodeDFSImpl(current, target, new HashSet<SkillTreeNode>(), conn => conn.Parents);
    }

    private bool FindNodeDFSImpl(SkillTreeNode start, SkillTreeNode target, HashSet<SkillTreeNode> visited, Func<NodeConnection, IReadOnlyList<SkillTreeNode>> getListFunc)
    {
        if (start == target)
        {
            return true;
        }
        visited.Add(start);
        NodeConnection connection = nodeConnections[start];
        foreach (var node in getListFunc(connection))
        {
            if (!visited.Contains(node))
            {
                if (FindNodeDFSImpl(node, target, visited, getListFunc))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void DisconnectNode(SkillTreeNode parent, SkillTreeNode child)
    {
        if (!IsValidNode(parent) || !IsValidNode(child))
        {
            throw new ArgumentException("One or both nodes to be disconnected do not exist in the skill tree.");
        }

        NodeConnection parentConn = nodeConnections[parent];
        NodeConnection childConn = nodeConnections[child];

        parentConn.Children.Remove(child);
        childConn.Parents.Remove(parent);
    }

    public IReadOnlyList<SkillTreeNode> GetParents(SkillTreeNode child)
    {
        if (!IsValidNode(child))
        {
            throw new ArgumentException("The child node does not exist in the skill tree.");
        }

        NodeConnection childConn = nodeConnections[child];
        return childConn.Parents.AsReadOnly();
    }

    public IReadOnlyList<SkillTreeNode> GetChildren(SkillTreeNode parent)
    {
        if (!IsValidNode(parent))
        {
            throw new ArgumentException("The parent node does not exist in the skill tree.");
        }

        NodeConnection parentConn = nodeConnections[parent];
        return parentConn.Children.AsReadOnly();
    }
}