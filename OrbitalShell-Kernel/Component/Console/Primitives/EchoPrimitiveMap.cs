namespace OrbitalShell.Component.Console.Primitives
{
    /// <summary>
    /// dynamically map echo(object) calls to external implementations
    /// <para>TO DO: implements mapping and conventions</para>
    /// </summary>
    public class EchoPrimitiveMap
    {
        public bool MappedCall(
            object obj,
            EchoEvaluationContext context
            )
        {
            if (obj == null)
            {
                context.Out.Echo(
                    EchoPrimitives.DumpNull(
                        context.CommandEvaluationContext
                        )
                    , context.Options.LineBreak);
                return true;
            }

            return false;
        }

    }
}
