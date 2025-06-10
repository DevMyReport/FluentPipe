using System.Collections.ObjectModel;
using FluentPipe.Blocks.Contracts;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Dynamic;

public sealed class ModuloDynamicBlock : DynamicPipeBlockBase<int, string>
{
    public override IPipeBuilder<int, string> GetBuilder(int input, bool isExplain)
    {
        if (isExplain || input % 2 == 0)
            return PipeBuilderFactory.Creer<int>()
                .Next<AddOneBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());

        return PipeBuilderFactory.Creer<int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());
    }
}

public sealed record ModuleOption(int Modulo = 2) : IPipeOption
{
    public IReadOnlyDictionary<string, string> Descrition()
    {
        return ReadOnlyDictionary<string, string>.Empty;
    }
}

public sealed class ModuleOptionBlock : DynamicPipeBlockBase<int, string, ModuleOption>
{
    public override IPipeBuilder<int, string> GetBuilder(int input, ModuleOption option, bool isExplain)
    {
        if (isExplain || input % option.Modulo == 0)
            return PipeBuilderFactory.Creer<int>()
                .Next<AddOneBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());

        return PipeBuilderFactory.Creer<int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());
    }
}

public sealed class AddMultiScopedOptionBlock : DynamicPipeBlockBase<int, string, ModuleOption>
{
    private readonly object _valueLock = new();
    private int _value;

    public override IPipeBuilder<int, string> GetBuilder(int input, ModuleOption option, bool isExplain)
    {
        lock (_valueLock)
        {
            _value += 1;
            IPipeBuilder<int, int> builder = PipeBuilderFactory.Creer<int>();

            for (var val = 0; val < _value; val++)
                builder = builder.Next<AddOneBlock, int>();
            return builder.Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());
        }
    }
}