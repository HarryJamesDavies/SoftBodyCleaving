using Unity.Jobs;

public struct TestJob : IJobParallelFor
{
    public int count;

    public void Execute(int _index)
    {
        count++;
    }

}
