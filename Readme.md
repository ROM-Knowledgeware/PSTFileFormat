PSTFileFormat is an open source library written in C# designed to read and write to Outlook PST files.

Usage Examples:
===============
List all folders in a PST file:

    public void ListAllFolders(PSTFile file)
    {
        tvFolders.Nodes.Clear();
        PSTFolder rootFolder = file.TopOfPersonalFolders;
        TreeNode rootNode = new TreeNode(rootFolder.DisplayName);
        rootNode.Name = rootFolder.NodeID.Value.ToString();
        tvFolders.Nodes.Add(rootNode);
        AddSubFolders(rootNode, rootFolder);
    }

    public void AddSubFolders(TreeNode node, PSTFolder folder)
    { 
        List<PSTFolder> childFolders = folder.GetChildFolders();
        foreach(PSTFolder childFolder in childFolders)
        {
            TreeNode childNode = new TreeNode(childFolder.DisplayName + " (" + childFolder.ItemType.ToString() + ")" );
            childNode.Name = childFolder.NodeID.Value.ToString();
            node.Nodes.Add(childNode);
            AddSubFolders(childNode, childFolder);
        }
    }

Technical References:
=====================
1. https://msdn.microsoft.com/en-us/library/ff385210(v=office.12).aspx

2. https://blogs.msdn.microsoft.com/openspecification/2010/11/30/ms-pst-how-to-navigate-the-node-btree/

3. https://blogs.msdn.microsoft.com/openspecification/2011/02/11/ms-pst-the-relationship-between-nodes-and-blocks/

License:
========
PSTFileFormat is licensed under the GNU Lesser Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
