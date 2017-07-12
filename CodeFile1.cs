using System.Collections.Generic;
using System.Collections;
using System;

class TreeNode
{
    //private readonly Dictionary<string, TreeNode> _childs = new Dictionary<string, TreeNode>();
    public List<TreeNode> _childs= new List<TreeNode>();

    public readonly string url;
    public readonly int depth;
    public TreeNode Parent { get; private set; }

    public TreeNode(string url, int curdepth=0)
    {
        this.url = url;
        this.depth = curdepth;
    }

    //public TreeNode GetChild(string id)
    //{
    //    return this._childs[id];
    //}

    public void Add(string url)
    {
        foreach (TreeNode _child in this._childs)
        {
            if (_child.url.ToLower() == url.ToLower())
                return;
        }
        TreeNode item= new TreeNode(url, this.depth+1);
        item.Parent = this;
        this._childs.Add(item);
    }

    //public IEnumerator<TreeNode> GetEnumerator()
    //{
    //    return this._childs.Values.GetEnumerator();
    //}

    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //    return this.GetEnumerator();
    //}

    public int Count
    {
        get { return this._childs.Count; }
    }

    public bool AddtoTree(string matchingUrl, string childurl)
    {
        var queue = new Queue<TreeNode>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            // Take the next node from the front of the queue
            var node = queue.Dequeue();

            // Process the node 'node'
            if (node.url.ToLower() == matchingUrl.ToLower())
            {
                node.Add(childurl);
                return true;
            }

            // Add the node’s children to the back of the queue
            foreach (var child in node._childs)
                queue.Enqueue(child);
        }

        // None of the nodes matched the specified predicate.
        return false;
    }

    public void PrintTree()
    {
        var stack = new Stack<TreeNode>();
        stack.Push(this);

        while (stack.Count > 0)
        {
            // Take the next node from the front of the queue
            var node = stack.Pop();

            for (int index = 1; index <= node.depth; index++)
            {
                Console.Write("\t");
            }

            Console.Write(String.Format("{0}\n", node.url));

            // Add the node’s children to the back of the queue
            foreach (var child in node._childs)
                stack.Push(child);
        }
    }
}