using ComputerInterface.Interfaces;
using SpectatorGUI;
using Zenject;

namespace ComputerModExample
{
    internal class MainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<IComputerModEntry>().To<ModEntry>().AsSingle();
        }
    }
}