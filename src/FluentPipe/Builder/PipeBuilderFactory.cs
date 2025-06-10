namespace FluentPipe.Builder;

public static class PipeBuilderFactory
{
    public static PipeBuilder<TLocalSortie, TLocalSortie> Creer<TLocalSortie>()
    {
        return new PipeBuilder<TLocalSortie, TLocalSortie>();
    }

    public static PipeParallelBuilder<IEnumerable<TLocalSortie>, IEnumerable<TLocalSortie>, TLocalSortie> CreerEnumerable<TLocalSortie>()
    {
        return new PipeParallelBuilder<IEnumerable<TLocalSortie>, IEnumerable<TLocalSortie>, TLocalSortie>();
    }
}