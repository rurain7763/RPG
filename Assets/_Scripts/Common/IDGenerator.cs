using System.Collections.Generic;

public class IDGenerator
{
    private ulong currentID = 0;
    private Queue<ulong> freeQueue = new();

    public ulong GenerateID()
    {
        if (freeQueue.Count > 0)
        {
            return freeQueue.Dequeue();
        }

        return ++currentID;
    }

    public void FreeID(ulong id)
    {
        freeQueue.Enqueue(id);
    }
}