using System.Collections.Generic;

/********************************************************************************************
 * A Generic LinkedList<Enemy> class, and a sorted version, that uses a shared free node
 * list to prevent constant node allocation and deletion as enemies frequently move between
 * lists.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public class EnemyComparerId : Comparer<Enemy>              // Compare by enemy id
    {
        public override int Compare(Enemy x, Enemy y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }

    public class EnemyComparerRespawnTime : Comparer<Enemy>    // Compare enemies by respawn time left
    {
        public override int Compare(Enemy x, Enemy y)
        {
            return x.TimeTillDeathRespawn.CompareTo(y.TimeTillDeathRespawn);
        }
    }


    /********************************************************************
     * Generic LinkedList<Enemy> class that also contains a static free
     * node list shared by all EnemyLists to prevent LinkedListNodes from 
     * being constantly created and destroyed.
     ********************************************************************/
    public class EnemyList
    {
        protected LinkedList<Enemy> enemyList;      // All of the enemies in this list
        static LinkedList<Enemy> freeNodeList;      // Shared pool of free LinkedListNodes

        public int Count
        {
            get { return enemyList.Count; }
        }

        public LinkedListNode<Enemy> First
        {
            get { return enemyList.First; }
        }

        /********************************************************************
         * Initialize enemyList, and free list if not already done (since
         * there can be multiple spawners)
         ********************************************************************/
        public EnemyList()
        {
            enemyList = new LinkedList<Enemy>();
            if (freeNodeList == null)
                freeNodeList = new LinkedList<Enemy>();
        }

        /********************************************************************
         * Add the enemy to the front of enemyList, using a free node.
         * 
         * @param   enemy       Enemy to add
         ********************************************************************/
        public void AddFirst(Enemy enemy)
        {
            LinkedListNode<Enemy> node = GetFreeNode();
            node.Value = enemy;
            enemyList.AddFirst(node);
        }

        /********************************************************************
         * Add the enemy to the end of enemyList, using a free node.
         * 
         * @param   enemy       Enemy to add
         ********************************************************************/
        public void AddLast(Enemy enemy)
        {
            LinkedListNode<Enemy> node = GetFreeNode();
            node.Value = enemy;
            enemyList.AddLast(node);
        }

        /********************************************************************
         * Removes and returns the enemy at the front of enemyList.
         * 
         * @return              Enemy at the front of the list (null if empty)
         ********************************************************************/
        public Enemy RemoveFirst()
        {
            return RetrieveEnemy(enemyList, RetrieveFirstNode);
        }

        /********************************************************************
         * Removes and returns the enemy at the end of enemyList.
         * 
         * @return              Enemy at the front of the list (null if empty)
         ********************************************************************/
        public Enemy RemoveLast()
        {
            return RetrieveEnemy(enemyList, RetrieveLastNode);
        }

        /********************************************************************
         * Removes and returns the enemy by searching for a matching enemy id.
         * 
         * @return              Enemy with matching id (null if empty list or enemy id not found)
         ********************************************************************/
        public Enemy RemoveBySearch(Enemy enemy)
        {
            return RetrieveEnemy(enemyList, enemy, RetrieveNodeBySearch);
        }

        /********************************************************************
         * Add a defined amount of LinkedListNode<Enemy> to the freeNodeList.
         * Used to create an initial pool at the start of the game.
         * 
         * @param   quantity    Number of nodes to create
         ********************************************************************/
        public static void AddNewFreeListNodes(int quantity)
        {
            for (int i = 0; i < quantity; ++i)
            {
                freeNodeList.AddLast(new LinkedListNode<Enemy>(null));
            }
        }

        /********************************************************************
         * Returns the node at the front of freeNodeList.  If the list is empty,
         * a new node is created and returned.
         * 
         * @return              Empty LinkedListNode<Enemy>
         ********************************************************************/
        protected LinkedListNode<Enemy> GetFreeNode()
        {
            if (freeNodeList.Count.Equals(0))
                return new LinkedListNode<Enemy>(null);

            return RetrieveFirstNode(freeNodeList, null);
        }

        /********************************************************************
         * Clears the node's value and places it at the end of the freeNodeList.
         * 
         * @param   node        The node to add
         ********************************************************************/
        protected void AddFreeNode(LinkedListNode<Enemy> node)
        {
            node.Value = null;
            freeNodeList.AddLast(node);
        }

        /********************************************************************
         * Delegate for functions that remove a LinkedListNode<Enemy> from a list.
         * 
         * @param   list    List to remove from
         * @param   enemy   Enemy to remove (may be unused depending on function)
         * @return          The node removed from the list
         ********************************************************************/
        private delegate LinkedListNode<Enemy> RemoveNode(LinkedList<Enemy> list, Enemy enemy);

        /********************************************************************
         * Wrapper for RetrieveEnemy(LinkedList<Enemy> list, Enemy enemy, RemoveNode removeNodeFunc)
         ********************************************************************/
        private Enemy RetrieveEnemy(LinkedList<Enemy> list, RemoveNode removeNodeFunc)
        {
            return RetrieveEnemy(list, null, removeNodeFunc);
        }

        /********************************************************************
         * Removes an enemy from the list and returns it.  The LinkedListNode
         * that held the enemy is placed back in the freeNodeList.  This also uses
         * the delegate function to choose which node to remove (and remove it).
         * 
         * @param   list            The list to remove from
         * @param   enemy           Enemy used in removal by search (can be null otherwise)
         * @param   removeNodeFunc  The function that chooses which node to remove (and removes it)
         * @return                  The enemy removed (null if not found or empty list)
         ********************************************************************/
        private Enemy RetrieveEnemy(LinkedList<Enemy> list, Enemy enemy, RemoveNode removeNodeFunc)
        {
            if (list.Count.Equals(0))
                return null;

            LinkedListNode<Enemy> node = removeNodeFunc(list, enemy);
            if (node == null)
                return null;

            Enemy enemyRetrieved = node.Value;
            AddFreeNode(node);
            return enemyRetrieved;
        }

        /********************************************************************
         * Removes and returns the node from front of the list.  Expects list
         * to be non-empty.
         * 
         * @param   list        The list to remove from
         * @param   enemy       Unused
         * @return              Node at the front of the list
         ********************************************************************/
        private LinkedListNode<Enemy> RetrieveFirstNode(LinkedList<Enemy> list, Enemy enemy)
        {
            LinkedListNode<Enemy> node = list.First;
            list.RemoveFirst();
            return node;
        }

        /********************************************************************
         * Removes and returns the node from end of the list.  Expects list
         * to be non-empty.
         * 
         * @param   list        The list to remove from
         * @param   enemy       Unused
         * @return              Node at the end of the list
         ********************************************************************/
        private LinkedListNode<Enemy> RetrieveLastNode(LinkedList<Enemy> list, Enemy enemy)
        {
            LinkedListNode<Enemy> node = list.Last;
            list.RemoveLast();
            return node;
        }

        /********************************************************************
         * Removes and returns node with matching enemy id.  Expects list to
         * be non-empty.
         * 
         * @param   list        The list to remove from
         * @param   enemy       Enemy with the id we are searching for
         * @return              Node with matching id (null if not found)
         ********************************************************************/
        private LinkedListNode<Enemy> RetrieveNodeBySearch(LinkedList<Enemy> list, Enemy enemy)
        {
            if (enemy == null)
                return null;

            LinkedListNode<Enemy> current = list.First;
            while(current != null)
            {
                if (current.Value.Id.Equals(enemy.Id))
                {
                    list.Remove(current);
                    return current;
                }
                current = current.Next;
            }
            return null;
        }
    }


    /********************************************************************
     * Sorted version of EnemyList that uses an Comparer<Enemy> comparator
     * to perform sorting on insert.
     ********************************************************************/
    public class SortedEnemyList : EnemyList
    {
        Comparer<Enemy> comparer;

        /********************************************************************
         * SortedEnemyList with default sorting by enemy id.
         ********************************************************************/
        public SortedEnemyList() : base()
        {
            comparer = new EnemyComparerId();
        }

        /********************************************************************
         * SortedEnemyList with sorting by given comparator.
         ********************************************************************/
        public SortedEnemyList(Comparer<Enemy> comparer) : base()
        {
            this.comparer = comparer;
        }

        /********************************************************************
         * Inserts the enemy into the list in sorted order.  This requires the
         * list to already be sorted.  Sorting is performed by the comparer in
         * this class.
         * 
         * @param   enemy       The enemy to add
         ********************************************************************/
        public void AddSorted(Enemy enemy)
        {
            if (enemyList.Count.Equals(0))
            {
                AddFirst(enemy);
                return;
            }

            LinkedListNode<Enemy> current = enemyList.First;
            while (current != null && comparer.Compare(enemy, current.Value) > 0)
            {
                current = current.Next;
            }

            LinkedListNode<Enemy> node = GetFreeNode();
            node.Value = enemy;
            if (current == null)
                enemyList.AddLast(node);
            else
                enemyList.AddBefore(current, enemy);
        }
    }
}