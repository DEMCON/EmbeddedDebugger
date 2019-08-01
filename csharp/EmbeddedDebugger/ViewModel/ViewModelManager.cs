using EmbeddedDebugger.Model;
using System;

namespace EmbeddedDebugger.ViewModel
{
    public class ViewModelManager
    {
        private static bool IsInstance = false;
        public ModelManager ModelManager { get; private set; }

        public SystemViewModel SystemViewModel { get; private set; }


        public ViewModelManager()
        {
            if (IsInstance) throw new ArgumentException("ViewModelManager already exists");
            // TODO remove static instance, maybe make it singleton
            IsInstance = true;
            ModelManager = new ModelManager();
            SystemViewModel = new SystemViewModel(ModelManager);
        }

    }
}
