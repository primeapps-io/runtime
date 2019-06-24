using McMaster.Extensions.CommandLineUtils;
namespace PrimeApps.CLI
{
    /// <summary>
    /// <see cref="HelpOptionAttribute"/> must be declared on each type that supports '--help'.
    /// Compare to the inheritance example, in which <see cref="GitCommandBase"/> delcares it
    /// once so that all subcommand types automatically support '--help'.
    /// </summary>
    [Command("auth", Description = "Manages authentication"), Subcommand(typeof(Login))]
    [HelpOption]
    public class Auth
    {
        private int OnExecute(IConsole console)
        {
            console.Error.WriteLine("You must specify an action. See --help for more details.");
            return 1;
        }
    }
}