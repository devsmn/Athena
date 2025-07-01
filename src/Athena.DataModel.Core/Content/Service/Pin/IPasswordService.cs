using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel.Core
{
    public interface IPasswordService
    {
        /// <summary>
        /// Requests the user to enter a new password.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="onNewPasswordEntered"></param>
        /// <returns></returns>
        Task New(IContext context, Action<string> onNewPasswordEntered);

        /// <summary>
        /// Prompts the user to enter a password.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="passwordEntered"></param>
        /// <returns></returns>
        Task Prompt(IContext context, Action<string> passwordEntered);
    }
}
