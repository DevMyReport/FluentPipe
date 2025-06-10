namespace FluentPipe.Runner;

public class PipeRunnerComplet(IServiceProvider provider) : PipeRunnerBase<PipeErreurManager, PipeEtatManagerStateless, EnumEtapeEtat, EnumEtapeTrigger, PipeProgressionManager>(provider), IRunner;