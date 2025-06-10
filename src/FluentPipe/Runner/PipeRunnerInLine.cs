using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Runner;

public sealed record SortieRunner<TOut, TError>(TOut? Sortie, IReadOnlyList<ProcessStep> Etapes, TError ErrorManager, TimeSpan DureeComplete);

public class PipeRunnerInLine(IServiceProvider provider) : PipeRunnerBase<PipeErreurManager>(provider), IRunner;