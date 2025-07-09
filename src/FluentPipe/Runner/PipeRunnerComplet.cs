using FluentPipe.Managers.Erreur;
using FluentPipe.Managers.Etat;

namespace FluentPipe.Runner;

public class PipeRunnerComplet(IServiceProvider provider) : PipeRunnerBase<PipeErreurManager, PipeEtatManager, EnumMachineEtat, EnumMachineDeclancheur, PipeProgressionManager>(provider), IPipeRunnerAvecEtatEtProgression, IPipeRunner;