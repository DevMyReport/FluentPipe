using FluentPipe.Blocks.Contracts;
using FluentPipe.Managers.Erreur;

namespace FluentPipe.Runner;


public class PipeRunnerInLine(IServiceProvider provider) : PipeRunnerBase<PipeErreurManager>(provider), IPipeRunner;