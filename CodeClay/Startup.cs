using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CodeClay.Startup))]
namespace CodeClay
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
