using FluentPipe.Managers.Erreur;
using FluentPipe.Managers.Etat;

namespace FluentPipe.Runner;

public class PipeRunnerComplet(IServiceProvider provider) : PipeRunnerBase<PipeErreurManager, PipeEtatManager, EnumEtapeEtat, EnumEtapeDeclancheur, PipeProgressionManager>(provider), IPipeRunner;