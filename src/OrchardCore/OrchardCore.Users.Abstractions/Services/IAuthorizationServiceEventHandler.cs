using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore.Users.Services
{
    public interface IAuthorizationServiceEventHandler {
        void Checking(CheckAccessContext context);
        void Adjust(CheckAccessContext context);
        void Complete(CheckAccessContext context);
    }

    public class CheckAccessContext {
        public Permission Permission { get; set; }
        public IUser User { get; set; }
        public IContent Content { get; set; }
        
        // true if the permission has been granted to the user.
        public bool Granted { get; set; }
        
        // if context.Permission was modified during an Adjust(context) in an event handler, Adjusted should be set to true.
        // It means that the permission check will be done again by the framework.
        public bool Adjusted { get; set; }
    }
}
