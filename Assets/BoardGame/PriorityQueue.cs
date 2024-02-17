using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityQueue<T>
{
    struct Comparer : IComparer<(int Priority, T)> //Constructor for a comparer that compares 2 priorities and returns an index
    {
        public int Compare((int Priority, T) x, (int Priority, T) y)
        {
            return y.Priority - x.Priority;
        }
    }
    List<(int, T Item)> storageList; //Storage list for tiles and priorities

    public PriorityQueue()
    {
        storageList = new List<(int, T)>();//When called upon instantiates the storageList
    }

    public void Enqueue(int priority, T node)
    {
        var index = storageList.BinarySearch((priority, node), new Comparer()); //Creates an index from a priority and a tile with the help of the Comparer
        //If the index is positive then the tile is already in the queue
        //If it is negative then the index is in binary the inverted index of where the tile should be inserted
        if (index < 0) index = ~index; //inverts the index so that it shows where the tile should be inserted
        storageList.Insert(index, (priority, node)); //Inserts tile and priority at the index
    }

    public T Dequeue()
    {
        var index = storageList.Count - 1; //Index is equal to the last position in the queue
        var element = storageList[index]; //Element is equal to the element at the index in storagelist
        storageList.RemoveAt(index); //Element at index is removed from the storageList
        return element.Item; //Returns the removed element
    }

    public T GetCurrentTile()// Gets current tile I.E the last element in the storageList

    {
        var index = storageList.Count -1;//Gets the index of the last element in storageList
        return storageList[index].Item; //Returns The element at index
    }
    
    public bool QueueIsEmpty()
    {
        if(storageList.Count.Equals(0)) return true; //If the count of the storageList is = 0 then the list is empty and it returns true else it returs false
        return false;

    }

    public void PrintQueue()//If the list isn't empty the it prints all items and the index of each item in the storageList
    {
        if (storageList.Count != 0)
        {
            for (int i = 0; i < storageList.Count; i++)
            {
                Debug.Log(storageList[i].Item + "at index " + i);
            }
        }
    }
}
